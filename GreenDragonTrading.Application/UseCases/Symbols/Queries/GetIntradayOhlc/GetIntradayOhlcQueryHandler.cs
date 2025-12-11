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
//                IntradayOhlcRequest intradayOhlcRequest = new()
//                {
//                    Symbol = request.Symbol,
//                    FromDate = request.FromDate,
//                    ToDate = request.ToDate,
//                    PageIndex = 1,
//                    PageSize = request.PageSize,
//                    Ascending = request.Ascending,
//                };

//                (IntradayOhlcResponse ssiOhlc, int count) = await _ssiServiceV2.FetchIntradayOhlcAsync(intradayOhlcRequest, cancellationToken);

//                //return ApiResponse<PaginatedResponse<IntradayOhlc>>.Success(
//                //    PaginatedResponse<IntradayOhlc>.Create(
//                //        ssiOhlc.Data,
//                //        ssiOhlc.count,
//                //        request.PageIndex,
//                //        request.PageSize));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error handling GetIntradayOhlcQuery for Symbol: {Symbol}", request.Symbol);
//                throw;
//            }
//        }
//    }
//}
