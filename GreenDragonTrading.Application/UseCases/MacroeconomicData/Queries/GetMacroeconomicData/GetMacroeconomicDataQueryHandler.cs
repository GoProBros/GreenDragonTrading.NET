using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GreenDragonTrading.Application.UseCases.MacroeconomicData.Queries.GetMacroeconomicData
{
    public class GetMacroeconomicDataQueryHandler : IRequestHandler<GetMacroeconomicDataQuery, ApiResponse<MacroeconomicDataDto?>>
    {
        private readonly IUnitOfWork _uow;

        public GetMacroeconomicDataQueryHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<ApiResponse<MacroeconomicDataDto?>> Handle(GetMacroeconomicDataQuery request, CancellationToken cancellationToken)
        {
            var entity = await _uow.MacroeconomicData.FirstOrDefaultAsync(x => true, cancellationToken);
            
            if (entity == null)
            {
                return ApiResponse<MacroeconomicDataDto?>.Success(null, "Chưa có dữ liệu vĩ mô.");
            }

            var dto = new MacroeconomicDataDto
            {
                RecordDate = entity.RecordDate,
                GovBondsReturn = entity.GovBondsReturn,
                UsdVndExchangeRate = entity.UsdVndExchangeRate,
                UsdVndExchangeRateReturn = entity.UsdVndExchangeRateReturn,
                EqualWeightIndexReturn = entity.EqualWeightIndexReturn,
                MarketIndexValue = entity.MarketIndexValue,
                MarketIndexReturn = entity.MarketIndexReturn,
                GoldSpotUsdReturn = entity.GoldSpotUsdReturn,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };

            return ApiResponse<MacroeconomicDataDto?>.Success(dto, "Lấy dữ liệu kinh tế vĩ mô thành công");
        }
    }
}