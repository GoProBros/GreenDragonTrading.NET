using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants.DNSE;
using GreenDragonTrading.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Infrastructure.Services;

/// <summary>
/// Maps DNSE API raw data to structured financial DTOs
/// </summary>
public class DnseDataMapper : IDnseDataMapper
{
    private readonly ILogger<DnseDataMapper> _logger;

    public DnseDataMapper(ILogger<DnseDataMapper> logger)
    {
        _logger = logger;
    }

    public FinancialReportData MapToFinancialReportData(Dictionary<string, DnseFinancialReportResponse> dnseResponses)
    {
        var reportData = new FinancialReportData();

        // Map Balance Sheet
        reportData.BalanceSheet = MapBalanceSheet(dnseResponses);

        // Map Income Statement
        reportData.IncomeStatement = MapIncomeStatement(dnseResponses);

        // Map Cash Flow Statement
        reportData.CashFlowStatement = MapCashFlowStatement(dnseResponses);

        return reportData;
    }

    public FinancialReportData MapToFinancialReportDataForPeriod(
        Dictionary<string, DnseFinancialReportResponse> dnseResponses,
        string targetPeriod)
    {
        // Create filtered responses with only the target period's data
        var filteredResponses = new Dictionary<string, DnseFinancialReportResponse>();

        foreach (var (reportCode, response) in dnseResponses)
        {
            var periodIndex = response.X.IndexOf(targetPeriod);
            if (periodIndex == -1)
            {
                _logger.LogWarning("Period {Period} not found in report {ReportCode}", targetPeriod, reportCode);
                continue;
            }

            // Create new response with single period
            var filteredResponse = new DnseFinancialReportResponse
            {
                X = new List<string> { targetPeriod },
                Type = response.Type,
                Data = response.Data.Select(series => new DnseDataSeries
                {
                    Id = series.Id,
                    Label = series.Label,
                    Type = series.Type,
                    Tooltip = series.Tooltip,
                    Y = periodIndex < series.Y.Count
                        ? new List<decimal> { series.Y[periodIndex] }
                        : new List<decimal> { 0 },
                    YAxisPosition = series.YAxisPosition
                }).ToList()
            };

            filteredResponses[reportCode] = filteredResponse;
        }

        return MapToFinancialReportData(filteredResponses);
    }

