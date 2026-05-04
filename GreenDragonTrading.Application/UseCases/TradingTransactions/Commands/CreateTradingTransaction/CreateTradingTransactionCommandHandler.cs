using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.TradingTransactions.Commands.CreateTradingTransaction;

public class CreateTradingTransactionCommandHandler : IRequestHandler<CreateTradingTransactionCommand, ApiResponse<TradingTransactionDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateTradingTransactionCommandHandler> _logger;

    public CreateTradingTransactionCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<CreateTradingTransactionCommandHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<TradingTransactionDto>> Handle(CreateTradingTransactionCommand request, CancellationToken cancellationToken)
    {
        if (!string.Equals(_currentUserService.Role, nameof(UserRole.User), StringComparison.OrdinalIgnoreCase))
        {
            throw new AccessDeniedException("Chỉ người dùng role User mới được tạo giao dịch.");
        }

        var userId = _currentUserService.GetRequiredUserId();

        var portfolio = await _uow.Portfolios.GetByIdAndUserIdAsync(request.PortfolioId, userId, cancellationToken);
        if (portfolio == null)
        {
            throw new NotFoundException("Portfolio không tồn tại hoặc bạn không có quyền truy cập.");
        }

        var normalizedTicker = portfolio.Ticker.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalizedTicker))
        {
            throw new BusinessRuleException("Portfolio chưa có mã cổ phiếu hợp lệ.");
        }

        var symbol = await _uow.Symbols.GetByIdAsync(normalizedTicker, cancellationToken);
        if (symbol == null)
        {
            throw new NotFoundException("Mã cổ phiếu không tồn tại.");
        }

        var transaction = new TradingTransaction
        {
            PortfolioId = request.PortfolioId,
            Side = request.Side!.Value,
            Quantity = request.Quantity,
            Price = request.Price,
            TransactionDate = request.TransactionDate ?? DateTimeOffset.UtcNow,
            RecordedAt = DateTimeOffset.UtcNow,
            Note = NormalizeNullableText(request.Note),
            OriginalMessage = NormalizeNullableText(request.OriginalMessage)
        };

        await _uow.TradingTransactions.AddAsync(transaction, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Trading transaction {TransactionId} created in portfolio {PortfolioId} by user {UserId}",
            transaction.Id,
            transaction.PortfolioId,
            userId);

        return ApiResponse<TradingTransactionDto>.Success(ToDto(transaction, normalizedTicker), "Tạo giao dịch thành công");
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

    private static TradingTransactionDto ToDto(TradingTransaction transaction, string ticker)
    {
        return new TradingTransactionDto
        {
            Id = transaction.Id,
            PortfolioId = transaction.PortfolioId,
            Ticker = ticker,
            Side = transaction.Side,
            Quantity = transaction.Quantity,
            Price = transaction.Price,
            TransactionDate = transaction.TransactionDate,
            RecordedAt = transaction.RecordedAt,
            Note = transaction.Note,
            OriginalMessage = transaction.OriginalMessage
        };
    }
}
