using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Heatmap.Queries;

/// <summary>
/// Handler xử lý query lấy dữ liệu heatmap
/// </summary>
public class GetHeatmapDataQueryHandler : IRequestHandler<GetHeatmapDataQuery, ApiResponse<HeatmapDataDto>>
{
    private readonly IHeatmapService _heatmapService;
    private readonly ILogger<GetHeatmapDataQueryHandler> _logger;

    public GetHeatmapDataQueryHandler(
        IHeatmapService heatmapService,
        ILogger<GetHeatmapDataQueryHandler> logger)
    {
        _heatmapService = heatmapService;
        _logger = logger;
    }

    public async Task<ApiResponse<HeatmapDataDto>> Handle(
        GetHeatmapDataQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Handling GetHeatmapDataQuery for exchange={Exchange}, sector={Sector}",
                request.Exchange, request.Sector);

            var data = await _heatmapService.GetHeatmapDataAsync(
                request.Exchange,
                request.Sector,
                cancellationToken);

            return ApiResponse<HeatmapDataDto>.Success(
                data,
                $"Lấy dữ liệu heatmap thành công với {data.TotalCount} mã");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetHeatmapDataQuery for exchange={Exchange}, sector={Sector}",
                request.Exchange, request.Sector);

            return ApiResponse<HeatmapDataDto>.Failure(
                "Có lỗi xảy ra khi lấy dữ liệu heatmap");
        }
    }
}
