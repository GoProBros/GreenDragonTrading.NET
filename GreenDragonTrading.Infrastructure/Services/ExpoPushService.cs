using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Application.Common.Options;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GreenDragonTrading.Infrastructure.Services
{
    /// <summary>
    /// Sends push notifications via FCM HTTP v1 API using a Google service account.
    /// Replaces the Expo Push API so that standalone APKs receive notifications
    /// without depending on Expo Go's Firebase sender ID.
    /// </summary>
    public class ExpoPushService(
        IHttpClientFactory httpClientFactory,
        IOptions<FcmOptions> fcmOptions,
        ILogger<ExpoPushService> logger) : IExpoPushService
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly FcmOptions _fcmOptions = fcmOptions.Value;
        private readonly ILogger<ExpoPushService> _logger = logger;

        private const string FcmScope = "https://www.googleapis.com/auth/firebase.messaging";
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

            var accessToken = await GetAccessTokenAsync(cancellationToken);
            if (accessToken is null) return [];

            var staleTokens = new List<string>();
            var endpoint = $"https://fcm.googleapis.com/v1/projects/{_fcmOptions.ProjectId}/messages:send";
            var httpClient = _httpClientFactory.CreateClient("FcmPush");

            foreach (var batch in tokens.Chunk(BatchSize))
            {
                foreach (var token in batch)
                {
                    var message = BuildMessage(token, title, body, dataUrl);
                    try
                    {
                        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                        request.Content = JsonContent.Create(message, options: JsonOptions);

                        using var response = await httpClient.SendAsync(request, cancellationToken);

                        if (response.IsSuccessStatusCode) continue;

                        var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                        if (errorBody.Contains("UNREGISTERED") || errorBody.Contains("INVALID_ARGUMENT"))
                        {
                            staleTokens.Add(token);
                            _logger.LogInformation("[FCMPush] Stale token: {Token}", token);
                        }
                        else
                        {
                            _logger.LogWarning("[FCMPush] HTTP {Status} for token {Token}: {Body}",
                                response.StatusCode, token, errorBody);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[FCMPush] Failed to send to token {Token}", token);
                    }
                }
            }

            return staleTokens;
        }

        private async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            try
            {
#pragma warning disable CS0618 // GoogleCredential.FromStreamAsync obsolete — CredentialFactory requires newer min version
                GoogleCredential credential;

                if (!string.IsNullOrWhiteSpace(_fcmOptions.ServiceAccountJson))
                {
                    using var jsonStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(_fcmOptions.ServiceAccountJson));
                    credential = (await GoogleCredential.FromStreamAsync(jsonStream, cancellationToken))
                        .CreateScoped(FcmScope);
                }
                else if (!string.IsNullOrWhiteSpace(_fcmOptions.ServiceAccountKeyPath) &&
                         File.Exists(_fcmOptions.ServiceAccountKeyPath))
                {
                    using var fileStream = File.OpenRead(_fcmOptions.ServiceAccountKeyPath);
                    credential = (await GoogleCredential.FromStreamAsync(fileStream, cancellationToken))
                        .CreateScoped(FcmScope);
                }
                else
                {
                    _logger.LogWarning("[FCMPush] No FCM credentials configured. Skipping push delivery.");
                    return null;
                }
#pragma warning restore CS0618

                return await credential.UnderlyingCredential.GetAccessTokenForRequestAsync(
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FCMPush] Failed to obtain FCM access token");
                return null;
            }
        }

        private static FcmMessage BuildMessage(string token, string title, string body, string? dataUrl)
        {
            var data = dataUrl is not null
                ? new Dictionary<string, string> { ["url"] = dataUrl }
                : null;

            return new FcmMessage(
                Message: new FcmMessagePayload(
                    Token: token,
                    Notification: new FcmNotification(title, body),
                    Android: new FcmAndroidConfig(
                        Priority: "high",
                        Notification: new FcmAndroidNotification(
                            ChannelId: "kf-stock-system",
                            Sound: "default"
                        )
                    ),
                    Data: data
                )
            );
        }

        // ─── Internal DTOs ────────────────────────────────────────────────────────

        private sealed record FcmMessage(FcmMessagePayload Message);

        private sealed record FcmMessagePayload(
            string Token,
            FcmNotification Notification,
            FcmAndroidConfig Android,
            [property: JsonPropertyName("data")] Dictionary<string, string>? Data = null);

        private sealed record FcmNotification(string Title, string Body);

        private sealed record FcmAndroidConfig(string Priority, FcmAndroidNotification Notification);

        private sealed record FcmAndroidNotification(
            [property: JsonPropertyName("channel_id")] string ChannelId,
            string Sound);
    }
}
