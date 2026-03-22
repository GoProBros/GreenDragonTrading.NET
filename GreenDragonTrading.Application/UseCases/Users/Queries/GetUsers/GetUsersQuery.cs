using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Users.Queries.GetUsers;

/// <summary>
/// Query to get users list for management.
/// </summary>
public record GetUsersQuery : PaginationQuery, IRequest<ApiResponse<PaginatedResponse<UserManagementListItemDto>>>;