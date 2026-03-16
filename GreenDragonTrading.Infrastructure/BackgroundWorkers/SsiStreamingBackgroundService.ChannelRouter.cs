using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Constants.SSI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Infrastructure.BackgroundWorkers
{
    public partial class SsiStreamingBackgroundService
    {
        /// <summary>
        /// Handles raw SSI broadcast payload and routes it to the corresponding channel handler.
        /// </summary>
        /// <param name="data">Raw JSON payload received from SSI SignalR broadcast event.</param>
        private async Task HandleBroadcast(string data)
        {
            try
            {
                StreamingWrapperResponse wrapperResponse = JsonSerializer.Deserialize<StreamingWrapperResponse>(data)!;

                using var scope = _serviceScopeFactory.CreateScope();
                var redis = scope.ServiceProvider.GetRequiredService<IRedisService>();

                if (string.Equals(wrapperResponse.DataType, SsiConstantsV2.SSI_STREAMING_DATA_TYPE_X_TRADE))
                {
                    var response = JsonSerializer.Deserialize<XTradeResponse>(wrapperResponse.Content!);
                    await HandleXTrade(redis, response);
                }
                else if (string.Equals(wrapperResponse.DataType, SsiConstantsV2.SSI_STREAMING_DATA_TYPE_FOREIGN))
                {
                    var response = JsonSerializer.Deserialize<ForeignRoomResponse>(wrapperResponse.Content!);
                    await HandleForeignRoom(redis, response);
                }
                else if (string.Equals(wrapperResponse.DataType, SsiConstantsV2.SSI_STREAMING_DATA_TYPE_X))
                {
                    var response = JsonSerializer.Deserialize<SecuritiesSnapshot>(wrapperResponse.Content!);
                    await HandleSnapshot(redis, response);
                }
                else if (string.Equals(wrapperResponse.DataType, SsiConstantsV2.SSI_STREAMING_DATA_TYPE_B))
                {
                    var response = JsonSerializer.Deserialize<OhlcvDataResponse>(wrapperResponse.Content!);
                    await HandleOhlcvData(redis, response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing streaming data: {Data}", data);
            }
        }
    }
}
