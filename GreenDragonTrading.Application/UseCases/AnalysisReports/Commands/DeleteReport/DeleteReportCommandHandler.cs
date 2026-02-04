using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReports.Commands.DeleteReport;

/// <summary>
/// Handler for DeleteReportCommand
/// </summary>
public class DeleteReportCommandHandler : IRequestHandler<DeleteReportCommand, ApiResponse>
{
    private readonly IUnitOfWork _uow;

    public DeleteReportCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ApiResponse> Handle(DeleteReportCommand request, CancellationToken cancellationToken)
    {
        var report = await _uow.AnalysisReports.GetByIdAsync(request.Id, cancellationToken);
        if (report == null)
        {
            throw new NotFoundException($"Báo cáo với ID '{request.Id}' không tồn tại");
        }

        // Soft delete
        report.Status = CommonStatus.InActive;
        report.UpdatedAt = DateTimeOffset.UtcNow;

        _uow.AnalysisReports.Update(report);
        await _uow.SaveChangesAsync(cancellationToken);

        return ApiResponse.Success("Xóa báo cáo thành công");
    }
}
