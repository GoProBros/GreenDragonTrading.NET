using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Infrastructure.Services
{
    /// <summary>
    /// Calculates financial ratios from a raw financial report snapshot.
    /// </summary>
    public class FinancialReportIndicatorCalculationService : IFinancialReportIndicatorCalculationService
    {
        public FinancialReportIndicatorData Calculate(
            FinancialReportData reportData,
            ReportPeriod period,
            FinancialReportData? comparisonReportData = null)
        {
            var netProfit = reportData.IncomeStatement?.ParentCompanyNetProfit?.ParentCompanyNetProfit;
            var netRevenue = reportData.IncomeStatement?.GrossProfit?.NetRevenue;
            var grossProfit = reportData.IncomeStatement?.GrossProfit?.GrossProfit;
            var operatingCashFlow = reportData.CashFlowStatement?.OperatingActivities;
            var operatingProfit = reportData.IncomeStatement?.ProfitBeforeTax?.OperatingProfit;
            var interestExpenses = reportData.IncomeStatement?.Expenses?.InterestExpenses;
            var fixedAssets = reportData.BalanceSheet?.LongTermAssets?.FixedAssets;
            var retainedEarnings = reportData.BalanceSheet?.Equity?.RetainedEarnings;
            var cash = reportData.BalanceSheet?.ShortTermAssets?.Cash;

            var totalEquity = CalculateTotalEquity(reportData);
            var totalAssets = CalculateTotalAssets(reportData);
            var totalLiabilities = CalculateTotalLiabilities(reportData);
            var shortTermLiabilities = reportData.BalanceSheet?.Liabilities?.ShortTerm;
            var totalBorrowings = CalculateTotalBorrowings(reportData);
            var longTermDebt = reportData.BalanceSheet?.Borrowings?.LongTermBorrowings
                ?? reportData.BalanceSheet?.Liabilities?.LongTerm;
            var inventory = reportData.BalanceSheet?.ShortTermAssets?.Inventories;
            var quickAssets = CalculateQuickAssets(reportData);

            var costOfGoodsSold = reportData.IncomeStatement?.GrossProfit?.CostOfGoodsSold
                ?? reportData.IncomeStatement?.Expenses?.CostOfGoodsSold;

            var securitiesRevenueTotal = CalculateSecuritiesRevenueTotal(reportData);
            var bankOperatingIncomeTotal = CalculateBankTotalOperatingIncome(reportData);
            var netMarginRevenue = securitiesRevenueTotal ?? bankOperatingIncomeTotal ?? netRevenue;
            var operatingMarginRevenue = securitiesRevenueTotal ?? bankOperatingIncomeTotal ?? netRevenue;
            var interestCoverageProfitBase = operatingProfit
                ?? reportData.IncomeStatement?.ProfitBeforeTax?.ProfitBeforeTax;

            var comparisonType = period == ReportPeriod.Yearly ? "YoY" : "QoQ";
            var comparisonGrossProfit = comparisonReportData?.IncomeStatement?.GrossProfit?.GrossProfit;
            var comparisonRevenue = CalculateRevenueForGrowth(comparisonReportData);
            var currentRevenue = CalculateRevenueForGrowth(reportData);

            var bankNetInterestIncome = reportData.IncomeStatement?.BankOperatingIncome?.NetInterestIncome;
            var bankEarningAssets = CalculateBankEarningAssets(reportData);
            var nonInterestIncome = CalculateNonInterestIncome(reportData);
            var totalOperatingIncome = CalculateBankTotalOperatingIncome(reportData);

            return new FinancialReportIndicatorData
            {
                Profitability = new ProfitabilityRatiosDto
                {
                    GrossMargin = SafeDivide(grossProfit, netRevenue),
                    OperatingProfitMargin = SafeDivide(operatingProfit, operatingMarginRevenue),
                    NetMargin = SafeDivide(netProfit, netMarginRevenue),
                    Roe = SafeDivide(netProfit, totalEquity),
                    Roa = SafeDivide(netProfit, totalAssets),
                    ReturnOnFixedAssets = SafeDivide(netProfit, fixedAssets)
                },
                LiquidityAndSolvency = new LiquidityAndSolvencyRatiosDto
                {
                    CurrentRatio = SafeDivide(reportData.BalanceSheet?.ShortTermAssets?.TotalShortTermAssets, shortTermLiabilities),
                    QuickRatio = SafeDivide(quickAssets, shortTermLiabilities),
                    CashRatio = SafeDivide(cash, shortTermLiabilities),
                    DebtToEquity = SafeDivide(totalBorrowings, totalEquity),
                    DebtRatio = SafeDivide(totalLiabilities, totalAssets),
                    LongTermDebtRatio = SafeDivide(longTermDebt, totalAssets),
                    InterestCoverageRatio = SafeDivide(interestCoverageProfitBase, interestExpenses),
                    RetainedEarningsToTotalAssets = SafeDivide(retainedEarnings, totalAssets)
                },
                Efficiency = new EfficiencyRatiosDto
                {
                    TotalAssetTurnover = SafeDivide(netMarginRevenue, totalAssets),
                    InventoryTurnover = SafeDivide(costOfGoodsSold, inventory)
                },
                Growth = new GrowthRatiosDto
                {
                    ComparisonType = comparisonType,
                    GrossProfitGrowth = SafeGrowth(grossProfit, comparisonGrossProfit),
                    RevenueGrowth = SafeGrowth(currentRevenue, comparisonRevenue)
                },
                BankSpecific = new BankSpecificRatiosDto
                {
                    Nim = SafeDivide(bankNetInterestIncome, bankEarningAssets),
                    NonInterestIncomeRatio = SafeDivide(nonInterestIncome, totalOperatingIncome)
                },
                CashFlow = new CashFlowRatiosDto
                {
                    OperatingCashFlowToNetProfit = SafeDivide(operatingCashFlow, netProfit)
                },
                CalculatedAt = DateTimeOffset.UtcNow
            };
        }

        private static decimal? CalculateSecuritiesRevenueTotal(FinancialReportData reportData)
        {
            var securitiesRevenue = reportData.IncomeStatement?.SecuritiesRevenue;
            if (securitiesRevenue == null)
            {
                return null;
            }

            var hasAnyValue = securitiesRevenue.BrokerageAndCustodyRevenue.HasValue
                || securitiesRevenue.LendingRevenue.HasValue
                || securitiesRevenue.TradingAndCapitalRevenue.HasValue
                || securitiesRevenue.InvestmentBankingRevenue.HasValue;

            if (!hasAnyValue)
            {
                return null;
            }

            return (securitiesRevenue.BrokerageAndCustodyRevenue ?? 0m)
                + (securitiesRevenue.LendingRevenue ?? 0m)
                + (securitiesRevenue.TradingAndCapitalRevenue ?? 0m)
                + (securitiesRevenue.InvestmentBankingRevenue ?? 0m);
        }

        private static decimal? CalculateRevenueForGrowth(FinancialReportData? reportData)
        {
            if (reportData == null)
            {
                return null;
            }

            var securitiesRevenue = CalculateSecuritiesRevenueTotal(reportData);
            var bankOperatingIncome = CalculateBankTotalOperatingIncome(reportData);
            return securitiesRevenue ?? bankOperatingIncome ?? reportData.IncomeStatement?.GrossProfit?.NetRevenue;
        }

        private static decimal? CalculateTotalEquity(FinancialReportData reportData)
        {
            var equity = reportData.BalanceSheet?.Equity;
            if (equity == null)
            {
                return null;
            }

            var hasAnyValue = equity.ContributedCapital.HasValue
                || equity.RetainedEarnings.HasValue
                || equity.OtherCapital.HasValue
                || equity.CreditInstitutionFunds.HasValue
                || equity.TreasuryShares.HasValue;

            if (!hasAnyValue)
            {
                return null;
            }

            return (equity.ContributedCapital ?? 0m)
                + (equity.RetainedEarnings ?? 0m)
                + (equity.OtherCapital ?? 0m)
                + (equity.CreditInstitutionFunds ?? 0m)
                + (equity.TreasuryShares ?? 0m);
        }

        private static decimal? CalculateTotalAssets(FinancialReportData reportData)
        {
            var shortTermAssets = reportData.BalanceSheet?.ShortTermAssets?.TotalShortTermAssets;
            var longTermAssets = reportData.BalanceSheet?.LongTermAssets?.TotalLongTermAssets;

            if (shortTermAssets.HasValue || longTermAssets.HasValue)
            {
                return (shortTermAssets ?? 0m) + (longTermAssets ?? 0m);
            }

            var bankShortTermAssets = reportData.BalanceSheet?.BankAssets?.TotalShortTermAssets;
            var bankLongTermAssets = reportData.BalanceSheet?.BankAssets?.TotalLongTermAssets;

            if (bankShortTermAssets.HasValue || bankLongTermAssets.HasValue)
            {
                return (bankShortTermAssets ?? 0m) + (bankLongTermAssets ?? 0m);
            }

            // Fallback: sum detailed BankAssets fields when total fields are not available
            var bankAssets = reportData.BalanceSheet?.BankAssets;
            if (bankAssets != null)
            {
                var hasDetailedAssets = bankAssets.DepositsAtCentralBank.HasValue
                    || bankAssets.DepositsAtOtherCreditInstitutions.HasValue
                    || bankAssets.TradingSecurities.HasValue
                    || bankAssets.LoansToCustomers.HasValue
                    || bankAssets.InvestmentSecurities.HasValue
                    || bankAssets.OtherAssets.HasValue;

                if (hasDetailedAssets)
                {
                    return (bankAssets.DepositsAtCentralBank ?? 0m)
                        + (bankAssets.DepositsAtOtherCreditInstitutions ?? 0m)
                        + (bankAssets.TradingSecurities ?? 0m)
                        + (bankAssets.LoansToCustomers ?? 0m)
                        + (bankAssets.InvestmentSecurities ?? 0m)
                        + (bankAssets.OtherAssets ?? 0m);
                }
            }

            return null;
        }

        private static decimal? CalculateTotalBorrowings(FinancialReportData reportData)
        {
            var borrowings = reportData.BalanceSheet?.Borrowings;
            if (borrowings == null)
            {
                return null;
            }

            if (!borrowings.ShortTermBorrowings.HasValue && !borrowings.LongTermBorrowings.HasValue)
            {
                return null;
            }

            return (borrowings.ShortTermBorrowings ?? 0m) + (borrowings.LongTermBorrowings ?? 0m);
        }

        private static decimal? CalculateTotalLiabilities(FinancialReportData reportData)
        {
            var liabilities = reportData.BalanceSheet?.Liabilities;
            if (liabilities == null)
            {
                return null;
            }

            if (liabilities.ShortTerm.HasValue || liabilities.LongTerm.HasValue)
            {
                return (liabilities.ShortTerm ?? 0m) + (liabilities.LongTerm ?? 0m);
            }

            var hasDetailedLiabilityValues = liabilities.GovAndCentralBankDebt.HasValue
                || liabilities.BorrowingsFromOtherCreditInstitutions.HasValue
                || liabilities.CustomerDeposits.HasValue
                || liabilities.IssuedValuePapers.HasValue
                || liabilities.OtherLiabilities.HasValue;

            if (!hasDetailedLiabilityValues)
            {
                return null;
            }

            return (liabilities.GovAndCentralBankDebt ?? 0m)
                + (liabilities.BorrowingsFromOtherCreditInstitutions ?? 0m)
                + (liabilities.CustomerDeposits ?? 0m)
                + (liabilities.IssuedValuePapers ?? 0m)
                + (liabilities.OtherLiabilities ?? 0m);
        }

        private static decimal? CalculateQuickAssets(FinancialReportData reportData)
        {
            var shortTermAssets = reportData.BalanceSheet?.ShortTermAssets;
            if (shortTermAssets == null)
            {
                return null;
            }

            var hasAnyValue = shortTermAssets.Cash.HasValue
                || shortTermAssets.FinancialInvestments.HasValue
                || shortTermAssets.Receivables.HasValue;

            if (!hasAnyValue)
            {
                return null;
            }

            return (shortTermAssets.Cash ?? 0m)
                + (shortTermAssets.FinancialInvestments ?? 0m)
                + (shortTermAssets.Receivables ?? 0m);
        }

        private static decimal? CalculateBankEarningAssets(FinancialReportData reportData)
        {
            var bankAssets = reportData.BalanceSheet?.BankAssets;
            if (bankAssets == null)
            {
                return null;
            }

            var hasDetailedAssets = bankAssets.DepositsAtCentralBank.HasValue
                || bankAssets.DepositsAtOtherCreditInstitutions.HasValue
                || bankAssets.TradingSecurities.HasValue
                || bankAssets.LoansToCustomers.HasValue
                || bankAssets.InvestmentSecurities.HasValue;

            if (hasDetailedAssets)
            {
                return (bankAssets.DepositsAtCentralBank ?? 0m)
                    + (bankAssets.DepositsAtOtherCreditInstitutions ?? 0m)
                    + (bankAssets.TradingSecurities ?? 0m)
                    + (bankAssets.LoansToCustomers ?? 0m)
                    + (bankAssets.InvestmentSecurities ?? 0m);
            }

            if (bankAssets.TotalShortTermAssets.HasValue || bankAssets.TotalLongTermAssets.HasValue)
            {
                return (bankAssets.TotalShortTermAssets ?? 0m) + (bankAssets.TotalLongTermAssets ?? 0m);
            }

            return null;
        }

        private static decimal? CalculateNonInterestIncome(FinancialReportData reportData)
        {
            var bankIncome = reportData.IncomeStatement?.BankOperatingIncome;
            if (bankIncome == null)
            {
                return null;
            }

            var hasAnyValue = bankIncome.ServiceFeeIncome.HasValue
                || bankIncome.TradingIncome.HasValue
                || bankIncome.OtherIncome.HasValue;

            if (!hasAnyValue)
            {
                return null;
            }

            return (bankIncome.ServiceFeeIncome ?? 0m)
                + (bankIncome.TradingIncome ?? 0m)
                + (bankIncome.OtherIncome ?? 0m);
        }

        private static decimal? CalculateBankTotalOperatingIncome(FinancialReportData reportData)
        {
            var bankIncome = reportData.IncomeStatement?.BankOperatingIncome;
            if (bankIncome == null)
            {
                return null;
            }

            var hasAnyValue = bankIncome.NetInterestIncome.HasValue
                || bankIncome.ServiceFeeIncome.HasValue
                || bankIncome.TradingIncome.HasValue
                || bankIncome.OtherIncome.HasValue;

            if (!hasAnyValue)
            {
                return null;
            }

            return (bankIncome.NetInterestIncome ?? 0m)
                + (bankIncome.ServiceFeeIncome ?? 0m)
                + (bankIncome.TradingIncome ?? 0m)
                + (bankIncome.OtherIncome ?? 0m);
        }

        private static decimal? SafeDivide(decimal? numerator, decimal? denominator)
        {
            if (!numerator.HasValue || !denominator.HasValue || denominator.Value == 0)
            {
                return null;
            }

            return numerator.Value / denominator.Value;
        }

        private static decimal? SafeGrowth(decimal? currentValue, decimal? previousValue)
        {
            if (!currentValue.HasValue || !previousValue.HasValue || previousValue.Value == 0)
            {
                return null;
            }

            return (currentValue.Value - previousValue.Value) / Math.Abs(previousValue.Value);
        }
    }
}
