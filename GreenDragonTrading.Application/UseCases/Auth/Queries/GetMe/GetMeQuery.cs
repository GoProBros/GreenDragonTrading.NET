using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Auth.Queries.GetMe
{
    /// <summary>
    /// Query to get current user information from token
    /// </summary>
    public record GetMeQuery : IRequest<ApiResponse<UserDto>>;
}