    private BalanceSheetDto MapBalanceSheet(Dictionary<string, DnseFinancialReportResponse> dnseResponses)
    {
        var balanceSheet = new BalanceSheetDto();

        // 1. Short Term Assets
        if (dnseResponses.TryGetValue(DnseConstants.ReportCodes.SHORT_TERM_ASSETS, out var shortTermAssets))
        {
            balanceSheet.ShortTermAssets = new ShortTermAssetsDto
            {
                Cash = GetValueByLabel(shortTermAssets, DnseLabels.ShortTermAssets.Cash),
                FinancialInvestments = GetValueByLabel(shortTermAssets, DnseLabels.ShortTermAssets.FinancialInvestments),
                Receivables = GetValueByLabel(shortTermAssets, DnseLabels.ShortTermAssets.Receivables),
                OtherAssets = GetValueByLabel(shortTermAssets, DnseLabels.ShortTermAssets.OtherAssets),
                Inventories = GetValueByLabel(shortTermAssets, DnseLabels.ShortTermAssets.Inventories),
                TotalShortTermAssets = GetValueByLabel(shortTermAssets, DnseLabels.ShortTermAssets.TotalShortTermAssets)
                    ?? GetValueByLabel(shortTermAssets, DnseLabels.ShortTermAssets.Cash) ?? 0
                        + GetValueByLabel(shortTermAssets, DnseLabels.ShortTermAssets.FinancialInvestments) ?? 0
                        + GetValueByLabel(shortTermAssets, DnseLabels.ShortTermAssets.Receivables) ?? 0
                        + GetValueByLabel(shortTermAssets, DnseLabels.ShortTermAssets.OtherAssets) ?? 0
                        + GetValueByLabel(shortTermAssets, DnseLabels.ShortTermAssets.Inventories) ?? 0
            };
        }

        // 2. Long Term Assets
        if (dnseResponses.TryGetValue(DnseConstants.ReportCodes.LONG_TERM_ASSETS, out var longTermAssets))
        {
            balanceSheet.LongTermAssets = new LongTermAssetsDto
            {
                FinancialInvestments = GetValueByLabel(longTermAssets, DnseLabels.LongTermAssets.FinancialInvestments),
                FixedAssets = GetValueByLabel(longTermAssets, DnseLabels.LongTermAssets.FixedAssets),
                InvestmentProperty = GetValueByLabel(longTermAssets, DnseLabels.LongTermAssets.InvestmentProperty),
                LongTermAssetsInProgress = GetValueByLabel(longTermAssets, DnseLabels.LongTermAssets.LongTermAssetsInProgress),
                OtherAssets = GetValueByLabel(longTermAssets, DnseLabels.LongTermAssets.OtherAssets),
                Receivables = GetValueByLabel(longTermAssets, DnseLabels.LongTermAssets.Receivables),
                TotalLongTermAssets = GetValueByLabel(longTermAssets, DnseLabels.LongTermAssets.TotalLongTermAssets)
                    ?? GetValueByLabel(shortTermAssets, DnseLabels.ShortTermAssets.Cash) ?? 0
                            + GetValueByLabel(shortTermAssets, DnseLabels.LongTermAssets.FinancialInvestments) ?? 0
                            + GetValueByLabel(shortTermAssets, DnseLabels.LongTermAssets.FixedAssets) ?? 0
                            + GetValueByLabel(shortTermAssets, DnseLabels.LongTermAssets.InvestmentProperty) ?? 0
                            + GetValueByLabel(shortTermAssets, DnseLabels.LongTermAssets.LongTermAssetsInProgress) ?? 0
                            + GetValueByLabel(shortTermAssets, DnseLabels.LongTermAssets.OtherAssets) ?? 0
                            + GetValueByLabel(shortTermAssets, DnseLabels.LongTermAssets.Receivables) ?? 0
            };
        }

        // 3. Total Assets (for banks & finance)
        if (dnseResponses.TryGetValue(DnseConstants.ReportCodes.TOTAL_ASSETS, out var bankAssets))
        {
            balanceSheet.BankAssets = new BankAssetsDto
            {
                DepositsAtCentralBank = GetValueByLabel(bankAssets, DnseLabels.BankAssets.DepositsAtCentralBank),
                DepositsAtOtherCreditInstitutions = GetValueByLabel(bankAssets, DnseLabels.BankAssets.DepositsAtOtherCreditInstitutions),
                TradingSecurities = GetValueByLabel(bankAssets, DnseLabels.BankAssets.TradingSecurities),
                LoansToCustomers = GetValueByLabel(bankAssets, DnseLabels.BankAssets.LoansToCustomers),
                InvestmentSecurities = GetValueByLabel(bankAssets, DnseLabels.BankAssets.InvestmentSecurities),
                OtherAssets = GetValueByLabel(bankAssets, DnseLabels.BankAssets.OtherAssets),
                TotalShortTermAssets = GetValueByLabel(bankAssets, DnseLabels.BankAssets.TotalShortTermAssets),
                TotalLongTermAssets = GetValueByLabel(bankAssets, DnseLabels.BankAssets.TotalLongTermAssets),
            };
        }

        // 4. Short Term Financial Assets (for securities companies)
        if (dnseResponses.TryGetValue(DnseConstants.ReportCodes.SHORT_TERM_FINANCIAL_ASSETS, out var stFinAssets))
        {
            balanceSheet.ShortTermFinancialAssets = new ShortTermFinancialAssetsDto
            {
                Cash = GetValueByLabel(stFinAssets, DnseLabels.ShortTermFinancialAssets.Cash),
                Loans = GetValueByLabel(stFinAssets, DnseLabels.ShortTermFinancialAssets.Loans),
                Other = GetValueByLabel(stFinAssets, DnseLabels.ShortTermFinancialAssets.Other),
                TradingAndCapitalAssets = GetValueByLabel(stFinAssets, DnseLabels.ShortTermFinancialAssets.TradingAndCapitalAssets)
            };
        }

        // 5. Trading and Capital Assets (for securities companies)
        if (dnseResponses.TryGetValue(DnseConstants.ReportCodes.INVESTMENT_ASSETS, out var invAssets))
        {
            balanceSheet.TradingAndCapitalAssets = new TradingAndCapitalAssetsDto
            {
                HeldToMaturity = GetValueByLabel(invAssets, DnseLabels.TradingAndCapitalAssets.HeldToMaturity),
                AvailableForSale = GetValueByLabel(invAssets, DnseLabels.TradingAndCapitalAssets.AvailableForSale),
                FVTPL = GetValueByLabel(invAssets, DnseLabels.TradingAndCapitalAssets.FVTPL)
            };
        }

        // 6. Liabilities
        if (dnseResponses.TryGetValue(DnseConstants.ReportCodes.ACCOUNTS_PAYABLE, out var liabilities))
        {
            balanceSheet.Liabilities = new LiabilitiesDto
            {
                ShortTerm = GetValueByLabel(liabilities, DnseLabels.Liabilities.ShortTerm),
                LongTerm = GetValueByLabel(liabilities, DnseLabels.Liabilities.LongTerm),
                BorrowingsFromOtherCreditInstitutions = GetValueByLabel(liabilities, DnseLabels.Liabilities.BorrowingsFromOtherCreditInstitutions),
                CustomerDeposits = GetValueByLabel(liabilities, DnseLabels.Liabilities.CustomerDeposits),
                GovAndCentralBankDebt = GetValueByLabel(liabilities, DnseLabels.Liabilities.GovAndCentralBankDebt),
                IssuedValuePapers = GetValueByLabel(liabilities, DnseLabels.Liabilities.IssuedValuePapers),
                OtherLiabilities = GetValueByLabel(liabilities, DnseLabels.Liabilities.OtherLiabilities),
            };
        }

        // 7. Borrowings
        if (dnseResponses.TryGetValue(DnseConstants.ReportCodes.BORROWINGS, out var borrowings))
        {
            balanceSheet.Borrowings = new BorrowingsDto
            {
                ShortTermBorrowings = GetValueByLabel(borrowings, DnseLabels.Borrowings.ShortTermBorrowings),
                LongTermBorrowings = GetValueByLabel(borrowings, DnseLabels.Borrowings.LongTermBorrowings)
            };
        }

        // 8. Equity
        if (dnseResponses.TryGetValue(DnseConstants.ReportCodes.OWNERS_EQUITY, out var equity))
        {
            balanceSheet.Equity = new EquityDto
            {
                ContributedCapital = GetValueByLabel(equity, DnseLabels.Equity.ContributedCapital),
                RetainedEarnings = GetValueByLabel(equity, DnseLabels.Equity.RetainedEarnings),
                TreasuryShares = GetValueByLabel(equity, DnseLabels.Equity.TreasuryShares),
                OtherCapital = GetValueByLabel(equity, DnseLabels.Equity.OtherCapital),
                CreditInstitutionFunds = GetValueByLabel(equity, DnseLabels.Equity.CreditInstitutionFunds),
            };
        }

        return balanceSheet;
    }

