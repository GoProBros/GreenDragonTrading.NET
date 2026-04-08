using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Application.Interfaces
{
    /// <summary>
    /// Calculates derived financial indicators from report data.
    /// </summary>
    public interface IFinancialReportIndicatorCalculationService
    {
        FinancialReportIndicatorData Calculate(
            FinancialReportData reportData,
            ReportPeriod period,
            FinancialReportData? comparisonReportData = null);
    }
}
