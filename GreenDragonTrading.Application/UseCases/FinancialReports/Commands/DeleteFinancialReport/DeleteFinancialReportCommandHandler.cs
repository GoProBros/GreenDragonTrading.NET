using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Commands.DeleteFinancialReport
{
    public class DeleteFinancialReportCommandHandler : IRequestHandler<DeleteFinancialReportCommand, ApiResponse>
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<DeleteFinancialReportCommandHandler> _logger;

        public DeleteFinancialReportCommandHandler(
            IUnitOfWork uow,
            ILogger<DeleteFinancialReportCommandHandler> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<ApiResponse> Handle(DeleteFinancialReportCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var financialReport = await _uow.FinancialReports.GetByIdAsync(request.Id, cancellationToken);
                
                if (financialReport == null)
                {
                    return ApiResponse.Failure($"Không tìm thấy báo cáo tài chính với ID {request.Id}.");
                }

                // Delete file if exists
                if (!string.IsNullOrEmpty(financialReport.FilePath))
                {
                    try
                    {
                        _logger.LogInformation("File deleted: {FilePath}", financialReport.FilePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete file: {FilePath}", financialReport.FilePath);
                    }
                }

                _uow.FinancialReports.Remove(financialReport);
                await _uow.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Financial report deleted: {Id}", request.Id);
                return ApiResponse.Success("Xóa báo cáo tài chính thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting financial report {Id}", request.Id);
                throw;
            }
        }
    }
}
