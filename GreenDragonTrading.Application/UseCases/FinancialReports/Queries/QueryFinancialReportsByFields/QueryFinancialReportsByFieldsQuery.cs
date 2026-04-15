using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Queries.QueryFinancialReportsByFields
{
    /// <summary>
    /// Advanced financial report query supporting field-level filters for both reportData and indicatorData.
    /// </summary>
    public record QueryFinancialReportsByFieldsQuery
        : PaginationQuery, IRequest<ApiResponse<PaginatedResponse<FinancialReportFieldQueryItemDto>>>
    {
        public string? Ticker { get; init; }
        public int? YearFrom { get; init; }
        public int? YearTo { get; init; }
        public ReportPeriod? Period { get; init; }
        public FinancialReportStatus? Status { get; init; }
        public int MaxScanRecords { get; init; } = 5000;
        public string SortBy { get; init; } = "year";
        public string SortDirection { get; init; } = "desc";
        public List<FinancialReportFieldFilterConditionDto> Filters { get; init; } = new();
        public List<string> SelectFields { get; init; } = new();
    }

    public record FinancialReportFieldFilterConditionDto
    {
        public string Path { get; init; } = string.Empty;
        public string Operator { get; init; } = "eq";
        public string? Value { get; init; }
    }

    public record FinancialReportFieldQueryItemDto
    {
        public Guid Id { get; init; }
        public string Ticker { get; init; } = string.Empty;
        public int Year { get; init; }
        public ReportPeriod Period { get; init; }
        public FinancialReportStatus Status { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset UpdatedAt { get; init; }
        public Dictionary<string, object?> Fields { get; init; } = new();
    }
}
