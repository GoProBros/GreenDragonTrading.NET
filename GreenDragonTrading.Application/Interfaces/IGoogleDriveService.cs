using Microsoft.AspNetCore.Http;

namespace GreenDragonTrading.Application.Interfaces
{
    /// <summary>
    /// Service interface for Google Drive operations
    /// </summary>
    public interface IGoogleDriveService
    {
        /// <summary>
        /// Upload a file to Google Drive
        /// </summary>
        /// <param name="file">File to upload</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="folderType">Folder type (from DriveFolderConstants)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Tuple containing file ID and shareable URL</returns>
        Task<(string FileId, string FileUrl)> UploadFileAsync(
            IFormFile file, 
            string fileName, 
            string folderType,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Upload a file to Google Drive from stream
        /// </summary>
        /// <param name="stream">File stream</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="mimeType">MIME type of the file</param>
        /// <param name="folderType">Folder type (from DriveFolderConstants)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Tuple containing file ID and shareable URL</returns>
        Task<(string FileId, string FileUrl)> UploadFileFromStreamAsync(
            Stream stream,
            string fileName,
            string mimeType,
            string folderType,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Download a file from Google Drive
        /// </summary>
        /// <param name="fileId">Google Drive file ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>File stream</returns>
        Task<Stream> DownloadFileAsync(string fileId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a file from Google Drive
        /// </summary>
        /// <param name="fileId">Google Drive file ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task DeleteFileAsync(string fileId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get file metadata from Google Drive
        /// </summary>
        /// <param name="fileId">Google Drive file ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>File metadata including name, size, and MIME type</returns>
        Task<(string Name, long Size, string MimeType)> GetFileMetadataAsync(
            string fileId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a shareable link for a file
        /// </summary>
        /// <param name="fileId">Google Drive file ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Shareable URL</returns>
        Task<string> CreateShareableLinkAsync(string fileId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get or create folder by type
        /// </summary>
        /// <param name="folderType">Folder type (from DriveFolderConstants)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Folder ID</returns>
        Task<string> GetOrCreateFolderAsync(string folderType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a folder in Google Drive
        /// </summary>
        /// <param name="folderName">Name of the folder</param>
        /// <param name="parentFolderId">Parent folder ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created folder ID</returns>
        Task<string> CreateFolderAsync(string folderName, string parentFolderId, CancellationToken cancellationToken = default);
    }
}
