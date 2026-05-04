using GreenDragonTrading.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GreenDragonTrading.Infrastructure.Services
{
    /// <summary>
    /// Sends push notifications via the Expo Push API.
    /// Expo transparently delivers through FCM (Android) and APNs (iOS).
    /// </summary>
    public class ExpoPushService(
        IHttpClientFactory httpClientFactory,
        ILogger<ExpoPushService> logger) : IExpoPushService
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly ILogger<ExpoPushService> _logger = logger;

        private const string ClientName = "ExpoPush";
        private const string PushEndpoint = "--/api/v2/push/send";
        private const int BatchSize = 100;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        /// <inheritdoc/>
        public async Task<IReadOnlyList<string>> SendAsync(
            IReadOnlyList<string> tokens,
            string title,
            string body,
            string? dataUrl = null,
            CancellationToken cancellationToken = default)
        {
            if (tokens.Count == 0) return [];

            var staleTokens = new List<string>();
            var tokenList = tokens.ToList();
            var httpClient = _httpClientFactory.CreateClient(ClientName);

            foreach (var batch in tokenList.Chunk(BatchSize))
            {
                var messages = batch.Select(token => new ExpoPushMessage(
                    To: token,
                    Title: title,
                    Body: body,
                    Data: dataUrl != null ? new ExpoPushData(dataUrl) : null
                )).ToList();

                try
                {
                    var response = await httpClient.PostAsJsonAsync(
                        PushEndpoint, messages, JsonOptions, cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning(
                            "[ExpoPush] HTTP {StatusCode} from Expo Push API", response.StatusCode);
                        continue;
                    }

                    var result = await response.Content
                        .ReadFromJsonAsync<ExpoPushResponse>(JsonOptions, cancellationToken);

                    if (result?.Data == null) continue;

                    for (var i = 0; i < result.Data.Count && i < batch.Length; i++)
                    {
                        var ticket = result.Data[i];
                        if (ticket.Status == "error" &&
                            ticket.Details?.Error == "DeviceNotRegistered")
                        {
                            staleTokens.Add(batch[i]);
                            _logger.LogInformation(
                                "[ExpoPush] Token marked as stale (DeviceNotRegistered): {Token}",
                                batch[i]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[ExpoPush] Failed to send batch of {Count} messages", batch.Length);
                }
            }

            return staleTokens;
        }

        // ─── Internal DTOs ────────────────────────────────────────────────────────

        private sealed record ExpoPushMessage(
            string To,
            string Title,
            string Body,
            string Sound = "default",
            string Priority = "high",
            string ChannelId = "kf-stock-system",
            ExpoPushData? Data = null);

        private sealed record ExpoPushData(string Url);

        private sealed record ExpoPushResponse(List<ExpoPushTicket> Data);

        private sealed record ExpoPushTicket(
            string Status,
            string? Id = null,
            string? Message = null,
            ExpoPushTicketDetails? Details = null);

        private sealed record ExpoPushTicketDetails(string? Error = null, string? Fault = null);
    }
}
