using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace GreenDragonTrading.Application.UseCases.Symbols.Queries.GetOhlcv
{
    public class GetOhlcvQueryHandler(IFinscService finscService, ILogger<GetOhlcvQueryHandler> logger) : IRequestHandler<GetOhlcvQuery, ApiResponse<FinscStockResponse>>
    {
        private readonly IFinscService _finscService = finscService;
        private readonly ILogger _logger = logger;
        public async Task<ApiResponse<FinscStockResponse>> Handle(GetOhlcvQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var resolution = request.Resolution ?? "1D";

                var toDate = string.IsNullOrEmpty(request.ToDate)
                    ? DateTime.Today
                    : DateTime.ParseExact(request.ToDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                var fromDate = string.IsNullOrEmpty(request.FromDate)
                    ? toDate.AddMonths(-3)
                    : DateTime.ParseExact(request.FromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                var fromTimestamp = ((DateTimeOffset)fromDate).ToUnixTimeSeconds();
                var toTimestamp = ((DateTimeOffset)toDate.AddDays(1)).ToUnixTimeSeconds();

                _logger.LogInformation(
                "Fetching stock chart for {Symbol} with resolution {Resolution} from {FromDate} to {ToDate}",
                request.Symbol, resolution, fromDate, toDate);

                var finscRequest = new FinscStockRequest
                {
                    Symbol = request.Symbol,
                    Resolution = resolution,
                    From = fromTimestamp,
                    To = toTimestamp
                };

                var stockData = await _finscService.GetStockDataAsync(finscRequest, cancellationToken);
                if (stockData.Status == "error" || stockData.Timestamps == null || stockData.Timestamps.Length == 0)
                {
                    _logger.LogWarning("No data returned from Finsc for {Symbol}", request.Symbol);
                    return ApiResponse<FinscStockResponse>.Success(
                        stockData,
                        "No data available for the requested period"
                    );
                }

                _logger.LogInformation("Successfully fetched {Count} data points for {Symbol}", stockData.Timestamps.Length, request.Symbol);

                return ApiResponse<FinscStockResponse>.Success(stockData);

            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Invalid date format in request for {Symbol}", request.Symbol);
                return ApiResponse<FinscStockResponse>.Failure(
                    $"Invalid date format. Please use yyyy-MM-dd format. Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching stock chart for {Symbol}", request.Symbol);
                return ApiResponse<FinscStockResponse>.Failure(
                    $"Failed to fetch stock chart data: {ex.Message}");
            }
        }

    }
}
