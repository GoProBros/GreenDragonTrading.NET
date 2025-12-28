using GreenDragonTrading.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace GreenDragonTrading.Application.DTOs
{
    /// <summary>
    /// Financial report data transfer object
    /// </summary>
    public record FinancialReportDto
    {
        public Guid Id { get; init; }
        public string Ticker { get; init; } = null!;
        public int Year { get; init; }
        public ReportPeriod Period { get; init; }
        public string? FilePath { get; init; }
        public string? FileUrl { get; init; }
        public long? FileSize { get; init; }
        public string? ContentType { get; init; }
        public string? KeyMetrics { get; init; }
        public FinancialReportStatus Status { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset UpdatedAt { get; init; }
    }
}
