using MediatR;
using System.Text.Json.Serialization;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSymbolsFromSsiV2
{
    /// <summary>
    /// Command to import symbol data from SSI API.
    /// </summary>
    public record ImportSymbolsFromSsiCommandV2() : IRequest<ImportSymbolsFromSsiV2Result>
    {
    }

    /// <summary>
    /// Result of the symbol import operation from SSI API.
    /// </summary>
    /// <param name="ImportedCount">Number of new symbols added to the database.</param>
    /// <param name="UpdatedCount">Number of existing symbols updated in the database.</param>
    /// <param name="Message">A descriptive message about the import operation result.</param>
    public record ImportSymbolsFromSsiV2Result(
        int ImportedCount,
        int UpdatedCount,
        [property: JsonIgnore]string Message);
}
