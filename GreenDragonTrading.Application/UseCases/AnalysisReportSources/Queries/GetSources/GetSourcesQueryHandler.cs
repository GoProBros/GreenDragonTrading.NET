using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportSources.Queries.GetSources;

/// <summary>
/// Handler for GetSourcesQuery
/// </summary>
public class GetSourcesQueryHandler : IRequestHandler<GetSourcesQuery, ApiResponse<PaginatedResponse<AnalysisReportSourceDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetSourcesQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ApiResponse<PaginatedResponse<AnalysisReportSourceDto>>> Handle(GetSourcesQuery request, CancellationToken cancellationToken)
    {
        var allSources = await _uow.AnalysisReportSources.GetAllAsync(cancellationToken);
        
        var filteredSources = allSources.AsQueryable();

        // Filter by status if provided
        if (request.Status.HasValue)
        {
            filteredSources = filteredSources.Where(s => s.Status == request.Status.Value);
        }

        var sourcesList = filteredSources
            .OrderBy(s => s.Name)
            .ToList();

        var totalCount = sourcesList.Count;

        var sources = sourcesList
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = sources.Select(s => new AnalysisReportSourceDto
        {
            Code = s.Code,
            Name = s.Name,
            Description = s.Description,
            Website = s.Website,
            LogoUrl = s.LogoUrl,
            Status = s.Status,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        }).ToList();

        var paginatedResponse = PaginatedResponse<AnalysisReportSourceDto>.Create(
            dtos,
            totalCount,
            request.PageIndex,
            request.PageSize
        );

        return ApiResponse<PaginatedResponse<AnalysisReportSourceDto>>.Success(paginatedResponse, "Lấy danh sách nguồn báo cáo thành công");
    }
}
