using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Symbols.Queries.GetSymbols
{
    public class GetSymbolsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetSymbolsQuery, GetSymbolsQueryResult>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<GetSymbolsQueryResult> Handle(GetSymbolsQuery request, CancellationToken cancellationToken)
        {
            var symbolType = request.Type.HasValue ? (SymbolType?)request.Type.Value : null;

            var (symbols, totalCount) = await _unitOfWork.Symbols.GetSymbolsAsync(
                request.PageIndex,
                request.PageSize,
                symbolType,
                request.Exchange,
                request.Sector,
                cancellationToken);

            var symbolDtos = symbols.Select(s => new SymbolDtos
            {
                Ticker = s.Ticker,
                Isin = s.Isin,
                EnCompanyName = s.EnCompanyName,
                ViCompanyName = s.ViCompanyName,
                ExchangeCode = s.ExchangeCode,
                SectorId = s.SectorId,
                Type = s.Type,
                Status = s.Status
            }).ToList();

            return new GetSymbolsQueryResult(symbolDtos, totalCount, "Success");
        }
    }
}
