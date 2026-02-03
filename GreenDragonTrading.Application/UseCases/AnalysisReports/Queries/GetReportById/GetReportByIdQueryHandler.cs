using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReports.Queries.GetReportById;

/// <summary>
/// Handler for GetReportByIdQuery
/// </summary>
public class GetReportByIdQueryHandler : IRequestHandler<GetReportByIdQuery, ApiResponse<AnalysisReportDto>>
{
    private readonly IUnitOfWork _uow;

    public GetReportByIdQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ApiResponse<AnalysisReportDto>> Handle(GetReportByIdQuery request, CancellationToken cancellationToken)
    {
        var report = await _uow.AnalysisReports.GetByIdAsync(request.Id, cancellationToken);
        if (report == null)
        {
            throw new NotFoundException($"Báo cáo với ID '{request.Id}' không tồn tại");
        }

        // Get related data
        var source = await _uow.AnalysisReportSources.GetByIdAsync(report.SourceId, cancellationToken);
        var category = await _uow.AnalysisReportCategories.GetByIdAsync(report.CategoryId, cancellationToken);
        
        string? uploaderName = null;
        if (report.UploadedBy.HasValue)
        {
            var uploader = await _uow.Users.GetByIdAsync(report.UploadedBy.Value, cancellationToken);
            uploaderName = uploader?.Username;
        }

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

        return ApiResponse<AnalysisReportDto>.Success(dto, "Lấy thông tin báo cáo thành công");
    }
}
