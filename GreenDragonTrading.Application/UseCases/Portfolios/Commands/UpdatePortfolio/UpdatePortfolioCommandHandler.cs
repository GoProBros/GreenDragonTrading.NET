using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Portfolios.Commands.UpdatePortfolio;

public class UpdatePortfolioCommandHandler : IRequestHandler<UpdatePortfolioCommand, ApiResponse<PortfolioDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdatePortfolioCommandHandler> _logger;

    public UpdatePortfolioCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<UpdatePortfolioCommandHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<PortfolioDto>> Handle(UpdatePortfolioCommand request, CancellationToken cancellationToken)
    {
        if (!string.Equals(_currentUserService.Role, nameof(UserRole.User), StringComparison.OrdinalIgnoreCase))
        {
            throw new AccessDeniedException("Chỉ người dùng role User mới được cập nhật portfolio.");
        }

        var userId = _currentUserService.GetRequiredUserId();

        var portfolio = await _uow.Portfolios.GetByIdAndUserIdAsync(request.Id, userId, cancellationToken);
        if (portfolio == null)
        {
            throw new NotFoundException("Portfolio không tồn tại hoặc bạn không có quyền cập nhật.");
        }

        var user = await _uow.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("Người dùng không tồn tại.");
        }

        if (request.Ticker != null)
        {
            var normalizedTicker = request.Ticker.Trim().ToUpperInvariant();
            var symbol = await _uow.Symbols.GetByIdAsync(normalizedTicker, cancellationToken);
            if (symbol == null)
            {
                throw new NotFoundException("Mã cổ phiếu không tồn tại.");
            }

            portfolio.Ticker = normalizedTicker;
        }

        portfolio.Name = NormalizeNullableText(request.Name);
        portfolio.Description = NormalizeNullableText(request.Description);

        if (request.Status.HasValue)
        {
            portfolio.Status = request.Status.Value;
        }

        _uow.Portfolios.Update(portfolio);
        await _uow.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Portfolio {PortfolioId} updated by user {UserId}", portfolio.Id, userId);

        return ApiResponse<PortfolioDto>.Success(ToDto(portfolio, user.InvestmentCapital ?? 0m), "Cập nhật portfolio thành công");
    }

    private static string? NormalizeNullableText(string? value)
    {
        if (value == null)
        {
            return null;
        }

        var trimmed = value.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static PortfolioDto ToDto(Portfolio portfolio, decimal availableCapital)
    {
        var normalizedTicker = string.IsNullOrWhiteSpace(portfolio.Ticker)
            ? string.Empty
            : portfolio.Ticker.Trim().ToUpperInvariant();

        return new PortfolioDto
        {
            Id = portfolio.Id,
            UserId = portfolio.UserId,
            Name = portfolio.Name,
            Description = portfolio.Description,
            Status = portfolio.Status,
            CreatedAt = portfolio.CreatedAt,
            Ticker = normalizedTicker,
            AvailableCapital = availableCapital,
            Summary = new PortfolioSummaryDto(),
            HistoryPerformance = new PortfolioHistoryPerformanceDto(),
            Overall = new PortfolioOverallDto(),
            TransactionHistory = []
        };
    }
}
