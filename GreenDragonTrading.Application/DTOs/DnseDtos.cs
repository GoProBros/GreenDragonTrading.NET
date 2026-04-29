namespace GreenDragonTrading.Application.DTOs;

/// <summary>
/// DNSE API Response for financial report details
/// </summary>
public record DnseFinancialReportResponse
{
    /// <summary>
    /// Time periods array (e.g., ["Q3/2024", "Q4/2024", "Q1/2025"])
    /// </summary>
    public List<string> X { get; init; } = new();
    
    /// <summary>
    /// Chart type (e.g., "stackedbar", "stackedbar-markupline")
    /// </summary>
    public string Type { get; init; } = string.Empty;
    
    /// <summary>
    /// Data series array
    /// </summary>
    public List<DnseDataSeries> Data { get; init; } = new();
}

/// <summary>
/// Data series in DNSE response
/// </summary>
public record DnseDataSeries
{
    /// <summary>
    /// Series ID
    /// </summary>
    public int Id { get; init; }
    
    /// <summary>
    /// Series label (Vietnamese description)
    /// </summary>
    public string Label { get; init; } = string.Empty;
    
    /// <summary>
    /// Chart type for this series (e.g., "bar", "line")
    /// </summary>
    public string Type { get; init; } = string.Empty;
    
    /// <summary>
    /// Tooltip description
    /// </summary>
    public string Tooltip { get; init; } = string.Empty;
    
    /// <summary>
    /// Data values corresponding to X periods
    /// </summary>
    public List<decimal?> Y { get; init; } = new();
    
    /// <summary>
    /// Y axis position ("left" or "right")
    /// </summary>
    public string YAxisPosition { get; init; } = string.Empty;
}

/// <summary>
/// Request to import financial reports from DNSE
/// </summary>
public record ImportFromDnseRequest
{
    /// <summary>
    /// Cycle type: "quy" (quarterly) or "nam" (yearly)
    /// </summary>
    public string CycleType { get; init; } = "quy";
    
    /// <summary>
    /// Number of cycles to import (5 or 10)
    /// </summary>
    public int CycleNumber { get; init; } = 10;
    
    /// <summary>
    /// List of report codes to import (e.g., ["SHORT_TERM_ASSETS", "TOTAL_ASSETS"])
    /// If null or empty, import all standard reports
    /// </summary>
    public List<string>? ReportCodes { get; init; }
}

/// <summary>
/// Request to import specific period from DNSE
/// </summary>
public record ImportSpecificPeriodFromDnseRequest
{
    /// <summary>
    /// Stock ticker
    /// </summary>
    public string Ticker { get; init; } = null!;
    
    /// <summary>
    /// Year
    /// </summary>
    public int Year { get; init; }
    
    /// <summary>
    /// Quarter (1-4) for quarterly report, null for yearly report
    /// </summary>
    public int? Quarter { get; init; }
    
    /// <summary>
    /// List of report codes to import
    /// If null or empty, import all standard reports
    /// </summary>
    public List<string>? ReportCodes { get; init; }
}

/// <summary>
/// Result of import operation
/// </summary>
public record DnseImportResult
{
    public int TotalTickers { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> SuccessTickers { get; set; } = new();
    public List<DnseImportError> Errors { get; set; } = new();
}

/// <summary>
/// Error details for failed import
/// </summary>
public record DnseImportError
{
    public string Ticker { get; init; } = null!;
    public string? ReportCode { get; init; }
    public string ErrorMessage { get; init; } = null!;
}

/// <summary>
/// DNSE API response for corporate actions
/// </summary>
public record DnseCorporateActionsResponse
{
    public List<DnseCorporateActionItem> CorporateActions { get; init; } = new();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int Total { get; init; }
}

/// <summary>
/// Corporate action item from DNSE
/// </summary>
public record DnseCorporateActionItem
{
    public string? Symbol { get; init; }
    public string? Name { get; init; }
    public int EventId { get; init; }
    public string? ExRightsDate { get; init; }
    public string? RecordDate { get; init; }
    public string? Title { get; init; }
    public string? TitleEvent { get; init; }
    public string? Content { get; init; }
    public string? Note { get; init; }
    public string? Url { get; init; }
    public string? ActionDate { get; init; }
    public int EventType { get; init; }
}
