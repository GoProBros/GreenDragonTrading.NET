using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.MarketIndex.Queries.GetMarketIndexById
{
    /// <summary>
    /// Handles <see cref="GetMarketIndexByIdQuery"/>.
    /// </summary>
    public class GetMarketIndexByIdQueryHandler(
        IUnitOfWork uow,
        ILogger<GetMarketIndexByIdQueryHandler> logger)
        : IRequestHandler<GetMarketIndexByIdQuery, ApiResponse<MarketIndexDto>>
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly ILogger<GetMarketIndexByIdQueryHandler> _logger = logger;

        public async Task<ApiResponse<MarketIndexDto>> Handle(
            GetMarketIndexByIdQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting market index by code: {Code}", request.Code);

            var index = await _uow.MarketIndices.GetByCodeAsync(request.Code.ToUpper(), cancellationToken)
                ?? throw new NotFoundException($"Market index '{request.Code}' not found.");

            var dto = new MarketIndexDto
            {
                Code = index.Code,
                Name = index.Name,
                ExchangeCode = index.ExchangeCode,
                Description = index.Description,
                IsBenchmark = index.IsBenchmark,
                Status = index.Status
            };

            return ApiResponse<MarketIndexDto>.Success(dto, "Lấy thông tin chỉ số thị trường thành công");
        }
    }
}
