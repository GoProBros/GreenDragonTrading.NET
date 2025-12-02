using MediatR;
using System.Text.Json.Serialization;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSectorsFromSsi
{
    /// <summary>
    /// Command to import sector/industry data from SSI API into the database.
    /// Processes all 4 levels of industry sectors.
    /// </summary>
    public record ImportSectorsFromSsiCommand() : IRequest<ImportSectorsFromSsiResult>;

    /// <summary>
    /// Result of the sector import operation from SSI API.
    /// </summary>
    /// <param name="ImportedCount">Number of new sectors added to the database.</param>
    /// <param name="UpdatedCount">Number of existing sectors updated in the database.</param>
    /// <param name="Message">A descriptive message about the import operation result.</param>
    public record ImportSectorsFromSsiResult(
        int ImportedCount,
        int UpdatedCount,
        [property: JsonIgnore] string Message
    );
}
