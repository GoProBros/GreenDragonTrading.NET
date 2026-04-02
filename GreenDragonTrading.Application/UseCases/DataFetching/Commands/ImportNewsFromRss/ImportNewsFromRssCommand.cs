using MediatR;
using System.Text.Json.Serialization;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportNewsFromRss;

public record ImportNewsFromRssCommand() : IRequest<ImportNewsFromRssResult>;

public record ImportNewsFromRssResult(
    int FetchedCount,
    int InsertedCount,
    int DuplicatedCount,
    [property: JsonIgnore] string Message
);
