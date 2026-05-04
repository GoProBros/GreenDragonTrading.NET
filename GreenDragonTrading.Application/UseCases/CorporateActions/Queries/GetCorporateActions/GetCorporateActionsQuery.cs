using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.CorporateActions.Queries.GetCorporateActions;

/// <summary>
/// Query to retrieve paginated corporate actions with filters.
/// </summary>
/// <param name="Search">Keyword filter for title or content.</param>
/// <param name="Symbol">Optional ticker filter (e.g. VCB, FPT).</param>
/// <param name="EventType">Optional event type filter.</param>
public record GetCorporateActionsQuery(
    string? Search = null,
    string? Symbol = null,
    int? EventType = null
) : PaginationQuery, IRequest<ApiResponse<PaginatedResponse<CorporateActionDto>>>;
