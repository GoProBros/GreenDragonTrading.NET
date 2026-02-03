using GreenDragonTrading.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace GreenDragonTrading.Application.DTOs;

/// <summary>
/// File upload request model for form data
/// </summary>
public class FileUploadRequestDto
{
    /// <summary>
    /// File to upload
    /// </summary>
    public IFormFile File { get; set; } = null!;

    /// <summary>
    /// File category
    /// </summary>
    public FileCategory Category { get; set; }

    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Related entity ID (e.g., FinancialReport ID, ticker for CompanyLogo)
    /// </summary>
    public string? RelatedEntityId { get; set; }
}
