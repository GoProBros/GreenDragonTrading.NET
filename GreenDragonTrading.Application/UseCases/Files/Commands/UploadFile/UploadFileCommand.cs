using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace GreenDragonTrading.Application.UseCases.Files.Commands.UploadFile;

/// <summary>
/// Upload file command
/// </summary>
public class UploadFileCommand : IRequest<ApiResponse<FileResponseDto>>
{
    /// <summary>
    /// File to upload
    /// </summary>
    public IFormFile File { get; set; } = null!;

    /// <summary>
    /// Upload metadata
    /// </summary>
    public FileUploadDto Metadata { get; set; } = null!;
}
