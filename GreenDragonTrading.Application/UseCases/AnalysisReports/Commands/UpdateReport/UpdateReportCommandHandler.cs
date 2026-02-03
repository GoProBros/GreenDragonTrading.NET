using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReports.Commands.UpdateReport;

/// <summary>
/// Handler for UpdateReportCommand
/// </summary>
public class UpdateReportCommandHandler : IRequestHandler<UpdateReportCommand, ApiResponse<AnalysisReportDto>>
{
    private readonly IUnitOfWork _uow;

    public UpdateReportCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ApiResponse<AnalysisReportDto>> Handle(UpdateReportCommand request, CancellationToken cancellationToken)
    {
        // Get existing report
        var report = await _uow.AnalysisReports.GetByIdAsync(request.Id, cancellationToken);
        if (report == null)
        {
            throw new NotFoundException($"Báo cáo với ID '{request.Id}' không tồn tại");
        }

        // Validate and update source if provided
        if (!string.IsNullOrEmpty(request.SourceId))
        {
            var sourceToValidate = await _uow.AnalysisReportSources.GetByIdAsync(request.SourceId, cancellationToken);
            if (sourceToValidate == null)
            {
                throw new NotFoundException($"Nguồn '{request.SourceId}' không tồn tại");
            }
            report.SourceId = request.SourceId;
        }

        // Validate and update category if provided
        if (!string.IsNullOrEmpty(request.CategoryId))
        {
            var categoryToValidate = await _uow.AnalysisReportCategories.GetByIdAsync(request.CategoryId, cancellationToken);
            if (categoryToValidate == null)
            {
                throw new NotFoundException($"Danh mục '{request.CategoryId}' không tồn tại");
            }
            report.CategoryId = request.CategoryId;
        }

        // Validate tickers if provided
        if (request.Tickers != null && request.Tickers.Length > 0)
        {
            foreach (var ticker in request.Tickers)
            {
                var symbol = await _uow.Symbols.GetByIdAsync(ticker, cancellationToken);
                if (symbol == null)
                {
                    throw new NotFoundException($"Mã CK '{ticker}' không tồn tại");
                }
            }
            report.Tickers = request.Tickers;
        }

        // Validate sector if provided
        if (request.SectorId != null)
        {
            if (!string.IsNullOrEmpty(request.SectorId))
            {
                var sector = await _uow.Sectors.GetByIdAsync(request.SectorId, cancellationToken);
                if (sector == null)
                {
                    throw new NotFoundException($"Ngành '{request.SectorId}' không tồn tại");
                }
            }
            report.SectorId = request.SectorId;
        }

        // Update other fields
        if (!string.IsNullOrEmpty(request.Title))
        {
            report.Title = request.Title;
        }

        if (request.Description != null)
        {
            report.Description = request.Description;
        }

        if (request.PublishDate.HasValue)
        {
            report.PublishDate = request.PublishDate;
        }

        if (request.Status.HasValue)
        {
            report.Status = request.Status.Value;
        }

        report.UpdatedAt = DateTimeOffset.UtcNow;

        _uow.AnalysisReports.Update(report);
        await _uow.SaveChangesAsync(cancellationToken);

        // Get related data for response
        var source = await _uow.AnalysisReportSources.GetByIdAsync(report.SourceId, cancellationToken);
        var category = await _uow.AnalysisReportCategories.GetByIdAsync(report.CategoryId, cancellationToken);

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
            CreatedAt = report.CreatedAt,
            UpdatedAt = report.UpdatedAt
        };

        return ApiResponse<AnalysisReportDto>.Success(dto, "Cập nhật báo cáo thành công");
    }
}
