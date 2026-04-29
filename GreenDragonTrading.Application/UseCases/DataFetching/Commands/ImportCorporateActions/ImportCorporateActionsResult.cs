using System.Text.Json.Serialization;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportCorporateActions;

public record ImportCorporateActionsResult(
    int TotalSymbols,
    int TotalPages,
    int FetchedCount,
    int InsertedCount,
    int UpdatedCount,
    int SkippedCount,
    [property: JsonIgnore] string Message
);
