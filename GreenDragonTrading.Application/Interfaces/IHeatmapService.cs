using GreenDragonTrading.Application.DTOs;

namespace GreenDragonTrading.Application.Interfaces;

/// <summary>
/// Service xử lý logic tính toán và aggregate dữ liệu heatmap
/// </summary>
public interface IHeatmapService
{
    /// <summary>
    /// Lấy dữ liệu heatmap cho một exchange/sector
    /// </summary>
    /// <param name="exchange">Mã sàn (HSX, HNX, UPCOM) - null để lấy tất cả</param>
    /// <param name="sector">Mã ngành - null để lấy tất cả</param>
    /// <param name="tickers">Danh sách mã cổ phiếu cụ thể - khi có sẽ đọc từ Redis cache trực tiếp</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dữ liệu heatmap</returns>
    Task<HeatmapDataDto> GetHeatmapDataAsync(
        string? exchange = null,
        string? sector = null,
        string[]? tickers = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tính màu cho heatmap dựa trên % thay đổi
    /// </summary>
    /// <param name="changePercent">Phần trăm thay đổi</param>
    /// <returns>Color type (ceiling, strong-up, up, neutral, down, strong-down, floor)</returns>
    string GetHeatmapColor(decimal changePercent);
}
