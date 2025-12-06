using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants.SSI;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Infrastructure.BackgroundWorkers
{
    /// <summary>
    /// Background service that handles SSI streaming events.
    /// Subscribes to streaming events once at startup and logs all received data.
    /// Broadcasts data to connected frontend clients via SignalR.
    /// </summary>
    public class SsiStreamingBackgroundService : BackgroundService
    {
        private readonly ISsiStreamingService _streamingService;
        private readonly ILogger<SsiStreamingBackgroundService> _logger;

        public SsiStreamingBackgroundService(
            ISsiStreamingService streamingService,
            ILogger<SsiStreamingBackgroundService> logger)
        {
            _streamingService = streamingService;
            _logger = logger;

            // Subscribe to events ONCE (this service is singleton)
            _streamingService.OnBroadcastReceived += HandleBroadcast;
            _streamingService.OnErrorReceived += HandleError;
            _streamingService.OnStateChanged += HandleStateChanged;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SSI Streaming Background Service started.");
            return Task.CompletedTask;
        }

        private void HandleBroadcast(string data)
        {
            _logger.LogInformation("Broadcast received: {Data}", data);

            // Process the broadcast data and send to frontend clients
            WrapperParser(data);
        }

        private async void HandleError(string error)
        {
            _logger.LogError("SSI Streaming error: {Error}", error);
            
            // Notify all connected clients about the error
        }

        private async void HandleStateChanged(string oldState, string newState)
        {
            _logger.LogInformation("SSI connection state changed: {OldState} -> {NewState}", oldState, newState);
            
            // Notify all connected clients about connection state change
        }

        public override void Dispose()
        {
            // Unsubscribe from events
            _streamingService.OnBroadcastReceived -= HandleBroadcast;
            _streamingService.OnErrorReceived -= HandleError;
            _streamingService.OnStateChanged -= HandleStateChanged;

            base.Dispose();
        }

        private void WrapperParser(string xQuoteString)
        {
            try
            {
                StreamingWrapperResponse wrapperResponse = JsonSerializer.Deserialize<StreamingWrapperResponse>(xQuoteString)!;

                var result = wrapperResponse.DataType switch
                {
                    SsiConstantsV2.SSI_STREAMING_DATA_TYPE_X_QUOTE => ResponseParser<XQuoteResponse>(wrapperResponse.Content!),
                    _ => null
                };
                _logger.LogInformation("Parsed streaming data: {Data}", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing streaming data: {Data}", xQuoteString);
            }
        }

        private static T ResponseParser<T>(string jsonString)
        {
            T response = JsonSerializer.Deserialize<T>(jsonString)!;
            return response;
        }
    }
}
