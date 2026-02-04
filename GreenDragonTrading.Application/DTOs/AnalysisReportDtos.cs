using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Application.DTOs;

// ============== Source DTOs ==============

/// <summary>
/// Analysis Report Source DTO
/// </summary>
public class AnalysisReportSourceDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public CommonStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary>
/// Create Analysis Report Source request
/// </summary>
public class CreateAnalysisReportSourceDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
}

/// <summary>
/// Update Analysis Report Source request
/// </summary>
public class UpdateAnalysisReportSourceDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public CommonStatus Status { get; set; }
}

// ============== Category DTOs ==============

/// <summary>
/// Analysis Report Category DTO
/// </summary>
public class AnalysisReportCategoryDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int Level { get; set; }
    public string? ParentId { get; set; }
    public CommonStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    
    // Navigation
    public AnalysisReportCategoryDto? ParentCategory { get; set; }
    public List<AnalysisReportCategoryDto> ChildCategories { get; set; } = new();
}

/// <summary>
/// Create Analysis Report Category request
/// </summary>
public class CreateAnalysisReportCategoryDto
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int Level { get; set; }
    public string? ParentId { get; set; }
}

/// <summary>
/// Update Analysis Report Category request
/// </summary>
public class UpdateAnalysisReportCategoryDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int Level { get; set; }
    public string? ParentId { get; set; }
    public CommonStatus Status { get; set; }
}

// ============== Analysis Report DTOs ==============

/// <summary>
/// Analysis Report DTO
/// </summary>
public class AnalysisReportDto
{
    public Guid Id { get; set; }
    public string SourceId { get; set; } = null!;
    public string CategoryId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string[]? Tickers { get; set; }
    public string? SectorId { get; set; }
    public DateTimeOffset? PublishDate { get; set; }
    public string? FilePath { get; set; } = null!;
    public string? OriginalFileName { get; set; } = null!;
    public string? FileExtension { get; set; } = null!;
    public string? MimeType { get; set; } = null!;
    public long? FileSize { get; set; }
    public Guid? UploadedBy { get; set; }
    public CommonStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary>
/// Create Analysis Report request (without file)
/// </summary>
public class CreateAnalysisReportDto
{
    public string SourceId { get; set; } = null!;
    public string CategoryId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string[]? Tickers { get; set; }
    public string? SectorId { get; set; }
    public DateTimeOffset? PublishDate { get; set; }
}

/// <summary>
/// Update Analysis Report request
/// </summary>
public class UpdateAnalysisReportDto
{
    public string? SourceId { get; set; }
    public string? CategoryId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string[]? Tickers { get; set; }
    public string? SectorId { get; set; }
    public DateTimeOffset? PublishDate { get; set; }
    public CommonStatus? Status { get; set; }
}

/// <summary>
/// Upload Analysis Report file request (FormData)
/// </summary>
public class UploadAnalysisReportDto
{
    public string SourceId { get; set; } = null!;
    public string CategoryId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string[]? Tickers { get; set; }
    public string? SectorId { get; set; }
    public DateTimeOffset? PublishDate { get; set; }
}
