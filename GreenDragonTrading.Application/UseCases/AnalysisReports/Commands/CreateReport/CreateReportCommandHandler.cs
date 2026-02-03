using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace GreenDragonTrading.Application.UseCases.AnalysisReports.Commands.CreateReport;

/// <summary>
/// Handler for CreateReportCommand
/// </summary>
public class CreateReportCommandHandler : IRequestHandler<CreateReportCommand, ApiResponse<AnalysisReportDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateReportCommandHandler(IUnitOfWork uow, IHttpContextAccessor httpContextAccessor)
    {
        _uow = uow;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse<AnalysisReportDto>> Handle(CreateReportCommand request, CancellationToken cancellationToken)
    {
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

        // Get current user
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
        Guid? uploadedBy = userIdClaim != null ? Guid.Parse(userIdClaim) : null;

        // Create report (without file)
        var report = new AnalysisReport
        {
            Id = Guid.NewGuid(),
            SourceId = request.SourceId,
            CategoryId = request.CategoryId,
            Title = request.Title,
            Description = request.Description,
            Tickers = request.Tickers,
            SectorId = request.SectorId,
            PublishDate = request.PublishDate,
            FilePath = string.Empty, // Will be updated when file is uploaded
            OriginalFileName = string.Empty,
            FileExtension = string.Empty,
            MimeType = string.Empty,
            FileSize = 0,
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

        return ApiResponse<AnalysisReportDto>.Success(dto, "Tạo báo cáo phân tích thành công. Vui lòng upload file.");
    }
}
