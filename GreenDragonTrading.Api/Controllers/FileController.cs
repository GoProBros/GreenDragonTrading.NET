using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.Files.Commands.UploadFile;
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
}
