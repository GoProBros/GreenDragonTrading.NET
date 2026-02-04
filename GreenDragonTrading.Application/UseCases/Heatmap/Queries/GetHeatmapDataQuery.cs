using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Heatmap.Queries;

/// <summary>
/// Query để lấy dữ liệu heatmap
/// </summary>
public class GetHeatmapDataQuery : IRequest<ApiResponse<HeatmapDataDto>>
{
    /// <summary>
    /// Mã sàn giao dịch (HSX, HNX, UPCOM) - null để lấy tất cả
    /// </summary>
    public string? Exchange { get; set; }

    /// <summary>
    /// Mã ngành - null để lấy tất cả
    /// </summary>
    public string? Sector { get; set; }
}
