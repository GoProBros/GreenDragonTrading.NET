using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Ohlcv.Commands.ImportIntradayOhlcvFromSsi
{
    public class ImportIntradayOhlcvFromSsiCommandHandler
        : IRequestHandler<ImportIntradayOhlcvFromSsiCommand, ApiResponse<ImportOhlcvResultDto>>
    {
        private readonly ISsiServiceV2 _ssiServiceV2;
        private readonly IOhlcvUnitOfWork _ohlcvUow;
        private readonly ILogger<ImportIntradayOhlcvFromSsiCommandHandler> _logger;

        public ImportIntradayOhlcvFromSsiCommandHandler(
            ISsiServiceV2 ssiServiceV2,
            IOhlcvUnitOfWork ohlcvUow,
            ILogger<ImportIntradayOhlcvFromSsiCommandHandler> logger)
        {
            _ssiServiceV2 = ssiServiceV2;
            _ohlcvUow = ohlcvUow;
            _logger = logger;
        }

        public async Task<ApiResponse<ImportOhlcvResultDto>> Handle(
            ImportIntradayOhlcvFromSsiCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Importing M1 OHLCV for {Ticker} from {From} to {To}",
                    request.Ticker, request.FromDate, request.ToDate);

                var result = new ImportOhlcvResultDto
                {
                    Ticker = request.Ticker,
                    Timeframe = "M1",
                    ImportedAt = DateTime.UtcNow
                };

                // Fetch từ SSI
                var ssiRequest = new IntradayOhlcRequest
                {
                    Symbol = request.Ticker,
                    FromDate = request.FromDate.ToString("dd/MM/yyyy"),
                    ToDate = request.ToDate.ToString("dd/MM/yyyy"),
                    Ascending = true,
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize
                };

                var (ssiResponse, count) = await _ssiServiceV2.FetchIntradayOhlcAsync(
                    ssiRequest, cancellationToken);

                if (ssiResponse?.Data == null || !ssiResponse.Data.Any())
                {
                    _logger.LogWarning("No OHLCV data from SSI for {Ticker}", request.Ticker);
                    result.Status = "No Data";
                    return ApiResponse<ImportOhlcvResultDto>.Success(result, "Không có dữ liệu từ SSI");
                }

                // Convert SSI response sang Entity
                var ohlcvEntities = new List<Domain.Entities.Ohlcv>();

                foreach (var item in ssiResponse.Data)
                {
                    try
                    {
                        var entity = ConvertToEntity(item);
                        if (entity != null)
                        {
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
                        _logger.LogWarning(ex, "Failed to parse OHLCV data for {Ticker}", request.Ticker);
                        result.RecordsSkipped++;
                    }
                }

                // Bulk insert vào DB
                if (ohlcvEntities.Any())
                {
                    await _ohlcvUow.Ohlcv.AddRangeAsync(ohlcvEntities, cancellationToken);
                    await _ohlcvUow.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Imported {Count} M1 candles for {Ticker}",
                        result.RecordsImported, request.Ticker);
                }

                return ApiResponse<ImportOhlcvResultDto>.Success(
                    result,
                    $"Import thành công {result.RecordsImported} nến M1"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing OHLCV for {Ticker}", request.Ticker);

                return ApiResponse<ImportOhlcvResultDto>.Failure(
                    $"Lỗi khi import OHLCV từ SSI: {ex.Message}"
                );
            }
        }

        private Domain.Entities.Ohlcv? ConvertToEntity(IntradayOhlcResponseModel ssiData)
        {
            // Parse data từ SSI (tất cả đều là string)
            if (!DateTime.TryParse($"{ssiData.TradingDate} {ssiData.Time}", out var time))
                return null;

            if (!decimal.TryParse(ssiData.Open, out var open)) return null;
            if (!decimal.TryParse(ssiData.High, out var high)) return null;
            if (!decimal.TryParse(ssiData.Low, out var low)) return null;
            if (!decimal.TryParse(ssiData.Close, out var close)) return null;
            if (!long.TryParse(ssiData.Volume, out var volume)) return null;

            decimal? value = null;
            if (!string.IsNullOrEmpty(ssiData.Value) && decimal.TryParse(ssiData.Value, out var v))
                value = v;

            return new Domain.Entities.Ohlcv
            {
                Time = DateTime.SpecifyKind(time, DateTimeKind.Utc),
                Ticker = ssiData.Symbol!.ToUpper(),
                Timeframe = OhlcvConstants.Timeframes.M1,
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