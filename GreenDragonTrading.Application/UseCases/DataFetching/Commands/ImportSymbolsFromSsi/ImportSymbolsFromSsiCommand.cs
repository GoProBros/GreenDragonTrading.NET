using MediatR;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSymbolsFromSsi
{
    public record ImportSymbolsFromSsiCommand() : IRequest<ImportSymbolsFromSsiResult>
    {
    }

    public record ImportSymbolsFromSsiResult(int ImportedCount, string Message);
}
