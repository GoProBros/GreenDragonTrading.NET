using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Users.Commands.DeactivateUser;

/// <summary>
/// Command to deactivate a user account.
/// </summary>
public record DeactivateUserCommand(Guid UserId) : IRequest<ApiResponse>;
