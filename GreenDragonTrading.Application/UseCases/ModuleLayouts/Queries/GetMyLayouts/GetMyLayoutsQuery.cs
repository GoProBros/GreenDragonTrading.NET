using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.ModuleLayouts.Queries.GetMyLayouts;

/// <summary>
/// Query để lấy danh sách layout của user theo loại module
/// </summary>
public record GetMyLayoutsQuery(ModuleType ModuleType) : IRequest<ApiResponse<List<ModuleLayoutListItemDto>>>;
