using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.ModuleLayouts.Commands.CreateLayout;
using GreenDragonTrading.Application.UseCases.ModuleLayouts.Commands.UpdateLayout;
using GreenDragonTrading.Application.UseCases.ModuleLayouts.Commands.DeleteLayout;
using GreenDragonTrading.Application.UseCases.ModuleLayouts.Queries.GetMyLayouts;
using GreenDragonTrading.Application.UseCases.ModuleLayouts.Queries.GetLayoutById;
using GreenDragonTrading.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers;

/// <summary>
/// Controller quản lý module layouts
/// </summary>
[ApiController]
[Route("api/v1/module-layouts")]
[Authorize]
public class ModuleLayoutController : ControllerBase
{
    private readonly IMediator _mediator;

    public ModuleLayoutController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy danh sách layout khả dụng cho một loại module (của user hiện tại)
    /// </summary>
    /// <param name="type">Loại module (1: stock screener, 2: stock chart, ... phần sau chưa liệt kê nên chưa có số đâu ) </param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Danh sách layout (system + personal)</returns>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<ModuleLayoutListItemDto>>>> GetMyLayouts(
        [FromQuery] ModuleType type,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyLayoutsQuery(type), cancellationToken);
        return result;
    }

    /// <summary>
    /// Lấy cấu hình JSON chi tiết của một layout cụ thể
    /// </summary>
    /// <param name="id">ID của layout</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Chi tiết layout bao gồm cấu hình JSON</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ModuleLayoutDto>>> GetLayoutById(
        long id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLayoutByIdQuery(id), cancellationToken);
        return result;
    }

    /// <summary>
    /// Lưu cấu hình hiện tại của một module thành một bản layout mới (Save As)
    /// </summary>
    /// <param name="command">Thông tin layout mới</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Layout vừa tạo</returns>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ModuleLayoutDto>>> CreateLayout(
        [FromBody] CreateLayoutCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result;
    }

    /// <summary>
    /// Cập nhật (Save) đè lên bản layout hiện có
    /// </summary>
    /// <param name="id">ID của layout cần cập nhật</param>
    /// <param name="command">Thông tin cập nhật</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Layout sau khi cập nhật</returns>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ModuleLayoutDto>>> UpdateLayout(
        [FromRoute] long id,
        [FromBody] UpdateLayoutCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Id != id)
        {
            return BadRequest(ApiResponse<ModuleLayoutDto>.Failure("ID không khớp."));
        }

        var result = await _mediator.Send(command, cancellationToken);
        return result;
    }

    /// <summary>
    /// Xóa bản layout cá nhân
    /// </summary>
    /// <param name="id">ID của layout cần xóa</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Kết quả xóa</returns>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> DeleteLayout(
        long id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteLayoutCommand(id), cancellationToken);
        return result;
    }
}
