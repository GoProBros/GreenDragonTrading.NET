using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.MacroeconomicData.Queries.GetMacroeconomicData
{
    public class GetMacroeconomicDataQuery : IRequest<ApiResponse<MacroeconomicDataDto?>>
    {
    }
}