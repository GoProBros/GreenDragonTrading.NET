using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportDailyOhlcvFromSsi
{
    public class ImportDailyOhlcvFromSsiCommandHandler 
        : IRequestHandler<ImportDailyOhlcvFromSsiCommand, ApiResponse<ImportOhlcvResultDto>>
    {
        private readonly ISsiServiceV2 _ssiServiceV2;
        private readonly IOhlcvUnitOfWork _ohlcvUow;
        private readonly ILogger<ImportDailyOhlcvFromSsiCommandHandler> _logger;

        public ImportDailyOhlcvFromSsiCommandHandler(
            ISsiServiceV2 ssiServiceV2,
            IOhlcvUnitOfWork ohlcvUow,
            ILogger<ImportDailyOhlcvFromSsiCommandHandler> logger)
        {
            _ssiServiceV2 = ssiServiceV2;
            _ohlcvUow = ohlcvUow;
            _logger = logger;
        }

        public async Task<ApiResponse<ImportOhlcvResultDto>> Handle(
            ImportDailyOhlcvFromSsiCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Importing D1 OHLCV for {Ticker} from {From} to {To}",
                    request.Ticker, request.FromDate, request.ToDate);

                var result = new ImportOhlcvResultDto
                {
                    Ticker = request.Ticker,
                    Timeframe = "D1",
                    ImportedAt = DateTime.UtcNow
                };

                // Fetch từ SSI - format dd/MM/yyyy
                var ssiRequest = new DailyOhlcRequest
                {
                    Symbol = request.Ticker,
                    Fromdate = request.FromDate.ToString("dd/MM/yyyy"),
                    Todate = request.ToDate.ToString("dd/MM/yyyy"),
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize
                };

                var (ssiResponse, count) = await _ssiServiceV2.FetchDailyOhlcAsync(
                    ssiRequest, cancellationToken);

                if (ssiResponse?.Data == null || !ssiResponse.Data.Any())
                {
                    _logger.LogWarning("No Daily OHLCV data from SSI for {Ticker}", request.Ticker);
                    result.Status = "No Data";
                    return ApiResponse<ImportOhlcvResultDto>.Success(result, "Không có dữ liệu từ SSI");
                }

                // Convert SSI response sang Entity
                var ohlcvEntities = new List<Ohlcv>();

                foreach (var item in ssiResponse.Data)
                {
                    if (item.Data == null) continue;

                    try
                    {
                        var entity = ConvertToEntity(item.Data);
                        if (entity != null)
                        {
                            // Check xem đã tồn tại chưa
                            var exists = await _ohlcvUow.Ohlcv.ExistsAsync(
                                entity.Ticker, entity.Timeframe, entity.Time, cancellationToken);

                            if (!exists)
                            {
                                ohlcvEntities.Add(entity);
                                result.RecordsImported++;
                            }
                            else
                            {
                                result.RecordsSkipped++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse Daily OHLCV data: {Data}", item.Data);
                        result.RecordsSkipped++;
                    }
                }

                // Bulk insert vào DB
                if (ohlcvEntities.Any())
                {
                    await _ohlcvUow.Ohlcv.AddRangeAsync(ohlcvEntities, cancellationToken);
                    await _ohlcvUow.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Imported {Count} D1 candles for {Ticker}",
                        result.RecordsImported, request.Ticker);
                }

                return ApiResponse<ImportOhlcvResultDto>.Success(
                    result,
                    $"Import thành công {result.RecordsImported} nến D1"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing Daily OHLCV for {Ticker}", request.Ticker);
                
                return ApiResponse<ImportOhlcvResultDto>.Failure(
                    "Lỗi khi import Daily OHLCV từ SSI",
                    ex.Message
                );
            }
        }

        private Ohlcv? ConvertToEntity(DailyOhlcResponseModel ssiData)
        {
            // Parse TradingDate từ SSI (format: dd/MM/yyyy)
            if (!DateTime.TryParseExact(ssiData.TradingDate, "dd/MM/yyyy", 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, out var tradingDate))
                return null;

            if (!decimal.TryParse(ssiData.Open, out var open)) return null;
            if (!decimal.TryParse(ssiData.High, out var high)) return null;
            if (!decimal.TryParse(ssiData.Low, out var low)) return null;
            if (!decimal.TryParse(ssiData.Close, out var close)) return null;
            if (!long.TryParse(ssiData.Volume, out var volume)) return null;

            decimal? value = null;
            if (!string.IsNullOrEmpty(ssiData.Value) && decimal.TryParse(ssiData.Value, out var v))
                value = v;

            return new Ohlcv
            {
                Time = DateTime.SpecifyKind(tradingDate, DateTimeKind.Utc),
                Ticker = ssiData.Symbol!.ToUpper(),
                Timeframe = OhlcvConstants.Timeframes.D1,
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = volume,
                Value = value,
                CreatedAt = DateTime.UtcNow,
                Source = OhlcvConstants.Sources.SsiApi
            };
        }
    }
}
