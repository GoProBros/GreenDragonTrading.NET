using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Files.Queries.DownloadFile;

/// <summary>
/// Query to download a file by category and entity ID
/// </summary>
public record DownloadFileQuery(
    FileCategory Category,
    string EntityId
) : IRequest<ApiResponse<FileDownloadResult>>;

/// <summary>
/// File download result containing stream and metadata
/// </summary>
public record FileDownloadResult
{
    public Stream FileStream { get; init; } = null!;
    public string FileName { get; init; } = null!;
    public string ContentType { get; init; } = null!;
    public long FileSize { get; init; }
}
