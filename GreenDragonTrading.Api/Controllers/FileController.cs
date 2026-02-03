using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.Files.Commands.DeleteFile;
using GreenDragonTrading.Application.UseCases.Files.Commands.UploadFile;
using GreenDragonTrading.Application.UseCases.Files.Queries.DownloadFile;
using GreenDragonTrading.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers;

/// <summary>
/// File management controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly IMediator _mediator;

    public FileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Upload a file (routes to appropriate entity based on category)
    /// </summary>
    /// <param name="request">File upload request containing file and metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Uploaded file information</returns>
    /// <remarks>
    /// Categories:
    /// - 1 = FinancialReport (requires relatedEntityId=FinancialReport GUID)
    /// - 2 = AnalysisReport (requires relatedEntityId=AnalysisReport GUID)
    /// - 3 = Avatar (no additional fields required)
    /// - 4 = CompanyLogo (requires relatedEntityId=ticker)
    /// </remarks>
    [HttpPost("upload")]
    [Authorize]
    public async Task<IActionResult> UploadFile(
        [FromForm] FileUploadRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var command = new UploadFileCommand
        {
            File = request.File,
            Metadata = new FileUploadDto
            {
                Category = request.Category,
                Description = request.Description,
                RelatedEntityId = request.RelatedEntityId
            }
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Download a file by category and entity ID
    /// </summary>
    /// <param name="category">File category (1=FinancialReport, 2=AnalysisReport, 3=Avatar, 4=CompanyLogo)</param>
    /// <param name="entityId">Entity ID (Financial/Analysis Report GUID, User GUID, or Symbol ticker)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File stream</returns>
    [HttpGet("download")]
    public async Task<IActionResult> DownloadFile(
        [FromQuery] FileCategory category,
        [FromQuery] string entityId,
        CancellationToken cancellationToken = default)
    {
        var query = new DownloadFileQuery(category, entityId);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess || result.Data == null)
        {
            return NotFound(result);
        }

        return File(
            result.Data.FileStream,
            result.Data.ContentType,
            result.Data.FileName);
    }

    /// <summary>
    /// Delete a file by category and entity ID
    /// </summary>
    /// <param name="category">File category (1=FinancialReport, 2=AnalysisReport, 3=Avatar, 4=CompanyLogo)</param>
    /// <param name="entityId">Entity ID (Financial/Analysis Report GUID, User GUID, or Symbol ticker)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    /// <remarks>
    /// Note: Deleting an AnalysisReport file will delete the entire report entity.
    /// For other categories, only the file is deleted and the entity is updated.
    /// </remarks>
    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteFile(
        [FromQuery] FileCategory category,
        [FromQuery] string entityId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteFileCommand(category, entityId);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
