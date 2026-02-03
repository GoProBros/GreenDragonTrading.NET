using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Files.Commands.DeleteFile;

/// <summary>
/// Handler for DeleteFileCommand
/// </summary>
public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, ApiResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ILocalFileStorageService _fileStorageService;

    public DeleteFileCommandHandler(
        IUnitOfWork uow,
        ILocalFileStorageService fileStorageService)
    {
        _uow = uow;
        _fileStorageService = fileStorageService;
    }

    public async Task<ApiResponse> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        string? filePath = null;

        switch (request.Category)
        {
            case FileCategory.FinancialReport:
                if (!Guid.TryParse(request.EntityId, out var financialReportId))
                {
                    throw new BusinessRuleException("ID báo cáo tài chính không hợp lệ");
                }

                var financialReport = await _uow.FinancialReports.GetByIdAsync(financialReportId, cancellationToken);
                if (financialReport == null)
                {
                    throw new NotFoundException("Báo cáo tài chính không tồn tại");
                }

                if (string.IsNullOrEmpty(financialReport.FilePath))
                {
                    throw new BusinessRuleException("Báo cáo tài chính không có file để xóa");
                }

                filePath = financialReport.FilePath;

                // Delete file from storage
                await _fileStorageService.DeleteFileAsync(filePath, cancellationToken);

                // Update entity
                financialReport.FilePath = null;
                financialReport.FileSize = null;
                financialReport.UpdatedAt = DateTimeOffset.UtcNow;

                _uow.FinancialReports.Update(financialReport);
                await _uow.SaveChangesAsync(cancellationToken);
                break;

            case FileCategory.AnalysisReport:
                if (!Guid.TryParse(request.EntityId, out var analysisReportId))
                {
                    throw new BusinessRuleException("ID báo cáo phân tích không hợp lệ");
                }

                var analysisReport = await _uow.AnalysisReports.GetByIdAsync(analysisReportId, cancellationToken);
                if (analysisReport == null)
                {
                    throw new NotFoundException("Báo cáo phân tích không tồn tại");
                }

                if (string.IsNullOrEmpty(analysisReport.FilePath))
                {
                    throw new BusinessRuleException("Báo cáo phân tích không có file để xóa");
                }

                filePath = analysisReport.FilePath;

                // Delete file from storage
                await _fileStorageService.DeleteFileAsync(filePath, cancellationToken);

                // Delete entire analysis report entity (since file is required)
                _uow.AnalysisReports.Delete(analysisReport);
                await _uow.SaveChangesAsync(cancellationToken);
                break;

            case FileCategory.Avatar:
                if (!Guid.TryParse(request.EntityId, out var userId))
                {
                    throw new BusinessRuleException("ID người dùng không hợp lệ");
                }

                var user = await _uow.Users.GetByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    throw new NotFoundException("Người dùng không tồn tại");
                }

                if (string.IsNullOrEmpty(user.AvatarUrl))
                {
                    throw new BusinessRuleException("Người dùng không có avatar để xóa");
                }

                filePath = user.AvatarUrl;

                // Delete file from storage
                await _fileStorageService.DeleteFileAsync(filePath, cancellationToken);

                // Update entity
                user.AvatarUrl = null;
                _uow.Users.Update(user);
                await _uow.SaveChangesAsync(cancellationToken);
                break;

            case FileCategory.CompanyLogo:
                var symbol = await _uow.Symbols.GetByIdAsync(request.EntityId, cancellationToken);
                if (symbol == null)
                {
                    throw new NotFoundException($"Mã chứng khoán {request.EntityId} không tồn tại");
                }

                if (string.IsNullOrEmpty(symbol.LogoPath))
                {
                    throw new BusinessRuleException("Công ty không có logo để xóa");
                }

                filePath = symbol.LogoPath;

                // Delete file from storage
                await _fileStorageService.DeleteFileAsync(filePath, cancellationToken);

                // Update entity
                symbol.LogoPath = null;
                _uow.Symbols.Update(symbol);
                await _uow.SaveChangesAsync(cancellationToken);
                break;

            default:
                throw new BusinessRuleException("Loại file không hợp lệ");
        }

        return ApiResponse.Success("Xóa file thành công");
    }
}
