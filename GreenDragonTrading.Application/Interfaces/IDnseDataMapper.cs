using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Application.Interfaces;

/// <summary>
/// Service for mapping DNSE raw data to structured DTOs
/// </summary>
public interface IDnseDataMapper
{
    /// <summary>
    /// Map DNSE responses to FinancialReportData DTOs
    /// </summary>
    /// <param name="dnseResponses">Dictionary of report code to DNSE response</param>
    /// <returns>Mapped FinancialReportData with BalanceSheet, IncomeStatement, and CashFlowStatement</returns>
    FinancialReportData MapToFinancialReportData(Dictionary<string, DnseFinancialReportResponse> dnseResponses);
    
    /// <summary>
    /// Map DNSE response for a specific period
    /// </summary>
    /// <param name="dnseResponses">Dictionary of report code to DNSE response</param>
    /// <param name="targetPeriod">Target period (e.g., "Q3/2024")</param>
    /// <returns>Mapped FinancialReportData for the specific period</returns>
    FinancialReportData MapToFinancialReportDataForPeriod(
        Dictionary<string, DnseFinancialReportResponse> dnseResponses, 
        string targetPeriod);
}
