using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportSpecificPeriodFromDnse;

/// <summary>
/// Command to import financial report for a specific period from DNSE
/// </summary>
public record ImportSpecificPeriodFromDnseCommand(
    string Ticker,
    int Year,
    ReportPeriod? Quarter
) : IRequest<ApiResponse<FinancialReportData>>;