    private IncomeStatementDto MapIncomeStatement(Dictionary<string, DnseFinancialReportResponse> dnseResponses)
    {
        var incomeStatement = new IncomeStatementDto();

        // 9. Bank Operating Income
        if (dnseResponses.TryGetValue(DnseConstants.ReportCodes.OPERATING_INCOME, out var bankOpIncome))
        {
            incomeStatement.BankOperatingIncome = new BankOperatingIncomeDto
            {
                NetInterestIncome = GetValueByLabel(bankOpIncome, DnseLabels.BankOperatingIncome.NetInterestIncome),
                ServiceFeeIncome = GetValueByLabel(bankOpIncome, DnseLabels.BankOperatingIncome.ServiceFeeIncome),
                TradingIncome = GetValueByLabel(bankOpIncome, DnseLabels.BankOperatingIncome.TradingIncome),
                OtherIncome = GetValueByLabel(bankOpIncome, DnseLabels.BankOperatingIncome.OtherIncome)
            };
        }

        // 10. Insurance Business
        if (dnseResponses.TryGetValue(DnseConstants.ReportCodes.INSURANCE_BUSINESS_PROFIT, out var insuranceBiz))
        {
            incomeStatement.InsuranceBusiness = new InsuranceBusinessDto
            {
                OperatingProfit = GetValueByLabel(insuranceBiz, DnseLabels.InsuranceBusiness.OperatingProfit),
                NetOperatingRevenue = GetValueByLabel(insuranceBiz, DnseLabels.InsuranceBusiness.NetOperatingRevenue),
                OperatingExpenses = GetValueByLabel(insuranceBiz, DnseLabels.InsuranceBusiness.OperatingExpenses)
            };
        }

        // 11. Securities Revenue
        if (dnseResponses.TryGetValue(DnseConstants.ReportCodes.MAIN_BUSINESS_OPERATING_PROFIT, out var secRevenue))
        {
            incomeStatement.SecuritiesRevenue = new SecuritiesRevenueDto
            {
                BrokerageAndCustodyRevenue = GetValueByLabel(secRevenue, DnseLabels.SecuritiesRevenue.BrokerageAndCustodyRevenue),
                LendingRevenue = GetValueByLabel(secRevenue, DnseLabels.SecuritiesRevenue.LendingRevenue),
                TradingAndCapitalRevenue = GetValueByLabel(secRevenue, DnseLabels.SecuritiesRevenue.TradingAndCapitalRevenue),
                InvestmentBankingRevenue = GetValueByLabel(secRevenue, DnseLabels.SecuritiesRevenue.InvestmentBankingRevenue)
            };
        }

        // 12. Gross Profit
        if (dnseResponses.TryGetValue(DnseConstants.ReportCodes.GROSS_PROFIT, out var grossProfit))
        {
            incomeStatement.GrossProfit = new GrossProfitDto
            {
                GrossProfit = GetValueByLabel(grossProfit, DnseLabels.GrossProfit.Value),
                NetRevenue = GetValueByLabel(grossProfit, DnseLabels.GrossProfit.NetRevenue),
                CostOfGoodsSold = GetValueByLabel(grossProfit, DnseLabels.GrossProfit.CostOfGoodsSold)
            };
        }

        // 13. Expenses
        if (dnseResponses.TryGetValue(DnseConstants.ReportCodes.OPERATING_EXPENSES, out var expenses))
        {
            incomeStatement.Expenses = new ExpensesDto
            {
                FinancialExpenses = GetValueByLabel(expenses, DnseLabels.Expenses.FinancialExpenses),
                ManagementExpenses = GetValueByLabel(expenses, DnseLabels.Expenses.ManagementExpenses),
                CostOfGoodsSold = GetValueByLabel(expenses, DnseLabels.Expenses.CostOfGoodsSold),
                InterestExpenses = GetValueByLabel(expenses, DnseLabels.Expenses.InterestExpenses),
                SellingExpenses = GetValueByLabel(expenses, DnseLabels.Expenses.SellingExpenses)
            };
        }

        // 14. Profit Before Tax
        if (dnseResponses.TryGetValue(DnseConstants.ReportCodes.PROFIT_BEFORE_TAX, out var profitBeforeTax))
        {
            incomeStatement.ProfitBeforeTax = new ProfitBeforeTaxDto
            {
                ProfitBeforeTax = GetValueByLabel(profitBeforeTax, DnseLabels.ProfitBeforeTax.Value) ?? GetValueByLabel(profitBeforeTax, DnseLabels.ProfitBeforeTax.ValueBank),
                OperatingProfit = GetValueByLabel(profitBeforeTax, DnseLabels.ProfitBeforeTax.OperatingProfit) ?? GetValueByLabel(profitBeforeTax, DnseLabels.ProfitBeforeTax.OperatingProfitAlt) ?? GetValueByLabel(profitBeforeTax, DnseLabels.ProfitBeforeTax.OperatingProfitBank),
                FinancialProfit = GetValueByLabel(profitBeforeTax, DnseLabels.ProfitBeforeTax.FinancialProfit) ?? GetValueByLabel(profitBeforeTax, DnseLabels.ProfitBeforeTax.FinancialProfitAlt),
                OtherProfit = GetValueByLabel(profitBeforeTax, DnseLabels.ProfitBeforeTax.OtherProfit) ?? GetValueByLabel(profitBeforeTax, DnseLabels.ProfitBeforeTax.OtherProfitAlt),
                ManagementExpenses = GetValueByLabel(profitBeforeTax, DnseLabels.ProfitBeforeTax.ManagementExpenses),
                ShareProfitOfAssociatesAndJoint = GetValueByLabel(profitBeforeTax, DnseLabels.ProfitBeforeTax.ShareProfitOfAssociatesAndJoint),
                OperatingExpenses = GetValueByLabel(profitBeforeTax, DnseLabels.ProfitBeforeTax.OperatingExpenses),
                ProvisionExpenses = GetValueByLabel(profitBeforeTax, DnseLabels.ProfitBeforeTax.ProvisionExpenses),
            };
        }

        // 15. Profit After Tax and AFS
        if (dnseResponses.TryGetValue(DnseConstants.ReportCodes.PROFIT_AFTER_TAX_AND_AFS, out var profitAfterTaxAfs))
        {
            incomeStatement.ProfitAfterTaxAndAfs = new ProfitAfterTaxAndAfsDto
            {
                ParentCompanyNetProfit = GetValueByLabel(profitAfterTaxAfs, DnseLabels.ProfitAfterTaxAndAfs.NetProfit),
                AfsGains = GetValueByLabel(profitAfterTaxAfs, DnseLabels.ProfitAfterTaxAndAfs.AfsGains),
                ProfitAfterTaxAndAfs = GetValueByLabel(profitAfterTaxAfs, DnseLabels.ProfitAfterTaxAndAfs.Value)
            };
        }

        // 16. Parent Company Net Profit
        if (dnseResponses.TryGetValue(DnseConstants.ReportCodes.PROFIT_AFTER_TAX_PARENT_COMPANY, out var parentNetProfit))
        {
            incomeStatement.ParentCompanyNetProfit = new ParentCompanyNetProfitDto
            {
                ParentCompanyNetProfit = GetValueByLabel(parentNetProfit, DnseLabels.ParentCompanyNetProfit.Value),
                ProfitBeforeTax = GetValueByLabel(parentNetProfit, DnseLabels.ParentCompanyNetProfit.BeforeTax),
                CorporateIncomeTax = GetValueByLabel(parentNetProfit, DnseLabels.ParentCompanyNetProfit.CorporateIncomeTax),
                MinorityInterests = GetValueByLabel(parentNetProfit, DnseLabels.ParentCompanyNetProfit.MinorityInterests)
            };
        }

        return incomeStatement;
    }

