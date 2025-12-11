//using GreenDragonTrading.Application.Common.Models;
//using GreenDragonTrading.Application.DTOs;
//using GreenDragonTrading.Application.Interfaces;
//using MediatR;
//using Microsoft.Extensions.Logging;

//namespace GreenDragonTrading.Application.UseCases.Symbols.Queries.GetIntradayOhlc
//{
//    public class GetIntradayOhlcQueryHandler(
//        ISsiServiceV2 ssiServiceV2,
//        ILogger<GetIntradayOhlcQueryHandler> logger) : IRequestHandler<GetIntradayOhlcQuery, ApiResponse<PaginatedResponse<IntradayOhlc>>>
//    {
//        private readonly ISsiServiceV2 _ssiServiceV2 = ssiServiceV2;
//        private readonly ILogger<GetIntradayOhlcQueryHandler> _logger = logger;

//        public async Task<ApiResponse<PaginatedResponse<IntradayOhlc>>> Handle(GetIntradayOhlcQuery request, CancellationToken cancellationToken)
//        {
//            try
//            {
//                _logger.LogInformation("Fetching intraday OHLC for Symbol: {Symbol}, FromDate: {FromDate}, ToDate: {ToDate}, Resolution: {Resolution}",
//                    request.Symbol, request.FromDate, request.ToDate, request.Resolution);

//                // Fetch all 1-minute data from SSI by looping through pages
//                var allOneMinuteData = new List<IntradayOhlcResponseModel>();
//                int currentPage = 1;
//                const int pageSize = 1000;
//                int totalFetched = 0;

//                while (true)
//                {
//                    IntradayOhlcRequest intradayOhlcRequest = new()
//                    {
//                        Symbol = request.Symbol,
//                        FromDate = request.FromDate,
//                        ToDate = request.ToDate,
//                        PageIndex = currentPage,
//                        PageSize = pageSize,
//                        Ascending = request.Ascending ?? true,
//                    };

//                    _logger.LogInformation("Fetching page {PageIndex} from SSI API", currentPage);

//                    (IntradayOhlcResponse ssiOhlc, int count) = await _ssiServiceV2.FetchIntradayOhlcAsync(intradayOhlcRequest, cancellationToken);

//                    if (ssiOhlc?.Data == null || ssiOhlc.Data.Count == 0)
//                    {
//                        _logger.LogInformation("No more data from SSI API at page {PageIndex}", currentPage);
//                        break;
//                    }

//                    allOneMinuteData.AddRange(ssiOhlc.Data);
//                    totalFetched += count;

//                    _logger.LogInformation("Fetched {Count} records from page {PageIndex}, total so far: {Total}",
//                        count, currentPage, allOneMinuteData.Count);

//                    // If we got fewer records than page size, we've reached the end
//                    if (count < pageSize)
//                    {
//                        _logger.LogInformation("Reached end of data (received {Count} < {PageSize})", count, pageSize);
//                        break;
//                    }

//                    currentPage++;
//                }

//                if (allOneMinuteData.Count == 0)
//                {
//                    return ApiResponse<PaginatedResponse<IntradayOhlc>>.Success(
//                        PaginatedResponse<IntradayOhlc>.Create([], 0, request.PageIndex, request.PageSize),
//                        "No data found");
//                }

//                _logger.LogInformation("Completed fetching all data: {TotalRecords} 1-minute candles from {Pages} pages",
//                    allOneMinuteData.Count, currentPage);

//                int targetResolution = request.Resolution ?? 1;

//                List<IntradayOhlc> processedData;

//                if (targetResolution <= 1)
//                {
//                    processedData = [.. allOneMinuteData.Select(item => new IntradayOhlc
//                    {
//                        Symbol = item.Symbol ?? string.Empty,
//                        Value = item.Value ?? string.Empty,
//                        TradingDate = item.TradingDate ?? string.Empty,
//                        Time = item.Time ?? string.Empty,
//                        Open = item.Open ?? string.Empty,
//                        High = item.High ?? string.Empty,
//                        Low = item.Low ?? string.Empty,
//                        Close = item.Close ?? string.Empty,
//                        Volume = item.Volume ?? string.Empty,
//                    })];
//                }
//                else
//                {
//                    processedData = AggregateCandles(allOneMinuteData, targetResolution);
//                }

//                var paginatedData = processedData
//                    .Skip((request.PageIndex - 1) * request.PageSize)
//                    .Take(request.PageSize)
//                    .ToList();

//                _logger.LogInformation("Successfully processed {OriginalCount} 1-minute candles into {ProcessedCount} {Resolution}-minute candles",
//                    allOneMinuteData.Count, processedData.Count, targetResolution);

//                return ApiResponse<PaginatedResponse<IntradayOhlc>>.Success(
//                    PaginatedResponse<IntradayOhlc>.Create(paginatedData, processedData.Count, request.PageIndex, request.PageSize),
//                    $"Successfully fetched {processedData.Count} records with {targetResolution}-minute resolution");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error handling GetIntradayOhlcQuery for Symbol: {Symbol}", request.Symbol);
//                throw;
//            }
//        }

