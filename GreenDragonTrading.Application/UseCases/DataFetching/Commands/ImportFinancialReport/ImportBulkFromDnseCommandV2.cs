using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportFromDnse;

/// <summary>
/// Command to import financial reports from DNSE API in bulk without overwriting existing data.
/// </summary>
public record ImportBulkFromDnseCommandV2(
    string CycleType,
    int CycleNumber
) : IRequest<ApiResponse<DnseImportResult>>;
