using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Application.DTOs;

/// <summary>
/// File upload request DTO
/// </summary>
public class FileUploadDto
{
    /// <summary>
    /// File category
    /// </summary>
    public FileCategory Category { get; set; }

    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Related entity ID (e.g., FinancialReport ID, Symbol ticker)
    /// </summary>
    public string? RelatedEntityId { get; set; }
}

/// <summary>
/// File response DTO
/// </summary>
public class FileResponseDto
{
    /// <summary>
    /// File ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Original file name
    /// </summary>
    public string OriginalFileName { get; set; } = null!;

    /// <summary>
    /// File category
    /// </summary>
    public FileCategory Category { get; set; }

    /// <summary>
    /// File type
    /// </summary>
    public FileType FileType { get; set; }

    /// <summary>
    /// File extension
    /// </summary>
    public string FileExtension { get; set; } = null!;

    /// <summary>
    /// MIME type
    /// </summary>
    public string MimeType { get; set; } = null!;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Related entity ID
    /// </summary>
    public string? RelatedEntityId { get; set; }

    /// <summary>
    /// Related entity type
    /// </summary>
    public string? RelatedEntityType { get; set; }

    /// <summary>
    /// Uploader ID
    /// </summary>
    public Guid? UploadedBy { get; set; }

    /// <summary>
    /// Uploader name
    /// </summary>
    public string? UploaderName { get; set; }

    /// <summary>
    /// File status
    /// </summary>
    public CommonStatus Status { get; set; }

    /// <summary>
    /// Created date
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Updated date
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Download URL (relative)
    /// </summary>
    public string DownloadUrl { get; set; } = null!;
}

/// <summary>
/// File update request DTO
/// </summary>
public class FileUpdateDto
{
    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Related entity ID
    /// </summary>
    public string? RelatedEntityId { get; set; }

    /// <summary>
    /// Related entity type
    /// </summary>
    public string? RelatedEntityType { get; set; }

    /// <summary>
    /// File status
    /// </summary>
    public CommonStatus? Status { get; set; }
}

/// <summary>
/// File download result
/// </summary>
public class FileDownloadResult
{
    /// <summary>
    /// File stream
    /// </summary>
    public Stream FileStream { get; set; } = null!;

    /// <summary>
    /// File name
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// MIME type
    /// </summary>
    public string MimeType { get; set; } = null!;
}
