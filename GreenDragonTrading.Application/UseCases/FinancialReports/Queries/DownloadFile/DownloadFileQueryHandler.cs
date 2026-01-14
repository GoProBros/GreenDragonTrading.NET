using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Queries.DownloadFile
{
    public class DownloadFileQueryHandler : IRequestHandler<DownloadFileQuery, (Stream FileStream, string FileName, string ContentType)>
    {
        private readonly IUnitOfWork _uow;
        private readonly IGoogleDriveService _googleDriveService;
        private readonly ILogger<DownloadFileQueryHandler> _logger;

        public DownloadFileQueryHandler(
            IUnitOfWork uow,
            IGoogleDriveService googleDriveService,
            ILogger<DownloadFileQueryHandler> logger)
        {
            _uow = uow;
            _googleDriveService = googleDriveService;
            _logger = logger;
        }

        public async Task<(Stream FileStream, string FileName, string ContentType)> Handle(
            DownloadFileQuery request, 
            CancellationToken cancellationToken)
        {
            var financialReport = await _uow.FinancialReports.GetByIdAsync(request.Id, cancellationToken);

            if (financialReport == null)
            {
                throw new FileNotFoundException($"Không tìm thấy báo cáo tài chính với ID {request.Id}");
            }

            if (string.IsNullOrEmpty(financialReport.FilePath))
            {
                throw new InvalidOperationException("Báo cáo tài chính này chưa có file đính kèm");
            }

            try
            {
                // Get file metadata from Google Drive
                var (fileName, fileSize, mimeType) = await _googleDriveService.GetFileMetadataAsync(
                    financialReport.FilePath, 
                    cancellationToken);

                // Download file stream from Google Drive
                var fileStream = await _googleDriveService.DownloadFileAsync(
                    financialReport.FilePath, 
                    cancellationToken);

                _logger.LogInformation("Downloaded file {FileName} for financial report {Id}", 
                    fileName, request.Id);

                return (fileStream, fileName, mimeType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file for financial report {Id} from Google Drive", request.Id);
                throw new Exception($"Lỗi khi tải file từ Google Drive: {ex.Message}", ex);
            }
        }
    }
}
