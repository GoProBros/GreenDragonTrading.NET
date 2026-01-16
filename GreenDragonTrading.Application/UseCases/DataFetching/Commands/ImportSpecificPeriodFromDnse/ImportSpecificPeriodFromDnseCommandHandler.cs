using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Common.Utils;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants.DNSE;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSpecificPeriodFromDnse;

public class ImportSpecificPeriodFromDnseCommandHandler(
    IUnitOfWork uow,
    IDnseService dnseService,
    IDnseDataMapper mapper,
    ILogger<ImportSpecificPeriodFromDnseCommandHandler> logger) : IRequestHandler<ImportSpecificPeriodFromDnseCommand, ApiResponse<FinancialReportData>>
{
    private readonly IDnseService _dnseService = dnseService;
    private readonly IDnseDataMapper _mapper = mapper;
    private readonly ILogger<ImportSpecificPeriodFromDnseCommandHandler> _logger = logger;
    private readonly IUnitOfWork _uow = uow;

    // All 17 report codes from DNSE API - import all by default
    private static readonly List<string> _defaultReportCodes =
    [
        DnseConstants.ReportCodes.SHORT_TERM_ASSETS,
        DnseConstants.ReportCodes.TOTAL_ASSETS,
        DnseConstants.ReportCodes.LONG_TERM_ASSETS,
        DnseConstants.ReportCodes.ACCOUNTS_PAYABLE,
        DnseConstants.ReportCodes.OWNERS_EQUITY,
        DnseConstants.ReportCodes.OPERATING_INCOME,
        DnseConstants.ReportCodes.INSURANCE_BUSINESS_PROFIT,
        DnseConstants.ReportCodes.PROFIT_BEFORE_TAX,
        DnseConstants.ReportCodes.PROFIT_AFTER_TAX_PARENT_COMPANY,
        DnseConstants.ReportCodes.CASH_FLOW,
        DnseConstants.ReportCodes.SHORT_TERM_FINANCIAL_ASSETS,
        DnseConstants.ReportCodes.INVESTMENT_ASSETS,
        DnseConstants.ReportCodes.BORROWINGS,
        DnseConstants.ReportCodes.MAIN_BUSINESS_OPERATING_PROFIT,
        DnseConstants.ReportCodes.OPERATING_EXPENSES,
        DnseConstants.ReportCodes.PROFIT_AFTER_TAX_AND_AFS,
        DnseConstants.ReportCodes.GROSS_PROFIT
    ];

    public async Task<ApiResponse<FinancialReportData>> Handle(ImportSpecificPeriodFromDnseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Importing specific period from DNSE: {Ticker}, Year={Year}, Quarter={Quarter}", 
                request.Ticker, request.Year, request.Quarter);

            // Verify symbol exists
            var symbol = await _uow.Symbols.GetByIdAsync(request.Ticker, cancellationToken);
            if (symbol == null)
            {
                return ApiResponse<FinancialReportData>.Failure($"Không tìm thấy mã chứng khoán {request.Ticker}");
            }

            // Determine cycle type and format period string
            var cycleType = (request.Quarter == ReportPeriod.Yearly) ? DnseConstants.CycleType.YEARLY : DnseConstants.CycleType.QUARTERLY;
            var targetPeriodString = cycleType == DnseConstants.CycleType.QUARTERLY ? DnsePeriodParser.FormatPeriod(request.Year, (int?) request.Quarter) : DnsePeriodParser.FormatPeriod(request.Year, null);

            var cycleNumber = 10;

            var reportsData = new Dictionary<string, DnseFinancialReportResponse>();
            var errors = new List<string>();

            // Fetch all report codes
            foreach (var reportCode in _defaultReportCodes)
            {
                try
                {
                    var dnseResponse = await _dnseService.GetFinancialReportDetailsAsync(
                        request.Ticker, reportCode, cycleType, cycleNumber, cancellationToken);

                    if (dnseResponse == null || dnseResponse.X.Count == 0)
                    {
                        _logger.LogWarning("No data returned from DNSE for {Ticker} - {ReportCode}", request.Ticker, reportCode);
                        continue;
                    }

                    // Check if target period exists in response
                    var periodIndex = dnseResponse.X.FindIndex(p => p == targetPeriodString);
                    if (periodIndex >= 0)
                    {
                        reportsData[reportCode] = dnseResponse;
                    }
                    else
                    {
                        _logger.LogWarning("Period {Period} not found in DNSE response for {Ticker} - {ReportCode}", 
                            targetPeriodString, request.Ticker, reportCode);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"{reportCode}: {ex.Message}");
                    _logger.LogWarning(ex, "Failed to fetch {ReportCode} for {Ticker}", reportCode, request.Ticker);
                }
            }

            if (reportsData.Count == 0)
            {
                return ApiResponse<FinancialReportData>.Failure(
                    $"Không tìm thấy dữ liệu cho kỳ {targetPeriodString} của {request.Ticker}");
            }

            var reportData = _mapper.MapToFinancialReportDataForPeriod(reportsData, targetPeriodString);

            return ApiResponse<FinancialReportData>.Success(reportData, "Lấy dữ liệu thành công!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing specific period from DNSE for {Ticker}", request.Ticker);
            return ApiResponse<FinancialReportData>.Failure("Lỗi khi import dữ liệu từ DNSE");
        }
    }
}
