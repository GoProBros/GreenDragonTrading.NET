using MediatR;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.Logout;

public record LogoutCommand(Guid UserId) : IRequest<bool>;
