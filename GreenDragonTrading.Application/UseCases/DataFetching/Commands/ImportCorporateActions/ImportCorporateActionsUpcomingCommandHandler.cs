using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportCorporateActions;

public class ImportCorporateActionsUpcomingCommandHandler(
    IUnitOfWork uow,
    IDnseService dnseService,
    ILogger<ImportCorporateActionsUpcomingCommandHandler> logger)
    : IRequestHandler<ImportCorporateActionsUpcomingCommand, ApiResponse<ImportCorporateActionsResult>>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IDnseService _dnseService = dnseService;
    private readonly ILogger<ImportCorporateActionsUpcomingCommandHandler> _logger = logger;

    public async Task<ApiResponse<ImportCorporateActionsResult>> Handle(
        ImportCorporateActionsUpcomingCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var validTickers = (await _uow.Symbols.GetAllTickersAsync(cancellationToken))
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim().ToUpperInvariant())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var totalPages = 0;
            var fetchedCount = 0;
            var insertedCount = 0;
            var updatedCount = 0;
            var skippedCount = 0;
            var page = 1;

            while (true)
            {
                var response = await _dnseService.GetCorporateActionsUpcomingAsync(page, cancellationToken);

                var items = response?.CorporateActions ?? new List<DnseCorporateActionItem>();
                if (items.Count == 0)
                {
                    break;
                }

                totalPages++;
                fetchedCount += items.Count;

                var (inserted, updated, skipped) = await UpsertCorporateActionsAsync(
                    items,
                    validTickers,
                    cancellationToken);

                insertedCount += inserted;
                updatedCount += updated;
                skippedCount += skipped;

                page++;

                await Task.Delay(50, cancellationToken);
            }

            var result = new ImportCorporateActionsResult(
                0,
                totalPages,
                fetchedCount,
                insertedCount,
                updatedCount,
                skippedCount,
                $"Upcoming corporate actions import completed. Inserted {insertedCount}, updated {updatedCount}.");

            return ApiResponse<ImportCorporateActionsResult>.Success(result, result.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing upcoming corporate actions from DNSE");
            return ApiResponse<ImportCorporateActionsResult>.Failure("Failed to import corporate actions from DNSE.");
        }
    }

    private async Task<(int Inserted, int Updated, int Skipped)> UpsertCorporateActionsAsync(
        List<DnseCorporateActionItem> items,
        HashSet<string> validTickers,
        CancellationToken cancellationToken)
    {
        var normalizedItems = items
            .Where(x => x.EventId > 0)
            .GroupBy(x => x.EventId)
            .Select(g => g.First())
            .ToList();

        if (normalizedItems.Count == 0)
        {
            return (0, 0, items.Count);
        }

        var eventIds = normalizedItems
            .Select(x => x.EventId)
            .Distinct()
            .ToList();

        var existingEntities = await _uow.CorporateActions.GetByEventIdsAsync(eventIds, cancellationToken);
        var existingMap = existingEntities.ToDictionary(x => x.EventId, x => x);

        var now = DateTimeOffset.UtcNow;
        var inserts = new List<CorporateAction>();
        var updates = new List<CorporateAction>();
        var skipped = 0;

        foreach (var item in normalizedItems)
        {
            var ticker = string.IsNullOrWhiteSpace(item.Symbol)
                ? string.Empty
                : item.Symbol.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(ticker))
            {
                skipped++;
                continue;
            }

            if (!validTickers.Contains(ticker))
            {
                skipped++;
                continue;
            }

            if (existingMap.TryGetValue(item.EventId, out var existing))
            {
                ApplyUpdates(existing, item, ticker, now);
                updates.Add(existing);
            }
            else
            {
                inserts.Add(BuildCorporateAction(item, ticker, now));
            }
        }

        if (inserts.Count > 0)
        {
            await _uow.CorporateActions.AddRangeAsync(inserts, cancellationToken);
        }

        if (updates.Count > 0)
        {
            _uow.CorporateActions.UpdateRange(updates);
        }

        if (inserts.Count > 0 || updates.Count > 0)
        {
            await _uow.SaveChangesAsync(cancellationToken);
        }

        return (inserts.Count, updates.Count, skipped);
    }

    private static CorporateAction BuildCorporateAction(
        DnseCorporateActionItem item,
        string ticker,
        DateTimeOffset now)
    {
        return new CorporateAction
        {
            EventId = item.EventId,
            Ticker = ticker,
            Name = NormalizeName(item),
            Title = item.Title,
            TitleEvent = item.TitleEvent,
            Content = item.Content,
            Note = item.Note,
            Url = item.Url,
            ExRightsDate = ParseDnseDate(item.ExRightsDate),
            RecordDate = ParseDnseDate(item.RecordDate),
            ActionDate = ParseDnseDate(item.ActionDate),
            EventType = item.EventType,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static void ApplyUpdates(
        CorporateAction existing,
        DnseCorporateActionItem item,
        string ticker,
        DateTimeOffset now)
    {
        existing.Ticker = ticker;
        existing.Name = NormalizeName(item);
        existing.Title = item.Title;
        existing.TitleEvent = item.TitleEvent;
        existing.Content = item.Content;
        existing.Note = item.Note;
        existing.Url = item.Url;
        existing.ExRightsDate = ParseDnseDate(item.ExRightsDate);
        existing.RecordDate = ParseDnseDate(item.RecordDate);
        existing.ActionDate = ParseDnseDate(item.ActionDate);
        existing.EventType = item.EventType;
        existing.UpdatedAt = now;
    }

    private static string NormalizeName(DnseCorporateActionItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.Name))
        {
            return item.Name.Trim();
        }

        if (!string.IsNullOrWhiteSpace(item.TitleEvent))
        {
            return item.TitleEvent.Trim();
        }

        if (!string.IsNullOrWhiteSpace(item.Title))
        {
            return item.Title.Trim();
        }

        return string.Empty;
    }

    private static DateTimeOffset? ParseDnseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTimeOffset.TryParse(value, out var parsed)
            ? parsed.ToUniversalTime()
            : null;
    }
}
