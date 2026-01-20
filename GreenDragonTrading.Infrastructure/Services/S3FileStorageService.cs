using Amazon.S3;
using Amazon.S3.Model;
using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GreenDragonTrading.Infrastructure.Services
{
    /// <summary>
    /// Cloudflare R2 file storage implementation (S3-compatible API).
    /// Supports any file type: financial reports, avatars, documents, company logos, etc.
    /// Use FileStorageConstants to maintain consistent folder structure.
    /// </summary>
    public class S3FileStorageService : IFileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _endpoint;
        private readonly ILogger<S3FileStorageService> _logger;

        public S3FileStorageService(
            IAmazonS3 s3Client,
            IOptions<R2Options> r2Options,
            ILogger<S3FileStorageService> logger)
        {
            _s3Client = s3Client;
            _logger = logger;
            var options = r2Options.Value;
            _bucketName = options.BucketName 
                ?? throw new ArgumentNullException(nameof(options.BucketName), "R2 BucketName is required");
            _endpoint = options.Endpoint;
        }

        public async Task<(string FilePath, long FileSize)> UploadAsync(
            Stream stream,
            string fileName,
            string folder,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var extension = Path.GetExtension(fileName);
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var key = $"{folder}/{uniqueFileName}".Replace("\\", "/");
                var fileSize = stream.Length;

                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = stream,
                    ContentType = contentType,
                    Metadata =
                    {
                        ["original-filename"] = fileName,
                        ["uploaded-at"] = DateTimeOffset.UtcNow.ToString("O")
                    },
                    DisablePayloadSigning = true,
                    UseChunkEncoding = false
                };

                _ = await _s3Client.PutObjectAsync(request, cancellationToken);

                _logger.LogInformation(
                    "Uploaded file to Cloudflare R2: {FileName} -> r2://{Bucket}/{Key} ({FileSize} bytes)",
                    fileName, _bucketName, key, fileSize);

                return (key, fileSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to Cloudflare R2: {FileName}", fileName);
                throw;
            }
        }

        public async Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = filePath
                };

                await _s3Client.DeleteObjectAsync(request, cancellationToken);
                
                _logger.LogInformation("Deleted file from Cloudflare R2: r2://{Bucket}/{Key}", _bucketName, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file from Cloudflare R2: {FilePath}", filePath);
                throw;
            }
        }

        public string GetFileUrl(string filePath)
        {
            return $"https://{_bucketName}.{_endpoint.Split('.')[0].Replace("https://", "")}.r2.dev/{filePath}";
        }

        public async Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = filePath
                };

                await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        public async Task<Stream> GetFileStreamAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = filePath
                };

                var response = await _s3Client.GetObjectAsync(request, cancellationToken);
                return response.ResponseStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file stream from Cloudflare R2: {FilePath}", filePath);
                throw;
            }
        }
    }
}
