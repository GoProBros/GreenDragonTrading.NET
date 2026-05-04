using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReports.Queries.GetReports;

/// <summary>
/// Handler for GetReportsQuery
/// </summary>
public class GetReportsQueryHandler : IRequestHandler<GetReportsQuery, ApiResponse<PaginatedResponse<AnalysisReportDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetReportsQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ApiResponse<PaginatedResponse<AnalysisReportDto>>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        // Get reports, optionally filtered by status
        var allReports = request.Status.HasValue
            ? (await _uow.AnalysisReports.FindAsync(r => r.Status == request.Status.Value, cancellationToken)).ToList()
            : (await _uow.AnalysisReports.GetAllAsync(cancellationToken)).ToList();

        // Apply filters in memory
        var filteredReports = allReports.AsEnumerable();

        if (!string.IsNullOrEmpty(request.SourceId))
        {
            filteredReports = filteredReports.Where(r => r.SourceId == request.SourceId);
        }

        if (!string.IsNullOrEmpty(request.CategoryId))
        {
            filteredReports = filteredReports.Where(r => r.CategoryId == request.CategoryId);
        }

        if (!string.IsNullOrEmpty(request.Ticker))
        {
            filteredReports = filteredReports.Where(r => r.Tickers != null && r.Tickers.Contains(request.Ticker));
        }

        if (!string.IsNullOrEmpty(request.SectorId))
        {
            filteredReports = filteredReports.Where(r => r.SectorId == request.SectorId);
        }

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            filteredReports = filteredReports.Where(r =>
                r.Title.ToLower().Contains(term) ||
                (r.Description != null && r.Description.ToLower().Contains(term)));
        }

        // Total count
        var totalCount = filteredReports.Count();

        // Pagination
        var reports = filteredReports
            .OrderByDescending(r => r.PublishDate ?? r.CreatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Get related data separately
        var sourceIds = reports.Select(r => r.SourceId).Distinct().ToList();
        var categoryIds = reports.Select(r => r.CategoryId).Distinct().ToList();
        var uploaderIds = reports.Where(r => r.UploadedBy.HasValue)
            .Select(r => r.UploadedBy!.Value)
            .Distinct()
            .ToList();

        var sources = (await _uow.AnalysisReportSources.GetAllAsync(cancellationToken))
            .Where(s => sourceIds.Contains(s.Code))
            .ToDictionary(s => s.Code);

        var categories = (await _uow.AnalysisReportCategories.GetAllAsync(cancellationToken))
            .Where(c => categoryIds.Contains(c.Code))
            .ToDictionary(c => c.Code);

        var uploaders = (await _uow.Users.GetAllAsync(cancellationToken))
            .Where(u => uploaderIds.Contains(u.Id))
            .ToDictionary(u => u.Id);

        var dtos = reports.Select(r => new AnalysisReportDto
        {
            Id = r.Id,
            SourceId = r.SourceId,
            CategoryId = r.CategoryId,
            Title = r.Title,
            Description = r.Description,
            Tickers = r.Tickers,
            SectorId = r.SectorId,
            PublishDate = r.PublishDate,
            FilePath = r.FilePath,
            OriginalFileName = r.OriginalFileName,
            FileExtension = r.FileExtension,
            MimeType = r.MimeType,
            FileSize = r.FileSize,
            UploadedBy = r.UploadedBy,
            Status = r.Status,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
        }).ToList();

        var paginatedResponse = PaginatedResponse<AnalysisReportDto>.Create(dtos, totalCount, request.PageIndex, request.PageSize);
        return ApiResponse<PaginatedResponse<AnalysisReportDto>>.Success(paginatedResponse, "Lấy danh sách báo cáo thành công");
    }
}
