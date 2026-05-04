using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.Services;

/// <summary>
/// Local file storage service implementation
/// </summary>
public class LocalFileStorageService : ILocalFileStorageService
{
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly string _baseStoragePath;

    public LocalFileStorageService(ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;
        
        // Get application root directory
        var appRoot = Directory.GetCurrentDirectory();
        _baseStoragePath = Path.Combine(appRoot, FileConstants.BASE_STORAGE_PATH);

        // Ensure base directory exists
        EnsureDirectoryExists(_baseStoragePath);
    }

    public async Task<string> SaveFileAsync(
        IFormFile file, 
        Domain.Enums.FileCategory category, 
        string originalFileName,
        string? relatedEntityId = null,
        string? ticker = null,
        int? year = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get category-specific subdirectory
            var categoryDir = GetCategoryDirectory(category);
            
            // For financial reports, add Ticker/Year subdirectory structure
            if (category == Domain.Enums.FileCategory.FinancialReport && !string.IsNullOrEmpty(ticker) && year.HasValue)
            {
                categoryDir = Path.Combine(categoryDir, ticker.ToUpper(), year.Value.ToString());
            }
            // For analysis reports, add Category subdirectory structure
            else if (category == Domain.Enums.FileCategory.AnalysisReport && !string.IsNullOrEmpty(ticker))
            {
                // ticker parameter is used for category code in analysis reports
                categoryDir = Path.Combine(categoryDir, ticker);
            }
            
            var fullDirectoryPath = Path.Combine(_baseStoragePath, categoryDir);
            
            // Ensure directory exists
            EnsureDirectoryExists(fullDirectoryPath);

            // Generate unique file name
            var uniqueFileName = GenerateUniqueFileName(originalFileName, relatedEntityId);
            var fullFilePath = Path.Combine(fullDirectoryPath, uniqueFileName);

            // Save file to disk
            using (var stream = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            // Return relative path for database storage
            var relativePath = Path.Combine(categoryDir, uniqueFileName);
            
            _logger.LogInformation("File saved successfully: {FilePath}", relativePath);
            
            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file: {FileName}", originalFileName);
            throw;
        }
    }

    public Task<Stream> GetFileStreamAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = Path.Combine(_baseStoragePath, filePath);
            
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
            
            _logger.LogInformation("File stream opened: {FilePath}", filePath);
            
            return Task.FromResult<Stream>(stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file stream: {FilePath}", filePath);
            throw;
        }
    }

    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullPath = Path.Combine(_baseStoragePath, filePath);
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted: {FilePath}", filePath);
            }
            else
            {
                _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            throw;
        }
    }

    public bool FileExists(string filePath)
    {
        var fullPath = Path.Combine(_baseStoragePath, filePath);
        return File.Exists(fullPath);
    }

    public string GetCategoryDirectory(Domain.Enums.FileCategory category)
    {
        return category switch
        {
            Domain.Enums.FileCategory.FinancialReport => FileConstants.FINANCIAL_REPORTS_DIR,
            Domain.Enums.FileCategory.AnalysisReport => FileConstants.ANALYSIS_REPORTS_DIR,
            Domain.Enums.FileCategory.Avatar => FileConstants.AVATARS_DIR,
            Domain.Enums.FileCategory.CompanyLogo => FileConstants.COMPANY_LOGOS_DIR,
            _ => throw new ArgumentException($"Invalid file category: {category}")
        };
    }

    private void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            _logger.LogInformation("Directory created: {Path}", path);
        }
    }

    private string GenerateUniqueFileName(string originalFileName, string? relatedEntityId = null)
    {
        var extension = Path.GetExtension(originalFileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        
        // Sanitize file name
        var sanitizedName = SanitizeFileName(fileNameWithoutExtension);
        
        // Generate unique name with timestamp and optional entity ID
        var uniqueName = relatedEntityId != null
            ? $"{sanitizedName}_{relatedEntityId}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}"
            : $"{sanitizedName}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
        
        return uniqueName;
    }

    private string SanitizeFileName(string fileName)
    {
        // Remove invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        
        // Limit length
        if (sanitized.Length > 50)
        {
            sanitized = sanitized.Substring(0, 50);
        }
        
        return sanitized;
    }
}
