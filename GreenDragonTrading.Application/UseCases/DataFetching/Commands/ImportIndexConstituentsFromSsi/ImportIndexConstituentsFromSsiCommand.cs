using MediatR;
using System.Text.Json.Serialization;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportIndexConstituentsFromSsi
{
    /// <summary>
    /// Command to fetch constituent symbols for all market indices from SSI API
    /// and sync them into the <c>market_index_symbols</c> junction table.
    /// </summary>
    public record ImportIndexConstituentsFromSsiCommand() : IRequest<ImportIndexConstituentsFromSsiResult>;

    /// <summary>
    /// Result of the index constituents import operation.
    /// </summary>
    /// <param name="ProcessedIndices">Number of indices processed.</param>
    /// <param name="UpsertedCount">Total number of constituent records inserted or updated.</param>
    /// <param name="Message">Human-readable summary.</param>
    public record ImportIndexConstituentsFromSsiResult(
        int ProcessedIndices,
        int UpsertedCount,
        [property: JsonIgnore] string Message
    );
}
