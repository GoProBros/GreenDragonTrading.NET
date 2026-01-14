using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace GreenDragonTrading.Infrastructure.Services
{
    /// <summary>
    /// Implementation of Google Drive service for file operations
    /// </summary>
    public class GoogleDriveService : IGoogleDriveService
    {
        private readonly GoogleDriveOptions _options;
        private readonly ILogger<GoogleDriveService> _logger;
        private readonly DriveService _driveService;
        private readonly Dictionary<string, string> _folderCache = new();

        public GoogleDriveService(
            IOptions<GoogleDriveOptions> options,
            ILogger<GoogleDriveService> logger)
        {
            _options = options.Value;
            _logger = logger;

            // Initialize Drive service with service account credentials
            GoogleCredential credential;

            if (!string.IsNullOrEmpty(_options.JsonCredentials))
            {
                // Use JSON credentials from environment variable (recommended)
                _logger.LogInformation("Initializing Google Drive with JSON credentials from environment variable");
                var credentialBytes = Encoding.UTF8.GetBytes(_options.JsonCredentials);
                using var stream = new MemoryStream(credentialBytes);
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(DriveService.ScopeConstants.Drive);
            }
            else if (!string.IsNullOrEmpty(_options.ServiceAccountKeyPath))
            {
                // Fallback to file path
                _logger.LogInformation("Initializing Google Drive with service account key file: {Path}", _options.ServiceAccountKeyPath);
                credential = GoogleCredential.FromFile(_options.ServiceAccountKeyPath)
                    .CreateScoped(DriveService.ScopeConstants.Drive);
            }
            else
            {
                throw new InvalidOperationException(
                    "Google Drive credentials not configured. Provide either JsonCredentials or ServiceAccountKeyPath.");
            }

            _driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = _options.ApplicationName
            });
        }

        public async Task<(string FileId, string FileUrl)> UploadFileAsync(
            IFormFile file,
            string fileName,
            string folderType,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
            {
                throw new InvalidOperationException("Google Drive integration is disabled");
            }

            if (file.Length > _options.MaxFileSizeBytes)
            {
                throw new InvalidOperationException(
                    $"File size exceeds maximum allowed size of {_options.MaxFileSizeBytes / 1024 / 1024}MB");
            }

            using var stream = file.OpenReadStream();
            return await UploadFileFromStreamAsync(
                stream, 
                fileName, 
                file.ContentType, 
                folderType, 
                cancellationToken);
        }

        public async Task<(string FileId, string FileUrl)> UploadFileFromStreamAsync(
            Stream stream,
            string fileName,
            string mimeType,
            string folderType,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
            {
                throw new InvalidOperationException("Google Drive integration is disabled");
            }

            try
            {
                // Get or create folder for the specified type
                var folderId = await GetOrCreateFolderAsync(folderType, cancellationToken);

                var fileMetadata = new Google.Apis.Drive.v3.Data.File
                {
                    Name = fileName,
                    Parents = new List<string> { folderId }
                };

                var request = _driveService.Files.Create(fileMetadata, stream, mimeType);
                request.Fields = "id, webViewLink, webContentLink";
                request.SupportsAllDrives = true;
                request.SupportsTeamDrives = true;

                _logger.LogInformation("Uploading file {FileName} to Google Drive folder type {FolderType} (ID: {FolderId})", 
                    fileName, folderType, folderId);

                var progress = await request.UploadAsync(cancellationToken);

                if (progress.Status == UploadStatus.Failed)
                {
                    _logger.LogError(progress.Exception, "Failed to upload file {FileName} to Google Drive", fileName);
                    throw new Exception($"Upload failed: {progress.Exception?.Message}", progress.Exception);
                }

                var uploadedFile = request.ResponseBody;
                
                // Make file publicly accessible (anyone with link can view)
                var shareableUrl = await CreateShareableLinkAsync(uploadedFile.Id, cancellationToken);

                _logger.LogInformation("Successfully uploaded file {FileName} with ID {FileId}", 
                    fileName, uploadedFile.Id);

                return (uploadedFile.Id, shareableUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName} to Google Drive", fileName);
                throw;
            }
        }

        public async Task<Stream> DownloadFileAsync(string fileId, CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
            {
                throw new InvalidOperationException("Google Drive integration is disabled");
            }

            try
            {
                _logger.LogInformation("Downloading file {FileId} from Google Drive", fileId);

                var request = _driveService.Files.Get(fileId);
                request.SupportsAllDrives = true;
                request.SupportsTeamDrives = true;
                var stream = new MemoryStream();

                await request.DownloadAsync(stream, cancellationToken);
                stream.Position = 0;

                _logger.LogInformation("Successfully downloaded file {FileId}", fileId);

                return stream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file {FileId} from Google Drive", fileId);
                throw;
            }
        }

        public async Task DeleteFileAsync(string fileId, CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
            {
                throw new InvalidOperationException("Google Drive integration is disabled");
            }

            try
            {
                _logger.LogInformation("Deleting file {FileId} from Google Drive", fileId);

                var deleteRequest = _driveService.Files.Delete(fileId);
                deleteRequest.SupportsAllDrives = true;
                deleteRequest.SupportsTeamDrives = true;
                await deleteRequest.ExecuteAsync(cancellationToken);

                _logger.LogInformation("Successfully deleted file {FileId}", fileId);
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("File {FileId} not found on Google Drive, skipping deletion", fileId);
                // File already deleted or doesn't exist - not an error
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileId} from Google Drive", fileId);
                throw;
            }
        }

        public async Task<(string Name, long Size, string MimeType)> GetFileMetadataAsync(
            string fileId,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
            {
                throw new InvalidOperationException("Google Drive integration is disabled");
            }

            try
            {
                _logger.LogInformation("Getting metadata for file {FileId} from Google Drive", fileId);

                var request = _driveService.Files.Get(fileId);
                request.Fields = "name, size, mimeType";
                request.SupportsAllDrives = true;
                request.SupportsTeamDrives = true;

                var file = await request.ExecuteAsync(cancellationToken);

                return (file.Name, file.Size ?? 0, file.MimeType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metadata for file {FileId} from Google Drive", fileId);
                throw;
            }
        }

        public async Task<string> CreateShareableLinkAsync(string fileId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Create permission for anyone with the link to view
                var permission = new Google.Apis.Drive.v3.Data.Permission
                {
                    Type = "anyone",
                    Role = "reader"
                };

                var permissionRequest = _driveService.Permissions.Create(permission, fileId);
                permissionRequest.SupportsAllDrives = true;
                permissionRequest.SupportsTeamDrives = true;
                await permissionRequest.ExecuteAsync(cancellationToken);

                // Get the web view link
                var request = _driveService.Files.Get(fileId);
                request.Fields = "webViewLink";
                request.SupportsAllDrives = true;
                request.SupportsTeamDrives = true;
                var file = await request.ExecuteAsync(cancellationToken);

                _logger.LogInformation("Created shareable link for file {FileId}", fileId);

                return file.WebViewLink;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating shareable link for file {FileId}", fileId);
                throw;
            }
        }

        public async Task<string> GetOrCreateFolderAsync(string folderType, CancellationToken cancellationToken = default)
        {
            // Check cache first
            if (_folderCache.TryGetValue(folderType, out var cachedFolderId))
            {
                return cachedFolderId;
            }

            // Check if folder ID is configured
            if (_options.Folders.TryGetValue(folderType, out var configuredFolderId) && !string.IsNullOrEmpty(configuredFolderId))
            {
                _folderCache[folderType] = configuredFolderId;
                return configuredFolderId;
            }

            // Check if RootFolderId is configured
            if (string.IsNullOrEmpty(_options.RootFolderId) || _options.RootFolderId == "YOUR_ROOT_FOLDER_ID")
            {
                throw new InvalidOperationException(
                    "Google Drive RootFolderId is not configured. " +
                    "Please set GoogleDrive:RootFolderId in appsettings.json or environment variable.");
            }

            // Auto-create folder if enabled
            if (_options.AutoCreateFolders)
            {
                _logger.LogInformation("Auto-creating folder for type {FolderType} in root folder {RootFolderId}", 
                    folderType, _options.RootFolderId);

                var folderId = await CreateFolderAsync(folderType, _options.RootFolderId, cancellationToken);
                _folderCache[folderType] = folderId;
                
                _logger.LogWarning("Created folder {FolderType} with ID {FolderId}. Consider adding this to configuration.", 
                    folderType, folderId);
                
                return folderId;
            }

            throw new InvalidOperationException(
                $"Folder type '{folderType}' is not configured and AutoCreateFolders is disabled. " +
                $"Add folder ID to GoogleDrive:Folders:{folderType} in configuration.");
        }

        public async Task<string> CreateFolderAsync(
            string folderName, 
            string parentFolderId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var folderMetadata = new Google.Apis.Drive.v3.Data.File
                {
                    Name = folderName,
                    MimeType = "application/vnd.google-apps.folder",
                    Parents = new List<string> { parentFolderId }
                };

                var request = _driveService.Files.Create(folderMetadata);
                request.Fields = "id, name";
                request.SupportsAllDrives = true;
                request.SupportsTeamDrives = true;

                var folder = await request.ExecuteAsync(cancellationToken);

                _logger.LogInformation("Created folder {FolderName} with ID {FolderId} in parent {ParentId}", 
                    folderName, folder.Id, parentFolderId);

                return folder.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating folder {FolderName} in Google Drive", folderName);
                throw;
            }
        }
    }
}
