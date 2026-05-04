using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReports.Commands.UploadReport;

/// <summary>
/// Handler for UploadReportCommand
/// </summary>
public class UploadReportCommandHandler : IRequestHandler<UploadReportCommand, ApiResponse<AnalysisReportDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ILocalFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;

    public UploadReportCommandHandler(
        IUnitOfWork uow,
        ILocalFileStorageService fileStorageService,
        ICurrentUserService currentUserService)
    {
        _uow = uow;
        _fileStorageService = fileStorageService;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<AnalysisReportDto>> Handle(UploadReportCommand request, CancellationToken cancellationToken)
    {
        var file = request.File;
        var metadata = request.Metadata;

        // Validate source
        var source = await _uow.AnalysisReportSources.GetByIdAsync(metadata.SourceId, cancellationToken);
        if (source == null)
        {
            throw new NotFoundException($"Nguồn '{metadata.SourceId}' không tồn tại");
        }

        // Validate category
        var category = await _uow.AnalysisReportCategories.GetByIdAsync(metadata.CategoryId, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException($"Danh mục '{metadata.CategoryId}' không tồn tại");
        }

        // Validate tickers if provided
        if (metadata.Tickers != null && metadata.Tickers.Length > 0)
        {
            foreach (var ticker in metadata.Tickers)
            {
                var symbol = await _uow.Symbols.GetByIdAsync(ticker, cancellationToken);
                if (symbol == null)
                {
                    throw new NotFoundException($"Mã CK '{ticker}' không tồn tại");
                }
            }
        }

        // Validate sector if provided
        if (!string.IsNullOrEmpty(metadata.SectorId))
        {
            var sector = await _uow.Sectors.GetByIdAsync(metadata.SectorId, cancellationToken);
            if (sector == null)
            {
                throw new NotFoundException($"Ngành '{metadata.SectorId}' không tồn tại");
            }
        }

        // Get current user
        Guid? uploadedBy = _currentUserService.UserId;

        // Determine file info
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var mimeType = FileConstants.MimeTypes.GetMimeType(extension);

        // Save file with category subdirectory structure
        var filePath = await _fileStorageService.SaveFileAsync(
            file,
            FileCategory.AnalysisReport,
            file.FileName,
            null,
            metadata.CategoryId,
            null,
            cancellationToken);

        // Create report
        var report = new AnalysisReport
        {
            Id = Guid.NewGuid(),
            SourceId = metadata.SourceId,
            CategoryId = metadata.CategoryId,
            Title = metadata.Title,
            Description = metadata.Description,
            Tickers = metadata.Tickers,
            SectorId = metadata.SectorId,
            PublishDate = metadata.PublishDate,
            FilePath = filePath,
            OriginalFileName = file.FileName,
            FileExtension = extension,
            MimeType = mimeType,
            FileSize = file.Length,
            UploadedBy = uploadedBy,
            Status = CommonStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _uow.AnalysisReports.AddAsync(report, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var dto = new AnalysisReportDto
        {
            Id = report.Id,
            SourceId = report.SourceId,
            CategoryId = report.CategoryId,
            Title = report.Title,
            Description = report.Description,
            Tickers = report.Tickers,
            SectorId = report.SectorId,
            PublishDate = report.PublishDate,
            FilePath = report.FilePath,
            OriginalFileName = report.OriginalFileName,
            FileExtension = report.FileExtension,
            MimeType = report.MimeType,
            FileSize = report.FileSize,
            UploadedBy = report.UploadedBy,
            Status = report.Status,
            CreatedAt = report.CreatedAt
        };

        return ApiResponse<AnalysisReportDto>.Success(dto, "Upload báo cáo phân tích thành công");
    }
}
