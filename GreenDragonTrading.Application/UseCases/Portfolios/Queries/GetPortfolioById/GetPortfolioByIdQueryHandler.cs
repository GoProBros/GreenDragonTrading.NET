using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Application.UseCases.Portfolios.Common;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Portfolios.Queries.GetPortfolioById;

public class GetPortfolioByIdQueryHandler : IRequestHandler<GetPortfolioByIdQuery, ApiResponse<PortfolioDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly IRedisService _redisService;
    private readonly ILogger<GetPortfolioByIdQueryHandler> _logger;

    public GetPortfolioByIdQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        IRedisService redisService,
        ILogger<GetPortfolioByIdQueryHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _redisService = redisService;
        _logger = logger;
    }

    public async Task<ApiResponse<PortfolioDto>> Handle(GetPortfolioByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var role = _currentUserService.Role;

        Portfolio? portfolio;
        User? owner;

        if (string.Equals(role, nameof(UserRole.User), StringComparison.OrdinalIgnoreCase))
        {
            portfolio = await _uow.Portfolios.GetByIdAndUserIdAsync(request.Id, userId, cancellationToken);
            if (portfolio == null)
            {
                throw new NotFoundException("Portfolio không tồn tại hoặc bạn không có quyền truy cập.");
            }

            owner = await _uow.Users.GetByIdAsync(userId, cancellationToken);
            if (owner == null)
            {
                throw new NotFoundException("Người dùng không tồn tại.");
            }
        }
        else if (_currentUserService.IsAdminOrStaff)
        {
            portfolio = await _uow.Portfolios.GetByIdAsync(request.Id, cancellationToken);
            if (portfolio == null)
            {
                throw new NotFoundException("Portfolio không tồn tại.");
            }

            owner = await _uow.Users.GetByIdAsync(portfolio.UserId, cancellationToken);
            if (owner == null || owner.Role != UserRole.User)
            {
                throw new AccessDeniedException("Staff/Admin chỉ được xem portfolio của tài khoản User.");
            }
        }
        else
        {
            throw new AccessDeniedException("Bạn không có quyền xem portfolio.");
        }

        var transactions = await _uow.TradingTransactions.GetByPortfolioIdsAsync([portfolio.Id], cancellationToken);

        var currentPricesByTicker = await GetCurrentPricesByTickerAsync(
            new[] { portfolio.Ticker }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToUpperInvariant()));

        var normalizedTicker = string.IsNullOrWhiteSpace(portfolio.Ticker)
            ? string.Empty
            : portfolio.Ticker.Trim().ToUpperInvariant();

        currentPricesByTicker.TryGetValue(normalizedTicker, out var currentPrice);

        var dto = PortfolioMetricsMapper.ToDto(
            portfolio,
            transactions,
            owner.InvestmentCapital ?? 0m,
            currentPrice);

        _logger.LogInformation("Portfolio {PortfolioId} was retrieved by {Role} {UserId}", request.Id, role, userId);

        return ApiResponse<PortfolioDto>.Success(dto, "Lấy portfolio thành công");
    }

    private async Task<Dictionary<string, decimal>> GetCurrentPricesByTickerAsync(IEnumerable<string> tickers)
    {
        var normalizedTickers = tickers
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalizedTickers.Count == 0)
        {
            return new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        }

        var redisKeys = normalizedTickers
            .Select(RedisConstants.MarketDataSymbol)
            .ToList();

        var snapshots = await _redisService.GetHashBatchAsync<MarketSymbolDto>(redisKeys);

        var currentPrices = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        foreach (var ticker in normalizedTickers)
        {
            var redisKey = RedisConstants.MarketDataSymbol(ticker);
            if (!snapshots.TryGetValue(redisKey, out var snapshot) || snapshot == null)
            {
                continue;
            }

            currentPrices[ticker] = Convert.ToDecimal(snapshot.LastPrice);
        }

        return currentPrices;
    }
}
