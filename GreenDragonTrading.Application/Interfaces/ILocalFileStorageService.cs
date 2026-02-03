using Microsoft.AspNetCore.Http;

namespace GreenDragonTrading.Application.Interfaces;

/// <summary>
/// Local file storage service interface
/// </summary>
public interface ILocalFileStorageService
{
    /// <summary>
    /// Save file to local storage
    /// </summary>
    /// <param name="file">File to save</param>
    /// <param name="category">File category</param>
    /// <param name="originalFileName">Original file name</param>
    /// <param name="relatedEntityId">Related entity ID (optional, for subdirectory naming)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Relative file path</returns>
    Task<string> SaveFileAsync(
        IFormFile file, 
        Domain.Enums.FileCategory category, 
        string originalFileName,
        string? relatedEntityId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get file stream for download
    /// </summary>
    /// <param name="filePath">Relative file path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File stream</returns>
    Task<Stream> GetFileStreamAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete file from local storage
    /// </summary>
    /// <param name="filePath">Relative file path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if file exists
    /// </summary>
    /// <param name="filePath">Relative file path</param>
    /// <returns>True if exists</returns>
    bool FileExists(string filePath);

    /// <summary>
    /// Get subdirectory for file category
    /// </summary>
    /// <param name="category">File category</param>
    /// <returns>Subdirectory name</returns>
    string GetCategoryDirectory(Domain.Enums.FileCategory category);
}
