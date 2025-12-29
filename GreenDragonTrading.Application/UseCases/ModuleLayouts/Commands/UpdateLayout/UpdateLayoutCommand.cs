using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.ModuleLayouts.Commands.UpdateLayout;

/// <summary>
/// Command để cập nhật module layout
/// </summary>
public record UpdateLayoutCommand(
    long Id,
    string? LayoutName = null,
    JsonElement? ConfigJson = null,
    bool? IsSystemDefault = null
) : IRequest<ApiResponse<ModuleLayoutDto>>;
