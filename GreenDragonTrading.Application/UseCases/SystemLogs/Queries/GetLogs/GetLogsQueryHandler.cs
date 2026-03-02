using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.SystemLogs.Queries.GetLogs
{
    /// <summary>
    /// Handles <see cref="GetLogsQuery"/> — returns a paginated, filtered list of log entries.
    /// </summary>
    public class GetLogsQueryHandler(
        ILogger<GetLogsQueryHandler> logger,
        ILogReaderService logReaderService) : IRequestHandler<GetLogsQuery, ApiResponse<PaginatedResponse<LogEntryDto>>>
    {
        private readonly ILogger<GetLogsQueryHandler> _logger = logger;
        private readonly ILogReaderService _logReaderService = logReaderService;

        public async Task<ApiResponse<PaginatedResponse<LogEntryDto>>> Handle(GetLogsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Fetching logs: Date={Date}, Level={Level}, Search={Search}, Page={PageIndex}/{PageSize}",
                request.Date, request.Level, request.Search, request.PageIndex, request.PageSize);

            var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            var pageSize = request.PageSize < 1 ? 50 : (request.PageSize > 500 ? 500 : request.PageSize);

            var (entries, totalCount) = await _logReaderService.GetLogsAsync(
                request.Date,
                request.Level,
                request.Search,
                pageIndex,
                pageSize,
                cancellationToken);

            var paginatedResponse = PaginatedResponse<LogEntryDto>.Create(entries, totalCount, pageIndex, pageSize);

            return ApiResponse<PaginatedResponse<LogEntryDto>>.Success(paginatedResponse, "Lấy danh sách log thành công");
        }
    }
}