    private CashFlowDto? MapCashFlowStatement(Dictionary<string, DnseFinancialReportResponse> dnseResponses)
    {
        CashFlowDto? cashFlow = null;

        // 17. Cash Flow
        if (dnseResponses.TryGetValue(DnseConstants.ReportCodes.CASH_FLOW, out var cashFlowResponse))
        {
            cashFlow = new CashFlowDto
            {
                NetCashFlow = GetValueByLabel(cashFlowResponse, DnseLabels.CashFlow.NetCashFlow),
                OperatingActivities = GetValueByLabel(cashFlowResponse, DnseLabels.CashFlow.OperatingActivities),
                InvestingActivities = GetValueByLabel(cashFlowResponse, DnseLabels.CashFlow.InvestingActivities),
                FinancingActivities = GetValueByLabel(cashFlowResponse, DnseLabels.CashFlow.FinancingActivities)
            };
        }

        return cashFlow ?? new CashFlowDto();
    }

    /// <summary>
    /// Get value from DNSE response by label (Vietnamese text)
    /// Sums all values across periods if multiple exist
    /// </summary>
    private decimal? GetValueByLabel(DnseFinancialReportResponse response, string label)
    {
        try
        {
            var series = response.Data.FirstOrDefault(d =>
                d.Label.Equals(label, StringComparison.OrdinalIgnoreCase));

            if (series == null || series.Y.Count == 0)
            {
                return null;
            }

            // For single period, return that value
            if (series.Y.Count == 1)
            {
                return series.Y[0];
            }

            // For multiple periods, return the most recent (last) value
            // This assumes X periods are sorted chronologically
            return series.Y[series.Y.Count - 1];
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting value for label {Label}", label);
            return null;
        }
    }
}
