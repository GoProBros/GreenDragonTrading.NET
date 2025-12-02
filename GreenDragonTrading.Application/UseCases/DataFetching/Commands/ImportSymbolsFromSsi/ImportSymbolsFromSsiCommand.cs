using MediatR;
using System.Text.Json.Serialization;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSymbolsFromSsi
{
    /// <summary>
    /// Command to import symbol data from SSI API.
    /// </summary>
    public record ImportSymbolsFromSsiCommand() : IRequest<ImportSymbolsFromSsiResult>;

    /// <summary>
    /// Result of the symbol import operation from SSI API.
    /// </summary>
    /// <param name="ImportedCount">Number of new symbols added to the database.</param>
    /// <param name="UpdatedCount">Number of existing symbols updated in the database.</param>
    /// <param name="Message">A descriptive message about the import operation result.</param>
    public record ImportSymbolsFromSsiResult(
        int ImportedCount,
        int UpdatedCount,
        [property: JsonIgnore] string Message
    );
}