//        /// <summary>
//        /// Aggregate 1-minute candles into larger timeframe candles
//        /// </summary>
//        /// <param name="oneMinuteData">List of 1-minute OHLC data from SSI</param>
//        /// <param name="targetResolution">Target resolution in minutes (e.g., 5, 15, 30, 60)</param>
//        /// <returns>List of aggregated candles</returns>
//        private List<IntradayOhlc> AggregateCandles(List<IntradayOhlcResponseModel> oneMinuteData, int targetResolution)
//        {
//            var result = new List<IntradayOhlc>();

//            // Parse and sort data by time ascending
//            var parsedData = oneMinuteData
//                .Where(d => !string.IsNullOrEmpty(d.Time) && 
//                           !string.IsNullOrEmpty(d.TradingDate) &&
//                           TimeSpan.TryParse(d.Time, out _) &&
//                           DateTime.TryParseExact(d.TradingDate, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out _))
//                .Select(d => new
//                {
//                    Data = d,
//                    DateTime = DateTime.ParseExact(d.TradingDate, "dd/MM/yyyy", null) + TimeSpan.Parse(d.Time),
//                    Time = TimeSpan.Parse(d.Time)
//                })
//                .OrderBy(x => x.DateTime)
//                .ToList();

//            if (parsedData.Count == 0)
//                return result;

//            // Group candles by target resolution intervals
//            var currentGroup = new List<IntradayOhlcResponseModel>();
//            DateTime? groupStartTime = null;

//            foreach (var item in parsedData)
//            {
//                if (groupStartTime == null)
//                {
//                    // Start new group
//                    groupStartTime = GetAlignedTime(item.DateTime, targetResolution);
//                    currentGroup.Add(item.Data);
//                }
//                else
//                {
//                    var nextGroupStartTime = groupStartTime.Value.AddMinutes(targetResolution);
                    
//                    if (item.DateTime < nextGroupStartTime)
//                    {
//                        // Add to current group
//                        currentGroup.Add(item.Data);
//                    }
//                    else
//                    {
//                        // Finalize current group and create aggregated candle
//                        if (currentGroup.Count > 0)
//                        {
//                            result.Add(CreateAggregatedCandle(currentGroup, targetResolution));
//                        }

//                        // Start new group
//                        groupStartTime = GetAlignedTime(item.DateTime, targetResolution);
//                        currentGroup = new List<IntradayOhlcResponseModel> { item.Data };
//                    }
//                }
//            }

//            // Don't forget the last group
//            if (currentGroup.Count > 0)
//            {
//                result.Add(CreateAggregatedCandle(currentGroup, targetResolution));
//            }

//            return result;
//        }

//        /// <summary>
//        /// Align datetime to resolution boundary (e.g., for 5-min: 09:00, 09:05, 09:10...)
//        /// </summary>
//        private static DateTime GetAlignedTime(DateTime dt, int resolution)
//        {
//            int totalMinutes = dt.Hour * 60 + dt.Minute;
//            int alignedMinutes = (totalMinutes / resolution) * resolution;
            
//            return dt.Date.AddMinutes(alignedMinutes);
//        }

//        /// <summary>
//        /// Create aggregated candle from a group of 1-minute candles
//        /// </summary>
//        private static IntradayOhlc CreateAggregatedCandle(List<IntradayOhlcResponseModel> group, int resolution)
//        {
//            var first = group[0];
//            var last = group[group.Count - 1];

//            // Parse numeric values for aggregation
//            var highValues = group
//                .Where(c => decimal.TryParse(c.High, out _))
//                .Select(c => decimal.Parse(c.High ?? string.Empty))
//                .ToList();

//            var lowValues = group
//                .Where(c => decimal.TryParse(c.Low, out _))
//                .Select(c => decimal.Parse(c.Low ?? string.Empty))
//                .ToList();

//            var volumeValues = group
//                .Where(c => decimal.TryParse(c.Volume, out _))
//                .Select(c => decimal.Parse(c.Volume ?? string.Empty))
//                .ToList();

//            return new IntradayOhlc
//            {
//                Symbol = first.Symbol ?? string.Empty,
//                Value = last.Value ?? string.Empty,
//                TradingDate = first.TradingDate ?? string.Empty,
//                Time = first.Time ?? string.Empty,
//                Open = first.Open ?? string.Empty,
//                High = highValues.Count > 0 ? highValues.Max().ToString() : first.High ?? string.Empty,
//                Low = lowValues.Count > 0 ? lowValues.Min().ToString() : first.Low ?? string.Empty,
//                Close = last.Close ?? string.Empty,
//                Volume = volumeValues.Count > 0 ? volumeValues.Sum().ToString() : "0",
//            };
//        }
//    }
//}
