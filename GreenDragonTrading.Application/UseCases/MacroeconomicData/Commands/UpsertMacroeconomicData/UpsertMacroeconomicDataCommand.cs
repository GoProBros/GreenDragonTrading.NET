using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;
using System;

namespace GreenDragonTrading.Application.UseCases.MacroeconomicData.Commands.UpsertMacroeconomicData
{
    public class UpsertMacroeconomicDataCommand : IRequest<ApiResponse<MacroeconomicDataDto>>
    {
        public DateOnly RecordDate { get; set; }
        public decimal GovBondsReturn { get; set; }
        public decimal UsdVndExchangeRate { get; set; }
        public decimal UsdVndExchangeRateReturn { get; set; }
        public decimal EqualWeightIndexReturn { get; set; }
        public decimal MarketIndexValue { get; set; }
        public decimal MarketIndexReturn { get; set; }
        public decimal GoldSpotUsdReturn { get; set; }
    }
}