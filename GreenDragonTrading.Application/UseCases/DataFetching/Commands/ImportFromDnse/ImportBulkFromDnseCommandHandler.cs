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

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportFromDnse;

public class ImportBulkFromDnseCommandHandler(
    IUnitOfWork uow,
    IDnseService dnseService,
    IDnseDataMapper mapper,
    ILogger<ImportBulkFromDnseCommandHandler> logger) : IRequestHandler<ImportBulkFromDnseCommand, ApiResponse<DnseImportResult>>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IDnseService _dnseService = dnseService;
    private readonly IDnseDataMapper _mapper = mapper;
    private readonly ILogger<ImportBulkFromDnseCommandHandler> _logger = logger;

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

    public async Task<ApiResponse<DnseImportResult>> Handle(ImportBulkFromDnseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting bulk import from DNSE: CycleType={CycleType}, CycleNumber={CycleNumber}", 
                request.CycleType, request.CycleNumber);

            // Determine which tickers to import
            var tickers = request.Tickers;
            if (tickers == null || tickers.Count == 0)
            {
                // Get all active symbols from database
                var symbols = await _uow.Symbols.GetAllAsync(cancellationToken);
                tickers = symbols.Select(s => s.Ticker).ToList();
                _logger.LogInformation("No tickers specified, importing for all {Count} active symbols", tickers.Count);
            }

            var result = new DnseImportResult
            {
                TotalTickers = tickers.Count,
                SuccessCount = 0,
                FailedCount = 0,
                SuccessTickers = [],
                Errors = []
            };

            // Process each ticker
            foreach (var ticker in tickers)
            {
                try
                {
                    await ProcessTickerAsync(ticker, _defaultReportCodes, request.CycleType, request.CycleNumber, result, cancellationToken);
                    result.SuccessCount++;
                    result.SuccessTickers.Add(ticker);
                    _logger.LogInformation("Successfully imported data for {Ticker}", ticker);
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add(new DnseImportError
                    {
                        Ticker = ticker,
                        ErrorMessage = ex.Message
                    });
                    _logger.LogWarning(ex, "Failed to import data for {Ticker}", ticker);
                }
            }

            _logger.LogInformation("Bulk import completed: Success={Success}, Failed={Failed}", 
                result.SuccessCount, result.FailedCount);

            return ApiResponse<DnseImportResult>.Success(result, 
                $"Import hoàn tất: {result.SuccessCount}/{result.TotalTickers} mã thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk import from DNSE");
            return ApiResponse<DnseImportResult>.Failure("Lỗi khi import dữ liệu từ DNSE");
        }
    }

    private async Task ProcessTickerAsync(
        string ticker,
        List<string> reportCodes,
        string cycleType,
        int cycleNumber,
        DnseImportResult result,
        CancellationToken cancellationToken)
    {
        // Verify symbol exists
        _ = await _uow.Symbols.GetByIdAsync(ticker, cancellationToken) ?? throw new InvalidOperationException($"Symbol {ticker} not found in database");

        // Dictionary to store all report data grouped by period
        var reportDataByPeriod = new Dictionary<string, Dictionary<string, DnseFinancialReportResponse>>();

        // Fetch all report codes for this ticker
        foreach (var reportCode in reportCodes)
        {
            try
            {
                var dnseResponse = await _dnseService.GetFinancialReportDetailsAsync(
                    ticker, reportCode, cycleType, cycleNumber, cancellationToken);

                if (dnseResponse == null || dnseResponse.X.Count == 0)
                {
                    _logger.LogWarning("No data returned from DNSE for {Ticker} - {ReportCode}", ticker, reportCode);
                    continue;
                }

                // Group by period
                foreach (var period in dnseResponse.X)
                {
                    if (!reportDataByPeriod.ContainsKey(period))
                    {
                        reportDataByPeriod[period] = new Dictionary<string, DnseFinancialReportResponse>();
                    }
                    reportDataByPeriod[period][reportCode] = dnseResponse;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(new DnseImportError
                {
                    Ticker = ticker,
                    ReportCode = reportCode,
                    ErrorMessage = ex.Message
                });
                _logger.LogWarning(ex, "Failed to fetch {ReportCode} for {Ticker}", reportCode, ticker);
            }
        }

        // Save each period as a separate financial report
        foreach (var (periodString, reportsData) in reportDataByPeriod)
        {
            try
            {
                var (year, quarter) = DnsePeriodParser.ParsePeriodString(periodString);
                var period = quarter.HasValue ? (ReportPeriod)quarter.Value : ReportPeriod.Yearly;

                // Check if report already exists
                var existingReport = await _uow.FinancialReports.GetByTickerYearPeriodAsync(
                    ticker, year, (int)period, cancellationToken);

                // Map raw DNSE data to structured DTOs for this specific period
                var reportData = _mapper.MapToFinancialReportDataForPeriod(reportsData, periodString);

                if (existingReport != null)
                {
                    // Update existing report
                    existingReport.ReportData = reportData;
                    existingReport.UpdatedAt = DateTimeOffset.UtcNow;
                    _uow.FinancialReports.Update(existingReport);
                }
                else
                {
                    // Create new report
                    var newReport = new FinancialReport
                    {
                        Id = Guid.NewGuid(),
                        Ticker = ticker,
                        Year = year,
                        Period = period,
                        ReportData = reportData,
                        Status = FinancialReportStatus.Completed,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow
                    };
                    await _uow.FinancialReports.AddAsync(newReport, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to save financial report for {Ticker} - {Period}", ticker, periodString);
            }
        }

        await _uow.SaveChangesAsync(cancellationToken);
    }
}
