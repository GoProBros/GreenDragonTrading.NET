using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.FinancialReports.Commands.UploadFinancialReport;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers
{
    /// <summary>
    /// Financial Report management endpoints
    /// </summary>
    [ApiController]
    [Route("api/v1/financial-reports")]
    public class FinancialReportController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Upload financial report
        /// </summary>
        /// <param name="request">Upload request containing ticker, year, period, and file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Uploaded report information</returns>
        [HttpPost("upload")]
        //[Authorize]
        public async Task<ActionResult<ApiResponse<UploadFinancialReportResponse>>> UploadFinancialReport(
            [FromForm] UploadFinancialReportCommand request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request, cancellationToken);
            return Ok(result);
        }
    }
}
