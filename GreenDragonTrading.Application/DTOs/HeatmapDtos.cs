namespace GreenDragonTrading.Application.DTOs;

/// <summary>
/// DTO cho một ô trong heatmap (một mã cổ phiếu)
/// </summary>
public class HeatmapItemDto
{
    /// <summary>
    /// Mã chứng khoán (VD: VNM, VIC, HPG)
    /// </summary>
    public string Ticker { get; set; } = string.Empty;

    /// <summary>
    /// Tên công ty
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Giá hiện tại
    /// </summary>
    public decimal CurrentPrice { get; set; }

    /// <summary>
    /// Phần trăm thay đổi so với giá tham chiếu
    /// </summary>
    public decimal ChangePercent { get; set; }

    /// <summary>
    /// Giá trị thay đổi (current - reference)
    /// </summary>
    public decimal ChangeValue { get; set; }

    /// <summary>
    /// Khối lượng giao dịch
    /// </summary>
    public long Volume { get; set; }

    /// <summary>
    /// Vốn hóa thị trường (optional)
    /// </summary>
    public decimal? MarketCap { get; set; }

    /// <summary>
    /// Sàn giao dịch
    /// </summary>
    public string Exchange { get; set; } = string.Empty;

    /// <summary>
    /// Ngành (ID)
    /// </summary>
    public string? Sector { get; set; }

    /// <summary>
    /// Tên ngành (tiếng Việt)
    /// </summary>
    public string? SectorName { get; set; }

    /// <summary>
    /// Loại màu cho heatmap (ceiling, strong-up, up, neutral, down, strong-down, floor)
    /// </summary>
    public string ColorType { get; set; } = "neutral";

    /// <summary>
    /// Thời gian cập nhật cuối
    /// </summary>
    public DateTime LastUpdate { get; set; }
}

/// <summary>
/// DTO cho toàn bộ dữ liệu heatmap
/// </summary>
public class HeatmapDataDto
{
    /// <summary>
    /// Sàn giao dịch (HSX, HNX, UPCOM)
    /// </summary>
    public string? Exchange { get; set; }

    /// <summary>
    /// Ngành (optional grouping)
    /// </summary>
    public string? Sector { get; set; }

    /// <summary>
    /// Danh sách các items trong heatmap
    /// </summary>
    public List<HeatmapItemDto> Items { get; set; } = new();

    /// <summary>
    /// Thời điểm tạo snapshot
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Tổng số items
    /// </summary>
    public int TotalCount => Items.Count;
}
