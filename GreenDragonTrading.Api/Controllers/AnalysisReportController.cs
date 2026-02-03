using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.AnalysisReportCategories.Commands.CreateCategory;
using GreenDragonTrading.Application.UseCases.AnalysisReportCategories.Queries.GetCategories;
using GreenDragonTrading.Application.UseCases.AnalysisReportSources.Commands.CreateSource;
using GreenDragonTrading.Application.UseCases.AnalysisReportSources.Commands.DeleteSource;
using GreenDragonTrading.Application.UseCases.AnalysisReportSources.Commands.UpdateSource;
using GreenDragonTrading.Application.UseCases.AnalysisReportSources.Queries.GetSourceById;
using GreenDragonTrading.Application.UseCases.AnalysisReportSources.Queries.GetSources;
using GreenDragonTrading.Application.UseCases.AnalysisReports.Commands.CreateReport;
using GreenDragonTrading.Application.UseCases.AnalysisReports.Queries.GetReports;
using GreenDragonTrading.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers;

/// <summary>
/// Analysis Report management controller - handles reports, sources, and categories
/// </summary>
[ApiController]
[Route("api/v1/analysis-reports")]
public class AnalysisReportController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnalysisReportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Reports

    /// <summary>
    /// Get analysis reports with filters and pagination
    /// </summary>
    /// <param name="sourceId">Filter by source ID</param>
    /// <param name="categoryId">Filter by category ID</param>
    /// <param name="ticker">Filter by ticker (reports tagged with this symbol)</param>
    /// <param name="sectorId">Filter by sector ID</param>
    /// <param name="searchTerm">Search in title, description, author</param>
    /// <param name="pageIndex">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    [HttpGet]
    public async Task<IActionResult> GetReports(
        [FromQuery] string? sourceId,
        [FromQuery] string? categoryId,
        [FromQuery] string? ticker,
        [FromQuery] string? sectorId,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetReportsQuery
        {
            SourceId = sourceId,
            CategoryId = categoryId,
            Ticker = ticker,
            SectorId = sectorId,
            SearchTerm = searchTerm,
            PageIndex = pageIndex,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new analysis report (without file)
    /// </summary>
    /// <remarks>
    /// After creating the report, use the returned ID to upload the file via /api/file/upload endpoint with category=2
    /// </remarks>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateReport(
        [FromBody] CreateAnalysisReportDto request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateReportCommand
        {
            SourceId = request.SourceId,
            CategoryId = request.CategoryId,
            Title = request.Title,
            Description = request.Description,
            Tickers = request.Tickers,
            SectorId = request.SectorId,
            PublishDate = request.PublishDate
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    #endregion

    #region Sources

    /// <summary>
    /// Get all analysis report sources
    /// </summary>
    [HttpGet("sources")]
    public async Task<IActionResult> GetSources(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] CommonStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSourcesQuery
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            Status = status
        };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get analysis report source by ID
    /// </summary>
    [HttpGet("sources/{id}")]
    public async Task<IActionResult> GetSourceById(string id, CancellationToken cancellationToken = default)
    {
        var query = new GetSourceByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new analysis report source
    /// </summary>
    [HttpPost("sources")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateSource(
        [FromBody] CreateAnalysisReportSourceDto request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateSourceCommand
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Website = request.Website,
            LogoUrl = request.LogoUrl
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Update an existing analysis report source
    /// </summary>
    [HttpPut("sources/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateSource(
        string id,
        [FromBody] UpdateAnalysisReportSourceDto request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateSourceCommand
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
            Website = request.Website,
            LogoUrl = request.LogoUrl,
            Status = request.Status
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Delete an analysis report source
    /// </summary>
    [HttpDelete("sources/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSource(string id, CancellationToken cancellationToken = default)
    {
        var command = new DeleteSourceCommand { Id = id };
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    #endregion

    #region Categories

    /// <summary>
    /// Get all analysis report categories (hierarchical)
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] CommonStatus? status = null,
        [FromQuery] int? level = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCategoriesQuery
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            Status = status,
            Level = level
        };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new analysis report category
    /// </summary>
    [HttpPost("categories")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateAnalysisReportCategoryDto request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateCategoryCommand
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Level = request.Level,
            ParentId = request.ParentId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    #endregion
}
