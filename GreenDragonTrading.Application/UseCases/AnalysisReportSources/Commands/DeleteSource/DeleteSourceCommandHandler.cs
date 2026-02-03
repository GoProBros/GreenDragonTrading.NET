using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportSources.Commands.DeleteSource;

/// <summary>
/// Handler for DeleteSourceCommand
/// </summary>
public class DeleteSourceCommandHandler : IRequestHandler<DeleteSourceCommand, ApiResponse>
{
    private readonly IUnitOfWork _uow;

    public DeleteSourceCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ApiResponse> Handle(DeleteSourceCommand request, CancellationToken cancellationToken)
    {
        var source = await _uow.AnalysisReportSources.GetByIdAsync(request.Id, cancellationToken);
        if (source == null)
        {
            throw new NotFoundException($"Nguồn với ID '{request.Id}' không tồn tại");
        }

        // Check if there are any reports using this source
        var hasReports = await _uow.AnalysisReports.AnyAsync(r => r.SourceId == request.Id, cancellationToken);
        if (hasReports)
        {
            throw new BusinessRuleException("Không thể xóa nguồn đang được sử dụng bởi các báo cáo");
        }

        _uow.AnalysisReportSources.Remove(source);
        await _uow.SaveChangesAsync(cancellationToken);

        return ApiResponse.Success("Xóa nguồn báo cáo thành công");
    }
}
