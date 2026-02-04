using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Symbols.Queries.GetAllTickers;

public class GetAllTickersQueryHandler 
    : IRequestHandler<GetAllTickersQuery, ApiResponse<List<string>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAllTickersQueryHandler> _logger;

    public GetAllTickersQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAllTickersQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResponse<List<string>>> Handle(
        GetAllTickersQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting all tickers");

            var tickers = await _unitOfWork.Symbols.GetAllTickersAsync(cancellationToken);
            var tickerList = tickers.ToList();

            _logger.LogInformation("Retrieved {Count} tickers", tickerList.Count);

            return ApiResponse<List<string>>.Success(
                tickerList,
                $"Lấy thành công {tickerList.Count} mã chứng khoán"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all tickers");
            return ApiResponse<List<string>>.Failure(
                $"Lỗi khi lấy danh sách mã chứng khoán: {ex.Message}"
            );
        }
    }
}
