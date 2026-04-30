using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.CorporateActions.Queries.GetCorporateActions;

/// <summary>
/// Handler for GetCorporateActionsQuery.
/// </summary>
public class GetCorporateActionsQueryHandler(
    IUnitOfWork uow,
    ILogger<GetCorporateActionsQueryHandler> logger)
    : IRequestHandler<GetCorporateActionsQuery, ApiResponse<PaginatedResponse<CorporateActionDto>>>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ILogger<GetCorporateActionsQueryHandler> _logger = logger;

    public async Task<ApiResponse<PaginatedResponse<CorporateActionDto>>> Handle(
        GetCorporateActionsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting corporate actions with Search={Search}, Symbol={Symbol}, EventType={EventType}, PageIndex={PageIndex}, PageSize={PageSize}",
            request.Search,
            request.Symbol,
            request.EventType,
            request.PageIndex,
            request.PageSize);

        var (items, totalCount) = await _uow.CorporateActions.GetPaginatedAsync(
            request.Search,
            request.Symbol,
            request.EventType,
            request.PageIndex,
            request.PageSize,
            cancellationToken);

        var results = items
            .Select(CorporateActionMapper.ToDto)
            .ToList();

        var paginated = PaginatedResponse<CorporateActionDto>.Create(
            results,
            totalCount,
            request.PageIndex,
            request.PageSize);

        return ApiResponse<PaginatedResponse<CorporateActionDto>>.Success(
            paginated,
            "Lấy danh sách sự kiện cổ tức thành công.");
    }
}
