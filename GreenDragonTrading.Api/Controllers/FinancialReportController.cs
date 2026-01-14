using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.FinancialReports.Commands.CreateFinancialReport;
using GreenDragonTrading.Application.UseCases.FinancialReports.Commands.DeleteFinancialReport;
using GreenDragonTrading.Application.UseCases.FinancialReports.Commands.UpdateFinancialReport;
using GreenDragonTrading.Application.UseCases.FinancialReports.Commands.UploadFile;
using GreenDragonTrading.Application.UseCases.FinancialReports.Queries.DownloadFile;
using GreenDragonTrading.Application.UseCases.FinancialReports.Queries.GetFinancialReportById;
using GreenDragonTrading.Application.UseCases.FinancialReports.Queries.GetFinancialReports;
using GreenDragonTrading.Application.UseCases.FinancialReports.Queries.GetFinancialReportsByTicker;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers
{
    /// <summary>
    /// Controller for managing financial report operations.
    /// </summary>
    [Route("api/v1/financial-reports")]
    [ApiController]
    public class FinancialReportController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Retrieves a paginated list of financial reports with optional filters.
        /// </summary>
        /// <param name="query">Query parameters for filtering and paginating financial reports.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>A paginated list of financial reports.</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<SimpleFinancialReportDto>>>> GetFinancialReports(
            [FromQuery] GetFinancialReportsQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(query, cancellationToken);
            
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific financial report by ID.
        /// </summary>
        /// <param name="id">The ID of the financial report to retrieve.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>The financial report details.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<FinancialReportDto>>> GetFinancialReportById(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetFinancialReportByIdQuery(id), cancellationToken);
            
            if (!result.IsSuccess)
            {
                return NotFound(result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all financial reports for a specific ticker symbol.
        /// </summary>
        /// <param name="ticker">The ticker symbol to retrieve reports for.</param>
        /// <param name="query">Query parameters for pagination.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>A paginated list of financial reports for the specified ticker.</returns>
        [HttpGet("ticker/{ticker}")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<SimpleFinancialReportDto>>>> GetFinancialReportsByTicker(
            [FromRoute] string ticker,
            [FromQuery] GetFinancialReportsByTickerQuery query,
            CancellationToken cancellationToken = default)
        {
            // Override ticker from route
            var queryWithTicker = query with { Ticker = ticker };
            var result = await _mediator.Send(queryWithTicker, cancellationToken);
            
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Creates a new financial report with optional file upload.
        /// </summary>
        /// <param name="request">The financial report creation request.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>The created financial report.</returns>
        [HttpPost]
        [Consumes("application/json")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<FinancialReportDto>>> CreateFinancialReport(
            [FromForm] CreateFinancialReportRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = new CreateFinancialReportCommand(
                request.Ticker,
                request.Year,
                request.Period,
                request.ShortTermAssets,
                request.CashAndCashEquivalents,
                request.ShortTermFinancialInvestments,
                request.ShortTermReceivables,
                request.Inventories,
                request.LongTermAssets,
                request.LongTermReceivables,
                request.FixedAssets,
                request.TotalAssets,
                request.Liabilities,
                request.ShortTermLiabilities,
                request.LongTermLiabilities,
                request.OwnerEquity,
                request.TotalResources,
                request.Revenue,
                request.NetRevenue,
                request.CostOfGoodsSold,
                request.GrossProfit,
                request.NetProfit,
                request.ProfitBeforeTax,
                request.IncomeTaxExpense,
                request.ProfitAfterTax,
                request.CashFromOperating,
                request.CashFromInvesting,
                request.CashFromFinancing,
                request.NetCashFlow,
                request.BeginningCash,
                request.EndingCash
            );

            var result = await _mediator.Send(command, cancellationToken);
            
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            
            return CreatedAtAction(
                nameof(GetFinancialReportById),
                new { id = result.Data!.Id },
                result);
        }

        /// <summary>
        /// Updates an existing financial report.
        /// </summary>
        /// <param name="id">The ID of the financial report to update.</param>
        /// <param name="request">The financial report update request.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>The updated financial report.</returns>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<FinancialReportDto>>> UpdateFinancialReport(
            [FromRoute] Guid id,
            [FromBody] UpdateFinancialReportRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = new UpdateFinancialReportCommand(
                id,
                request.ShortTermAssets,
                request.CashAndCashEquivalents,
                request.ShortTermFinancialInvestments,
                request.ShortTermReceivables,
                request.Inventories,
                request.LongTermAssets,
                request.LongTermReceivables,
                request.FixedAssets,
                request.TotalAssets,
                request.Liabilities,
                request.ShortTermLiabilities,
                request.LongTermLiabilities,
                request.OwnerEquity,
                request.TotalResources,
                request.Revenue,
                request.NetRevenue,
                request.CostOfGoodsSold,
                request.GrossProfit,
                request.NetProfit,
                request.ProfitBeforeTax,
                request.IncomeTaxExpense,
                request.ProfitAfterTax,
                request.CashFromOperating,
                request.CashFromInvesting,
                request.CashFromFinancing,
                request.NetCashFlow,
                request.BeginningCash,
                request.EndingCash,
                request.Status
            );

            var result = await _mediator.Send(command, cancellationToken);
            
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Deletes a financial report.
        /// </summary>
        /// <param name="id">The ID of the financial report to delete.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>Success or failure response.</returns>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> DeleteFinancialReport(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new DeleteFinancialReportCommand(id), cancellationToken);
            
            if (!result.IsSuccess)
            {
                return NotFound(result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Uploads or replaces a file for an existing financial report.
        /// </summary>
        /// <param name="id">The ID of the financial report to upload file for.</param>
        /// <param name="file">The file to upload.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>The updated financial report with new file information.</returns>
        [HttpPost("{id}/upload-file")]
        [Consumes("multipart/form-data")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<FinancialReportDto>>> UploadFile(
            [FromRoute] Guid id,
            IFormFile file,
            CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<FinancialReportDto>.Failure("File không được để trống."));
            }

            var command = new UploadFileCommand(id, file);
            var result = await _mediator.Send(command, cancellationToken);
            
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Downloads a file from Google Drive for a financial report.
        /// </summary>
        /// <param name="id">The ID of the financial report.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>File stream with the financial report file.</returns>
        [HttpGet("{id}/download-file")]
        public async Task<IActionResult> DownloadFile(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = new DownloadFileQuery(id);
                var (fileStream, fileName, contentType) = await _mediator.Send(query, cancellationToken);

                return File(fileStream, contentType, fileName);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(ApiResponse.Failure(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.Failure(ex.Message));
            }
        }
    }
}
