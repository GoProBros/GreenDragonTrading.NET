using MediatR;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSectorsFromSsi
{
    public record ImportSectorsFromSsiCommand() : IRequest<ImportSectorsFromSsiResult>;

    public record ImportSectorsFromSsiResult(int ImportedCount, string Message);
    
}
