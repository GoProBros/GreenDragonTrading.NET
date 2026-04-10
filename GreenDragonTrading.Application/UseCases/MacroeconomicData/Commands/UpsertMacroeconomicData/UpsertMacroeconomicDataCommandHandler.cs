using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GreenDragonTrading.Application.UseCases.MacroeconomicData.Commands.UpsertMacroeconomicData
{
    public class UpsertMacroeconomicDataCommandHandler : IRequestHandler<UpsertMacroeconomicDataCommand, ApiResponse<MacroeconomicDataDto>>
    {
        private readonly IUnitOfWork _uow;

        public UpsertMacroeconomicDataCommandHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<ApiResponse<MacroeconomicDataDto>> Handle(UpsertMacroeconomicDataCommand request, CancellationToken cancellationToken)
        {
            var entity = await _uow.MacroeconomicData.FirstOrDefaultAsync(x => true, cancellationToken);
            
            if (entity == null)
            {
                entity = new Domain.Entities.MacroeconomicData
                {
                    RecordDate = request.RecordDate,
                    GovBondsReturn = request.GovBondsReturn,
                    UsdVndExchangeRate = request.UsdVndExchangeRate,
                    UsdVndExchangeRateReturn = request.UsdVndExchangeRateReturn,
                    EqualWeightIndexReturn = request.EqualWeightIndexReturn,
                    MarketIndexValue = request.MarketIndexValue,
                    MarketIndexReturn = request.MarketIndexReturn,
                    GoldSpotUsdReturn = request.GoldSpotUsdReturn
                };
                await _uow.MacroeconomicData.AddAsync(entity, cancellationToken);
            }
            else
            {
                entity.RecordDate = request.RecordDate;
                entity.GovBondsReturn = request.GovBondsReturn;
                entity.UsdVndExchangeRate = request.UsdVndExchangeRate;
                entity.UsdVndExchangeRateReturn = request.UsdVndExchangeRateReturn;
                entity.EqualWeightIndexReturn = request.EqualWeightIndexReturn;
                entity.MarketIndexValue = request.MarketIndexValue;
                entity.MarketIndexReturn = request.MarketIndexReturn;
                entity.GoldSpotUsdReturn = request.GoldSpotUsdReturn;
                entity.UpdatedAt = DateTimeOffset.UtcNow;
                _uow.MacroeconomicData.Update(entity);
            }

            await _uow.SaveChangesAsync(cancellationToken);

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

            return ApiResponse<MacroeconomicDataDto>.Success(dto, "Lưu dữ liệu kinh tế vĩ mô thành công");
        }
    }
}