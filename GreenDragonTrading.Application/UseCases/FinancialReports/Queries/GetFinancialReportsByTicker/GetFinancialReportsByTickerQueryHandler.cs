using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Queries.GetFinancialReportsByTicker
{
    public class GetFinancialReportsByTickerQueryHandler : IRequestHandler<GetFinancialReportsByTickerQuery, ApiResponse<PaginatedResponse<SimpleFinancialReportDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<GetFinancialReportsByTickerQueryHandler> _logger;

        public GetFinancialReportsByTickerQueryHandler(
            IUnitOfWork uow,
            IFileStorageService fileStorageService,
            ILogger<GetFinancialReportsByTickerQueryHandler> logger)
        {
            _uow = uow;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<ApiResponse<PaginatedResponse<SimpleFinancialReportDto>>> Handle(GetFinancialReportsByTickerQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if symbol exists
                var symbol = await _uow.Symbols.GetByIdAsync(request.Ticker, cancellationToken);
                if (symbol == null)
                {
                    return ApiResponse<PaginatedResponse<SimpleFinancialReportDto>>.Failure(
                        $"Không tìm thấy mã chứng khoán {request.Ticker}.");
                }

                var (reports, totalCount) = await _uow.FinancialReports.GetByTickerPaginatedAsync(
                    request.Ticker,
                    request.PageIndex,
                    request.PageSize,
                    cancellationToken);

                var dtos = reports.Select(r => new SimpleFinancialReportDto
                {
                    Id = r.Id,
                    Ticker = r.Ticker,
                    Year = r.Year,
                    Period = r.Period,
                    FileUrl = r.FilePath != null ? _fileStorageService.GetFileUrl(r.FilePath) : null,
                    FileSize = r.FileSize,
                    Revenue = r.Revenue,
                    NetProfit = r.NetProfit,
                    ProfitAfterTax = r.ProfitAfterTax,
                    TotalAssets = r.TotalAssets,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt
                }).ToList();

                var paginatedResponse = PaginatedResponse<SimpleFinancialReportDto>.Create(
                    dtos,
                    totalCount,
                    request.PageIndex,
                    request.PageSize);

                return ApiResponse<PaginatedResponse<SimpleFinancialReportDto>>.Success(
                    paginatedResponse,
                    "Lấy danh sách báo cáo tài chính thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting financial reports by ticker {Ticker}", request.Ticker);
                throw;
            }
        }
    }
}
