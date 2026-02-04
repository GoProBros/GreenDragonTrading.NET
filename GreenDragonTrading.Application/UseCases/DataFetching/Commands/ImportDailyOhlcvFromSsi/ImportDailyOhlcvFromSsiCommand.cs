using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportDailyOhlcvFromSsi
{
    /// <summary>
    /// Command để import daily OHLCV (D1) từ SSI vào TimescaleDB
    /// </summary>
    public class ImportDailyOhlcvFromSsiCommand : IRequest<ApiResponse<ImportOhlcvResultDto>>
    {
        public string Ticker { get; set; } = null!;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 1000;
    }
}
