using GreenDragonTrading.Domain.Entities;
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
        
        /// <summary>
        /// Dữ liệu báo cáo tài chính dạng JSON
        /// </summary>
        public FinancialReportData ReportData { get; init; } = new();

        /// <summary>
        /// Dữ liệu chỉ số tài chính tính toán sẵn dạng JSON
        /// </summary>
        public FinancialReportIndicatorData? IndicatorData { get; init; }
        
        public string? FilePath { get; init; }
        public string? FileUrl { get; init; }
        public long? FileSize { get; init; }
        public FinancialReportStatus Status { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset UpdatedAt { get; init; }
    }

    /// <summary>
    /// Request model for creating financial report
    /// </summary>
    public record CreateFinancialReportRequest
    {
        public string Ticker { get; init; } = null!;
        public int Year { get; init; }
        public int Quarter { get; init; }
        public ReportPeriod Period { get; init; }
        
        /// <summary>
        /// Dữ liệu báo cáo tài chính (JSON structure)
        /// </summary>
        public FinancialReportData ReportData { get; init; } = new();
    }

    /// <summary>
    /// Request model for updating financial report
    /// </summary>
    public record UpdateFinancialReportRequest
    {
        /// <summary>
        /// Dữ liệu báo cáo tài chính (JSON structure)
        /// </summary>
        public FinancialReportData? ReportData { get; init; }
        
        public FinancialReportStatus? Status { get; init; }
    }
}
