using GreenDragonTrading.Application.DTOs;

namespace GreenDragonTrading.Application.Interfaces;

/// <summary>
/// Service interface for DNSE API integration
/// </summary>
public interface IDnseService
{
    /// <summary>
    /// Get financial report details from DNSE API
    /// </summary>
    /// <param name="ticker">Stock ticker symbol</param>
    /// <param name="reportCode">Report code (e.g., "SHORT_TERM_ASSETS", "TOTAL_ASSETS")</param>
    /// <param name="cycleType">Cycle type: "quy" (quarterly) or "nam" (yearly)</param>
    /// <param name="cycleNumber">Number of cycles to retrieve (5 or 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Financial report response from DNSE</returns>
    Task<DnseFinancialReportResponse?> GetFinancialReportDetailsAsync(
        string ticker,
        string reportCode,
        string cycleType,
        int cycleNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get corporate actions history for a ticker from DNSE API
    /// </summary>
    /// <param name="ticker">Stock ticker symbol</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Corporate actions response</returns>
    Task<DnseCorporateActionsResponse?> GetCorporateActionsHistoryAsync(
        string ticker,
        int page,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get upcoming corporate actions from DNSE API
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Corporate actions response</returns>
    Task<DnseCorporateActionsResponse?> GetCorporateActionsUpcomingAsync(
        int page,
        CancellationToken cancellationToken = default);
}
