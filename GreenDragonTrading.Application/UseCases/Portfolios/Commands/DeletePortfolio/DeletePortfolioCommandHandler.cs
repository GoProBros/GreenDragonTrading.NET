using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Portfolios.Commands.DeletePortfolio;

public class DeletePortfolioCommandHandler : IRequestHandler<DeletePortfolioCommand, ApiResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeletePortfolioCommandHandler> _logger;

    public DeletePortfolioCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<DeletePortfolioCommandHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse> Handle(DeletePortfolioCommand request, CancellationToken cancellationToken)
    {
        if (!string.Equals(_currentUserService.Role, nameof(UserRole.User), StringComparison.OrdinalIgnoreCase))
        {
            throw new AccessDeniedException("Chỉ người dùng role User mới được xóa portfolio.");
        }

        var userId = _currentUserService.GetRequiredUserId();

        var portfolio = await _uow.Portfolios.GetByIdAndUserIdAsync(request.Id, userId, cancellationToken);
        if (portfolio == null)
        {
            throw new NotFoundException("Portfolio không tồn tại hoặc bạn không có quyền xóa.");
        }

        _uow.Portfolios.Remove(portfolio);
        await _uow.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Portfolio {PortfolioId} deleted by user {UserId}", portfolio.Id, userId);

        return ApiResponse.Success("Xóa portfolio thành công");
    }
}
