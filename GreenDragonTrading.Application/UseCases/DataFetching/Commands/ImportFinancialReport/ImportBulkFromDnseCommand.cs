using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportFromDnse;

/// <summary>
/// Command to import financial reports from DNSE API in bulk
/// </summary>
public record ImportBulkFromDnseCommand(
    List<string>? Tickers,
    string CycleType,
    int CycleNumber
) : IRequest<ApiResponse<DnseImportResult>>;
