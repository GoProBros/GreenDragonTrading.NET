namespace GreenDragonTrading.Application.Interfaces
{
    /// <summary>
    /// File storage service abstraction (supports Local, AWS S3, Cloudflare R2, Azure Blob, etc.).
    /// Generic interface for storing any type of files: financial reports, avatars, documents, logos, etc.
    /// Use FileStorageConstants for consistent folder structure.
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Upload file and return storage metadata
        /// </summary>
        /// <param name="stream">File stream</param>
        /// <param name="fileName">Original file name</param>
        /// <param name="folder">Storage folder path</param>
        /// <param name="contentType">MIME content type</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Tuple of (FilePath, FileSize)</returns>
        Task<(string FilePath, long FileSize)> UploadAsync(
            Stream stream,
            string fileName,
            string folder,
            string contentType,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete file from storage
        /// </summary>
        Task DeleteAsync(string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get public URL for file download
        /// </summary>
        string GetFileUrl(string filePath);

        /// <summary>
        /// Check if file exists in storage
        /// </summary>
        Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get file stream for reading
        /// </summary>
        Task<Stream> GetFileStreamAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
