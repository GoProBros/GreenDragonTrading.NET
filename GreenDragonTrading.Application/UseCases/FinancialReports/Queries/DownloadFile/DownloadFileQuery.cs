using MediatR;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Queries.DownloadFile
{
    /// <summary>
    /// Query to download a financial report file from Google Drive
    /// </summary>
    public record DownloadFileQuery(
        Guid Id
    ) : IRequest<(Stream FileStream, string FileName, string ContentType)>;
}
