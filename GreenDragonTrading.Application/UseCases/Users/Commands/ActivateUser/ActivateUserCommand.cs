using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Users.Commands.ActivateUser;

/// <summary>
/// Command to activate (unban) a user account.
/// </summary>
public record ActivateUserCommand(Guid UserId) : IRequest<ApiResponse>;
