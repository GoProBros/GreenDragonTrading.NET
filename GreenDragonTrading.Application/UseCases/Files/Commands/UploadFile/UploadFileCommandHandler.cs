using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace GreenDragonTrading.Application.UseCases.Files.Commands.UploadFile;

/// <summary>
/// Handler for upload file command - routes to appropriate entity based on category
/// </summary>
public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, ApiResponse<FileResponseDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ILocalFileStorageService _fileStorageService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UploadFileCommandHandler(
        IUnitOfWork uow,
        ILocalFileStorageService fileStorageService,
        IHttpContextAccessor httpContextAccessor)
    {
        _uow = uow;
        _fileStorageService = fileStorageService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse<FileResponseDto>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var file = request.File;
        var metadata = request.Metadata;

        // Get current user ID
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
        Guid? uploadedBy = userIdClaim != null ? Guid.Parse(userIdClaim) : null;

        // Determine file type and extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileType = DetermineFileType(extension);
        var mimeType = FileConstants.MimeTypes.GetMimeType(extension);

        // Route to appropriate handler based on category
        return metadata.Category switch
        {
            FileCategory.FinancialReport => await HandleFinancialReportUpload(file, metadata, extension, mimeType, cancellationToken),
            FileCategory.AnalysisReport => await HandleAnalysisReportUpload(file, metadata, extension, mimeType, uploadedBy, cancellationToken),
            FileCategory.Avatar => await HandleAvatarUpload(file, metadata, extension, mimeType, uploadedBy, cancellationToken),
            FileCategory.CompanyLogo => await HandleCompanyLogoUpload(file, metadata, extension, mimeType, cancellationToken),
            _ => throw new BusinessRuleException("Invalid file category")
        };
    }

    private async Task<ApiResponse<FileResponseDto>> HandleFinancialReportUpload(
        IFormFile file, FileUploadDto metadata, string extension, string mimeType, CancellationToken cancellationToken)
    {
        // Financial reports must have ID
        if (string.IsNullOrWhiteSpace(metadata.RelatedEntityId))
        {
            throw new BusinessRuleException("ID báo cáo tài chính là bắt buộc");
        }

        if (!Guid.TryParse(metadata.RelatedEntityId, out var reportId))
        {
            throw new BusinessRuleException("ID báo cáo tài chính không hợp lệ");
        }

        // Get existing report
        var existingReport = await _uow.FinancialReports.GetByIdAsync(reportId, cancellationToken);
        if (existingReport == null)
        {
            throw new NotFoundException("Báo cáo tài chính không tồn tại");
        }

        // Save file with Ticker/Year structure
        var filePath = await _fileStorageService.SaveFileAsync(
            file, 
            FileCategory.FinancialReport, 
            file.FileName,
            existingReport.Id.ToString(),
            existingReport.Ticker,
            existingReport.Year,
            cancellationToken);

        // Delete old file if exists
        if (!string.IsNullOrEmpty(existingReport.FilePath))
        {
            await _fileStorageService.DeleteFileAsync(existingReport.FilePath, cancellationToken);
        }

        // Update report with new file
        existingReport.FilePath = filePath;
        existingReport.FileSize = file.Length;
        existingReport.UpdatedAt = DateTimeOffset.UtcNow;

        _uow.FinancialReports.Update(existingReport);
        await _uow.SaveChangesAsync(cancellationToken);

        return ApiResponse<FileResponseDto>.Success(
            MapToFileResponse(existingReport.Id, file.FileName, FileCategory.FinancialReport, 
                FileType.Pdf, extension, mimeType, file.Length, metadata.Description, 
                existingReport.Id.ToString(), "FinancialReport", null, null),
            "Upload báo cáo tài chính thành công");
    }

    private async Task<ApiResponse<FileResponseDto>> HandleAnalysisReportUpload(
        IFormFile file, FileUploadDto metadata, string extension, string mimeType, Guid? uploadedBy, CancellationToken cancellationToken)
    {
        // Analysis reports must have ID
        if (string.IsNullOrWhiteSpace(metadata.RelatedEntityId))
        {
            throw new BusinessRuleException("ID báo cáo phân tích là bắt buộc");
        }

        if (!Guid.TryParse(metadata.RelatedEntityId, out var reportId))
        {
            throw new BusinessRuleException("ID báo cáo phân tích không hợp lệ");
        }

        // Get existing report
        var existingReport = await _uow.AnalysisReports.GetByIdAsync(reportId, cancellationToken);
        if (existingReport == null)
        {
            throw new NotFoundException("Báo cáo phân tích không tồn tại");
        }

        // Save file with category subdirectory structure
        var filePath = await _fileStorageService.SaveFileAsync(
            file, 
            FileCategory.AnalysisReport, 
            file.FileName,
            reportId.ToString(),
            existingReport.CategoryId,
            null,
            cancellationToken);

        // Delete old file if exists
        if (!string.IsNullOrEmpty(existingReport.FilePath))
        {
            await _fileStorageService.DeleteFileAsync(existingReport.FilePath, cancellationToken);
        }

        // Update report with file info
        existingReport.FilePath = filePath;
        existingReport.OriginalFileName = file.FileName;
        existingReport.FileExtension = extension;
        existingReport.MimeType = mimeType;
        existingReport.FileSize = file.Length;
        existingReport.UpdatedAt = DateTimeOffset.UtcNow;

        _uow.AnalysisReports.Update(existingReport);
        await _uow.SaveChangesAsync(cancellationToken);

        string? uploaderName = null;
        if (existingReport.UploadedBy.HasValue)
        {
            var uploader = await _uow.Users.GetByIdAsync(existingReport.UploadedBy.Value, cancellationToken);
            uploaderName = uploader?.Username;
        }

        return ApiResponse<FileResponseDto>.Success(
            MapToFileResponse(existingReport.Id, file.FileName, FileCategory.AnalysisReport,
                DetermineFileType(extension), extension, mimeType, file.Length, existingReport.Description,
                existingReport.Id.ToString(), "AnalysisReport", existingReport.UploadedBy, uploaderName),
            "Upload báo cáo phân tích thành công");
    }

    private async Task<ApiResponse<FileResponseDto>> HandleAvatarUpload(
        IFormFile file, FileUploadDto metadata, string extension, string mimeType, Guid? uploadedBy, CancellationToken cancellationToken)
    {
        if (!uploadedBy.HasValue)
        {
            throw new UnauthenticatedException("Vui lòng đăng nhập để upload avatar");
        }

        var user = await _uow.Users.GetByIdAsync(uploadedBy.Value, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User không tồn tại");
        }

        var filePath = await _fileStorageService.SaveFileAsync(
            file, 
            FileCategory.Avatar, 
            file.FileName,
            uploadedBy.ToString(),
            null,
            null,
            cancellationToken);

        // Delete old avatar if exists
        if (!string.IsNullOrEmpty(user.AvatarUrl))
        {
            try
            {
                await _fileStorageService.DeleteFileAsync(user.AvatarUrl, cancellationToken);
            }
            catch
            {
                // Ignore if old file doesn't exist
            }
        }

        user.AvatarUrl = filePath;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(cancellationToken);

        return ApiResponse<FileResponseDto>.Success(
            MapToFileResponse(user.Id, file.FileName, FileCategory.Avatar,
                DetermineFileType(extension), extension, mimeType, file.Length, "User avatar",
                user.Id.ToString(), "User", uploadedBy, user.Username),
            "Upload avatar thành công");
    }

    private async Task<ApiResponse<FileResponseDto>> HandleCompanyLogoUpload(
        IFormFile file, FileUploadDto metadata, string extension, string mimeType, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(metadata.RelatedEntityId))
        {
            throw new BusinessRuleException("Ticker là bắt buộc cho logo công ty");
        }

        var symbol = await _uow.Symbols.GetByIdAsync(metadata.RelatedEntityId, cancellationToken);
        if (symbol == null)
        {
            throw new NotFoundException($"Mã chứng khoán {metadata.RelatedEntityId} không tồn tại");
        }

        var filePath = await _fileStorageService.SaveFileAsync(
            file, 
            FileCategory.CompanyLogo, 
            file.FileName,
            metadata.RelatedEntityId,
            null,
            null,
            cancellationToken);

        // Delete old logo if exists
        if (!string.IsNullOrEmpty(symbol.LogoPath))
        {
            try
            {
                await _fileStorageService.DeleteFileAsync(symbol.LogoPath, cancellationToken);
            }
            catch
            {
                // Ignore if old file doesn't exist
            }
        }

        symbol.LogoPath = filePath;
        _uow.Symbols.Update(symbol);
        await _uow.SaveChangesAsync(cancellationToken);

        return ApiResponse<FileResponseDto>.Success(
            MapToFileResponse(Guid.NewGuid(), file.FileName, FileCategory.CompanyLogo,
                DetermineFileType(extension), extension, mimeType, file.Length, $"Logo for {symbol.Ticker}",
                symbol.Ticker, "Symbol", null, null),
            "Upload logo công ty thành công");
    }

    private static FileType DetermineFileType(string extension)
    {
        return extension switch
        {
            ".pdf" => FileType.Pdf,
            ".docx" => FileType.Docx,
            ".jpg" or ".jpeg" => FileType.Jpeg,
            ".png" => FileType.Png,
            ".gif" => FileType.Gif,
            ".webp" => FileType.WebP,
            _ => FileType.Other
        };
    }

    private static FileResponseDto MapToFileResponse(
        Guid id, string fileName, FileCategory category, FileType fileType,
        string extension, string mimeType, long fileSize, string? description,
        string? relatedEntityId, string? relatedEntityType, Guid? uploadedBy, string? uploaderName)
    {
        return new FileResponseDto
        {
            Id = id,
            OriginalFileName = fileName,
            Category = category,
            FileType = fileType,
            FileExtension = extension,
            MimeType = mimeType,
            FileSize = fileSize,
            Description = description,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            UploadedBy = uploadedBy,
            UploaderName = uploaderName,
            Status = CommonStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            DownloadUrl = $"/api/files/{id}/download"
        };
    }
}
