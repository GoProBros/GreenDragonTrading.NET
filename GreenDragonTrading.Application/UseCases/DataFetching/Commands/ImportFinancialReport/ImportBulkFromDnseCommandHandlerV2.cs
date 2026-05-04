using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.DataFetching.Commands.ImportFromDnse;

public class ImportBulkFromDnseCommandHandlerV2(
    IUnitOfWork uow,
    IDnseService dnseService,
    IDnseDataMapper mapper,
    IFinancialReportIndicatorCalculationService indicatorCalculationService,
    ILogger<ImportBulkFromDnseCommandHandlerV2> logger)
    : IRequestHandler<ImportBulkFromDnseCommandV2, ApiResponse<DnseImportResult>>
{
    private readonly IUnitOfWork _uow = uow;
    private readonly IDnseService _dnseService = dnseService;
    private readonly IDnseDataMapper _mapper = mapper;
    private readonly IFinancialReportIndicatorCalculationService _indicatorCalculationService = indicatorCalculationService;
    private readonly ILogger<ImportBulkFromDnseCommandHandlerV2> _logger = logger;

    public Task<ApiResponse<DnseImportResult>> Handle(ImportBulkFromDnseCommandV2 request, CancellationToken cancellationToken)
    {
        return DnseBulkImportProcessor.RunAsync(
            request.CycleType,
            request.CycleNumber,
            false,
            _uow,
            _dnseService,
            _mapper,
            _indicatorCalculationService,
            _logger,
            cancellationToken);
    }
}
