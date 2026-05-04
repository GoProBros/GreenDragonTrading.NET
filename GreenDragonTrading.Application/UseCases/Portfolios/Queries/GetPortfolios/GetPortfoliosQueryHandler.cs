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

namespace GreenDragonTrading.Application.UseCases.Portfolios.Queries.GetPortfolios;

public class GetPortfoliosQueryHandler : IRequestHandler<GetPortfoliosQuery, ApiResponse<PortfolioListResponseDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly IRedisService _redisService;
    private readonly ILogger<GetPortfoliosQueryHandler> _logger;

    public GetPortfoliosQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        IRedisService redisService,
        ILogger<GetPortfoliosQueryHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _redisService = redisService;
        _logger = logger;
    }

    public async Task<ApiResponse<PortfolioListResponseDto>> Handle(GetPortfoliosQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.GetRequiredUserId();
        var role = _currentUserService.Role;

        List<Portfolio> portfolios;
        Dictionary<Guid, decimal> availableCapitalByUserId;

        if (string.Equals(role, nameof(UserRole.User), StringComparison.OrdinalIgnoreCase))
        {
            var user = await _uow.Users.GetByIdAsync(currentUserId, cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("Người dùng không tồn tại.");
            }

            portfolios = await _uow.Portfolios.GetByUserIdAsync(currentUserId, cancellationToken);
            portfolios = portfolios
                .Where(x => x.Status == CommonStatus.Active)
                .ToList();

            availableCapitalByUserId = new Dictionary<Guid, decimal>
            {
                [currentUserId] = user.InvestmentCapital ?? 0m
            };
        }
        else if (_currentUserService.IsAdminOrStaff)
        {
            var endUsers = (await _uow.Users.GetAllAsync(cancellationToken))
                .Where(x => x.Role == UserRole.User)
                .ToList();

            var endUserIds = endUsers
                .Select(x => x.Id)
                .ToHashSet();

            availableCapitalByUserId = endUsers
                .ToDictionary(x => x.Id, x => x.InvestmentCapital ?? 0m);

            portfolios = (await _uow.Portfolios.GetAllAsync(cancellationToken))
                .Where(x => endUserIds.Contains(x.UserId))
                .ToList();

            if (request.UserId.HasValue)
            {
                portfolios = portfolios
                    .Where(x => x.UserId == request.UserId.Value)
                    .ToList();
            }

            if (request.Status.HasValue)
            {
                portfolios = portfolios
                    .Where(x => x.Status == request.Status.Value)
                    .ToList();
            }
        }
        else
        {
            throw new AccessDeniedException("Bạn không có quyền xem danh sách portfolio.");
        }

        if (!string.IsNullOrWhiteSpace(request.Ticker))
        {
            var normalizedTickerSearch = request.Ticker.Trim().ToUpperInvariant();

            portfolios = portfolios
                .Where(x => !string.IsNullOrWhiteSpace(x.Ticker)
                            && x.Ticker.Trim().ToUpperInvariant().Contains(normalizedTickerSearch, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var portfolioIds = portfolios
            .Select(x => x.Id)
            .Distinct()
            .ToList();

        var transactions = await _uow.TradingTransactions.GetByPortfolioIdsAsync(portfolioIds, cancellationToken);

        var transactionsByPortfolioId = transactions
            .GroupBy(x => x.PortfolioId)
            .ToDictionary(x => x.Key, x => (IEnumerable<TradingTransaction>)x.ToList());

        var currentPricesByTicker = await GetCurrentPricesByTickerAsync(
            portfolios
                .Select(x => x.Ticker)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim().ToUpperInvariant()));

        var result = portfolios
            .OrderByDescending(x => x.CreatedAt)
            .Select(portfolio =>
            {
                transactionsByPortfolioId.TryGetValue(portfolio.Id, out var portfolioTransactions);

                var normalizedTicker = string.IsNullOrWhiteSpace(portfolio.Ticker)
                    ? string.Empty
                    : portfolio.Ticker.Trim().ToUpperInvariant();

                currentPricesByTicker.TryGetValue(normalizedTicker, out var currentPrice);

                return PortfolioMetricsMapper.ToListItemDto(
                    portfolio,
                    portfolioTransactions ?? [],
                    currentPrice);
            })
            .ToList();

        result = request.OverallFilter switch
        {
            PortfolioOverallFilter.Profit => result
                .Where(x => x.Overall.TotalPnL > 0m)
                .ToList(),
            PortfolioOverallFilter.Loss => result
                .Where(x => x.Overall.TotalPnL < 0m)
                .ToList(),
            _ => result
        };

        result = result
            .OrderByDescending(x => x.Overall.TotalPnL)
            .ThenByDescending(x => x.CreatedAt)
            .ToList();

        var totalInvestedAmount = result.Sum(x => x.TotalInvestedAmount);
        var totalSoldAmount = result.Sum(x => x.TotalSoldAmount);
        var totalHoldingAmount = result.Sum(x => x.TotalHoldingAmount);
        var investmentCapital = CalculateInvestmentCapital(
            role,
            currentUserId,
            request.UserId,
            result,
            availableCapitalByUserId);

        var totalCount = result.Count;
        var pagedItems = result
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var paginatedResponse = PaginatedResponse<PortfolioListItemDto>.Create(
            pagedItems,
            totalCount,
            request.PageIndex,
            request.PageSize);

        var response = new PortfolioListResponseDto
        {
            Portfolios = paginatedResponse,
            InvestmentCapital = investmentCapital,
            TotalInvestedAmount = totalInvestedAmount,
            TotalSoldAmount = totalSoldAmount,
            TotalHoldingAmount = totalHoldingAmount
        };

        _logger.LogInformation(
            "Retrieved {Count} portfolios (paged {PageIndex}/{PageSize}) for {Role} {UserId}",
            totalCount,
            request.PageIndex,
            request.PageSize,
            role,
            currentUserId);

        return ApiResponse<PortfolioListResponseDto>.Success(response, "Lấy danh sách portfolio thành công");
    }

    private static decimal CalculateInvestmentCapital(
        string? role,
        Guid currentUserId,
        Guid? requestedUserId,
        IEnumerable<PortfolioListItemDto> portfolios,
        IReadOnlyDictionary<Guid, decimal> availableCapitalByUserId)
    {
        if (string.Equals(role, nameof(UserRole.User), StringComparison.OrdinalIgnoreCase))
        {
            return availableCapitalByUserId.TryGetValue(currentUserId, out var currentUserCapital)
                ? currentUserCapital
                : 0m;
        }

        if (requestedUserId.HasValue)
        {
            return availableCapitalByUserId.TryGetValue(requestedUserId.Value, out var requestedCapital)
                ? requestedCapital
                : 0m;
        }

        return portfolios
            .Select(x => x.UserId)
            .Distinct()
            .Sum(userId => availableCapitalByUserId.TryGetValue(userId, out var userCapital)
                ? userCapital
                : 0m);
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
