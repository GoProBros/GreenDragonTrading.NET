using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Users.Queries.GetUserDetail;

/// <summary>
/// Query to get user detail for management.
/// </summary>
/// <param name="UserId">User identifier.</param>
public record GetUserDetailQuery(Guid UserId) : IRequest<ApiResponse<UserManagementDetailDto>>;