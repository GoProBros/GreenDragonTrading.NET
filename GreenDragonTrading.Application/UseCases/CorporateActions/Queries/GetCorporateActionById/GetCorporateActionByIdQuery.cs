using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.CorporateActions.Queries.GetCorporateActionById;

/// <summary>
/// Query to retrieve a corporate action by event id.
/// </summary>
/// <param name="EventId">Corporate action event id.</param>
public record GetCorporateActionByIdQuery(int EventId) : IRequest<ApiResponse<CorporateActionDto>>;
