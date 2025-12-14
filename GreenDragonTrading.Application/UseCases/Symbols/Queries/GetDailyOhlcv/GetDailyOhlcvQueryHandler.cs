using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace GreenDragonTrading.Application.UseCases.Symbols.Queries.GetDailyOhlcv
{
    public class GetDailyOhlcvQueryHandler(
        IFinscService finscService, 
        ILogger<GetDailyOhlcvQueryHandler> logger) 
        : IRequestHandler<GetDailyOhlcvQuery, ApiResponse<FinscStockResponse>>
    {
        private readonly IFinscService _finscService = finscService;
        private readonly ILogger<GetDailyOhlcvQueryHandler> _logger = logger;

        public async Task<ApiResponse<FinscStockResponse>> Handle(
            GetDailyOhlcvQuery request, 
            CancellationToken cancellationToken)
        {
            try
            {
                // Always use 1D resolution for daily data
                const string resolution = "1D";

                var toDate = string.IsNullOrEmpty(request.ToDate)
                    ? DateTime.Today
                    : DateTime.ParseExact(request.ToDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                var fromDate = string.IsNullOrEmpty(request.FromDate)
                    ? toDate.AddMonths(-3)
                    : DateTime.ParseExact(request.FromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                var fromTimestamp = ((DateTimeOffset)fromDate).ToUnixTimeSeconds();
                var toTimestamp = ((DateTimeOffset)toDate.AddDays(1)).ToUnixTimeSeconds();

                // Finsc API requires uppercase symbol
                var symbol = request.Symbol.ToUpperInvariant();

                _logger.LogInformation(
                    "Fetching daily OHLCV chart for {Symbol} from {FromDate} to {ToDate}",
                    symbol, fromDate, toDate);

                var finscRequest = new FinscStockRequest
                {
                    Symbol = symbol,
                    Resolution = resolution,
                    From = fromTimestamp,
                    To = toTimestamp
                };

                var stockData = await _finscService.GetStockDataAsync(finscRequest, cancellationToken);

                if (stockData.Status == "error" || stockData.Timestamps == null || stockData.Timestamps.Length == 0)
                {
                    _logger.LogWarning("No daily data returned from Finsc for {Symbol}", symbol);
                    return ApiResponse<FinscStockResponse>.Success(
                        stockData,
                        "No data available for the requested period"
                    );
                }

                _logger.LogInformation(
                    "Successfully fetched {Count} daily data points for {Symbol}",
                    stockData.Timestamps.Length, symbol);

                return ApiResponse<FinscStockResponse>.Success(stockData);
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Invalid date format in request for {Symbol}", request.Symbol);
                return ApiResponse<FinscStockResponse>.Failure(
                    $"Invalid date format. Please use dd/MM/yyyy format. Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching daily OHLCV chart for {Symbol}", request.Symbol);
                return ApiResponse<FinscStockResponse>.Failure(
                    $"Failed to fetch daily chart data: {ex.Message}");
            }
        }
    }
}
