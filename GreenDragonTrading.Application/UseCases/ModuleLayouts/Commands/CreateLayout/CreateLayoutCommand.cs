using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.ModuleLayouts.Commands.CreateLayout;

/// <summary>
/// Command để tạo mới module layout
/// </summary>
public record CreateLayoutCommand(
    string LayoutName,
    ModuleType ModuleType,
    JsonElement ConfigJson,
    bool IsSystemDefault = false
) : IRequest<ApiResponse<ModuleLayoutDto>>;
