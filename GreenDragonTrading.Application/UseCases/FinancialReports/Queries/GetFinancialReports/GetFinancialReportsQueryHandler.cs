using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Queries.GetFinancialReports
{
    public class GetFinancialReportsQueryHandler : IRequestHandler<GetFinancialReportsQuery, ApiResponse<PaginatedResponse<FinancialReportDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<GetFinancialReportsQueryHandler> _logger;

        public GetFinancialReportsQueryHandler(
            IUnitOfWork uow,
            ILogger<GetFinancialReportsQueryHandler> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<ApiResponse<PaginatedResponse<FinancialReportDto>>> Handle(GetFinancialReportsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var (reports, totalCount) = await _uow.FinancialReports.GetPaginatedAsync(
                    request.PageIndex,
                    request.PageSize,
                    request.Ticker,
                    request.Year,
                    request.Period.HasValue ? (int)request.Period.Value : null,
                    request.Status.HasValue ? (int)request.Status.Value : null,
                    cancellationToken);

                var dtos = reports.Select(r => new FinancialReportDto
                {
                    Id = r.Id,
                    Ticker = r.Ticker,
                    Year = r.Year,
                    Period = r.Period,
                    FileUrl = r.FilePath,
                    FileSize = r.FileSize,
                    ReportData = r.ReportData,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt
                }).ToList();

                var paginatedResponse = PaginatedResponse<FinancialReportDto>.Create(
                    dtos,
                    totalCount,
                    request.PageIndex,
                    request.PageSize);

                return ApiResponse<PaginatedResponse<FinancialReportDto>>.Success(
                    paginatedResponse,
                    "Lấy danh sách báo cáo tài chính thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting financial reports");
                throw;
            }
        }
    }
}
