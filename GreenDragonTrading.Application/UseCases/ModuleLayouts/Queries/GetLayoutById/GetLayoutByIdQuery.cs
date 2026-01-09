using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.ModuleLayouts.Queries.GetLayoutById;

/// <summary>
/// Query để lấy chi tiết layout theo ID
/// </summary>
public record GetLayoutByIdQuery(long Id) : IRequest<ApiResponse<ModuleLayoutDto>>;
