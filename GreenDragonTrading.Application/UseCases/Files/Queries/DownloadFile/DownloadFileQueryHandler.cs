using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Files.Queries.DownloadFile;

/// <summary>
/// Handler for DownloadFileQuery
/// </summary>
public class DownloadFileQueryHandler : IRequestHandler<DownloadFileQuery, ApiResponse<FileDownloadResult>>
{
    private readonly IUnitOfWork _uow;
    private readonly ILocalFileStorageService _fileStorageService;

    public DownloadFileQueryHandler(
        IUnitOfWork uow,
        ILocalFileStorageService fileStorageService)
    {
        _uow = uow;
        _fileStorageService = fileStorageService;
    }

    public async Task<ApiResponse<FileDownloadResult>> Handle(DownloadFileQuery request, CancellationToken cancellationToken)
    {
        string? filePath = null;
        string? fileName = null;
        string? mimeType = null;
        long fileSize = 0;

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
                    throw new BusinessRuleException("Báo cáo tài chính chưa có file đính kèm");
                }

                filePath = financialReport.FilePath;
                fileName = $"{financialReport.Ticker}_{financialReport.Year}_{financialReport.Period}.pdf";
                mimeType = "application/pdf";
                fileSize = financialReport.FileSize ?? 0;
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
                    throw new BusinessRuleException("Báo cáo phân tích chưa có file đính kèm");
                }

                filePath = analysisReport.FilePath;
                fileName = analysisReport.OriginalFileName;
                mimeType = analysisReport.MimeType ?? "application/pdf";
                fileSize = analysisReport.FileSize;
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
                    throw new BusinessRuleException("Người dùng chưa có avatar");
                }

                filePath = user.AvatarUrl;
                fileName = $"avatar_{user.Username}.jpg";
                mimeType = "image/jpeg";
                fileSize = 0; // Avatar doesn't store file size
                break;

            case FileCategory.CompanyLogo:
                var symbol = await _uow.Symbols.GetByIdAsync(request.EntityId, cancellationToken);
                if (symbol == null)
                {
                    throw new NotFoundException($"Mã chứng khoán {request.EntityId} không tồn tại");
                }

                if (string.IsNullOrEmpty(symbol.LogoPath))
                {
                    throw new BusinessRuleException("Công ty chưa có logo");
                }

                filePath = symbol.LogoPath;
                fileName = $"logo_{symbol.Ticker}.png";
                mimeType = "image/png";
                fileSize = 0; // Logo doesn't store file size
                break;

            default:
                throw new BusinessRuleException("Loại file không hợp lệ");
        }

        // Check if file exists
        if (!_fileStorageService.FileExists(filePath))
        {
            throw new NotFoundException("File không tồn tại trên hệ thống");
        }

        // Get file stream
        var fileStream = await _fileStorageService.GetFileStreamAsync(filePath, cancellationToken);

        var result = new FileDownloadResult
        {
            FileStream = fileStream,
            FileName = fileName,
            ContentType = mimeType,
            FileSize = fileSize
        };

        return ApiResponse<FileDownloadResult>.Success(result, "Tải file thành công");
    }
}
