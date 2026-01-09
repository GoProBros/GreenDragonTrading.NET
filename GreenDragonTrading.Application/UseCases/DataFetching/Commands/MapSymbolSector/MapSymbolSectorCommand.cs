using MediatR;
using System.Text.Json.Serialization;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.MapSymbolSector
{
    public record MapSymbolSectorCommand() : IRequest<MapSymbolSectorCommandResult>
    {
    }

    public record MapSymbolSectorCommandResult(
        int UpdatedCount,
        [property: JsonIgnore] string Message);
}
