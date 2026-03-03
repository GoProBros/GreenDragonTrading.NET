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

        // Validate source
        var source = await _uow.AnalysisReportSources.GetByIdAsync(request.SourceId, cancellationToken);
        if (source == null)
        {
            throw new NotFoundException($"Nguồn '{request.SourceId}' không tồn tại");
        }

        // Validate category
        var category = await _uow.AnalysisReportCategories.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException($"Danh mục '{request.CategoryId}' không tồn tại");
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
        }

        // Validate sector if provided
        if (!string.IsNullOrEmpty(request.SectorId))
        {
            var sector = await _uow.Sectors.GetByIdAsync(request.SectorId, cancellationToken);
            if (sector == null)
            {
                throw new NotFoundException($"Ngành '{request.SectorId}' không tồn tại");
            }
        }

        // Update all fields from input
        report.SourceId = request.SourceId;
        report.CategoryId = request.CategoryId;
        report.Title = request.Title;
        report.Description = request.Description;
        report.Tickers = request.Tickers;
        report.SectorId = request.SectorId;
        report.PublishDate = request.PublishDate;
        report.Status = request.Status;
        report.UpdatedAt = DateTimeOffset.UtcNow;

        _uow.AnalysisReports.Update(report);
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
            CreatedAt = report.CreatedAt,
            UpdatedAt = report.UpdatedAt
        };

        return ApiResponse<AnalysisReportDto>.Success(dto, "Cập nhật báo cáo thành công");
    }
}
