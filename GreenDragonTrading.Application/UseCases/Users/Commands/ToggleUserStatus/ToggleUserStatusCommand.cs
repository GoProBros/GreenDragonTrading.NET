using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Users.Commands.ToggleUserStatus;

/// <summary>
/// Command to toggle user account status between Active and InActive.
/// </summary>
public record ToggleUserStatusCommand(Guid UserId) : IRequest<ApiResponse>;
