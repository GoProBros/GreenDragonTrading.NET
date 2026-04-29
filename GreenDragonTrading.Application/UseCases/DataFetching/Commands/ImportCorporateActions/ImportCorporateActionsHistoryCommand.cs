using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportCorporateActions;

public record ImportCorporateActionsHistoryCommand() : IRequest<ApiResponse<ImportCorporateActionsResult>>;
