using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportCorporateActions;

public record ImportCorporateActionsUpcomingCommand() : IRequest<ApiResponse<ImportCorporateActionsResult>>;
