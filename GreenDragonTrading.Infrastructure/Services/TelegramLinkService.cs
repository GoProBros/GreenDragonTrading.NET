using System.Security.Cryptography;
using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace GreenDragonTrading.Infrastructure.Services;

public class TelegramLinkService(
    IRedisService redisService,
    IUnitOfWork uow,
    IJwtService jwtService,
    IOptions<TelegramBotOptions> telegramOptions) : ITelegramLinkService
{
    private readonly IRedisService _redisService = redisService;
    private readonly IUnitOfWork _uow = uow;
    private readonly IJwtService _jwtService = jwtService;
    private readonly TelegramBotOptions _telegramOptions = telegramOptions.Value;

    public async Task<string> CreateStartTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var startToken = GenerateStartToken();
        var ttlMinutes = Math.Max(1, _telegramOptions.StartTokenTtlMinutes);

        await _redisService.SetAsync(
            RedisConstants.TelegramStartToken(startToken),
            userId.ToString(),
            TimeSpan.FromMinutes(ttlMinutes));

        return startToken;
    }

    public async Task<TelegramLinkResult> LinkByStartTokenAsync(
        string startToken,
        string chatId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(startToken) || string.IsNullOrWhiteSpace(chatId))
        {
            return new TelegramLinkResult
            {
                IsSuccess = false,
                Message = TelegramConstants.InvalidLinkTokenMessage
            };
        }

        var token = startToken.Trim();
        var normalizedChatId = chatId.Trim();

        var userId = await ResolveUserIdAsync(token);
        if (!userId.HasValue)
        {
            return new TelegramLinkResult
            {
                IsSuccess = false,
                Message = TelegramConstants.InvalidOrExpiredLinkTokenMessage
            };
        }

        var user = await _uow.Users.GetByIdAsync(userId.Value, cancellationToken);
        if (user == null)
        {
            return new TelegramLinkResult
            {
                IsSuccess = false,
                Message = TelegramConstants.LinkUserNotFoundMessage
            };
        }

        var duplicatedLink = await _uow.Users.FirstOrDefaultAsync(
            x => x.TelegramId == normalizedChatId,
            cancellationToken);

        if (duplicatedLink != null && duplicatedLink.Id != user.Id)
        {
            return new TelegramLinkResult
            {
                IsSuccess = false,
                Message = TelegramConstants.TelegramAlreadyLinkedMessage
            };
        }

        user.TelegramId = normalizedChatId;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(cancellationToken);

        await _redisService.RemoveAsync(RedisConstants.TelegramStartToken(token));

        return new TelegramLinkResult
        {
            IsSuccess = true,
            Message = TelegramConstants.LinkSuccessServiceMessage,
            UserId = user.Id
        };
    }

    private async Task<Guid?> ResolveUserIdAsync(string startToken)
    {
        var cachedUserId = await _redisService.GetAsync<string>(RedisConstants.TelegramStartToken(startToken));
        if (Guid.TryParse(cachedUserId, out var redisUserId))
        {
            return redisUserId;
        }

        var principal = _jwtService.ValidateToken(startToken);
        if (principal == null)
        {
            return null;
        }

        var userIdClaim = principal.FindFirst("uid")?.Value
            ?? principal.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdClaim, out var jwtUserId) ? jwtUserId : null;
    }

    private static string GenerateStartToken()
    {
        var bytes = new byte[24];
        RandomNumberGenerator.Fill(bytes);
        return WebEncoders.Base64UrlEncode(bytes);
    }
}