using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.SystemLogs.Queries.GetLogDates
{
    /// <summary>
    /// Handles <see cref="GetLogDatesQuery"/> — returns all available log file dates.
    /// </summary>
    public class GetLogDatesQueryHandler(
        ILogger<GetLogDatesQueryHandler> logger,
        ILogReaderService logReaderService) : IRequestHandler<GetLogDatesQuery, ApiResponse<LogDatesResult>>
    {
        private readonly ILogger<GetLogDatesQueryHandler> _logger = logger;
        private readonly ILogReaderService _logReaderService = logReaderService;

        public async Task<ApiResponse<LogDatesResult>> Handle(GetLogDatesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching available log dates");

            var dates = await _logReaderService.GetAvailableDatesAsync(cancellationToken);

            return ApiResponse<LogDatesResult>.Success(
                new LogDatesResult { Dates = dates },
                "Lấy danh sách ngày log thành công");
        }
    }
}
