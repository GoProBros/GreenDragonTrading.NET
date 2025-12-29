using GreenDragonTrading.Domain.Enums;
using System.Text.Json;

namespace GreenDragonTrading.Application.DTOs;


public class ModuleLayoutDto
{
    public long Id { get; set; }
    public string LayoutName { get; set; } = string.Empty;
    public ModuleType ModuleType { get; set; }
    public string ModuleTypeName { get; set; } = string.Empty;
    public JsonElement? ConfigJson { get; set; }
    public bool IsSystemDefault { get; set; }
    public bool IsPersonal { get; set; }
    public Guid? UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary>
/// DTO cho danh sách layout (không có ConfigJson để giảm payload)
/// </summary>
public class ModuleLayoutListItemDto
{
    public long Id { get; set; }
    public string LayoutName { get; set; } = string.Empty;
    public ModuleType ModuleType { get; set; }
    public string ModuleTypeName { get; set; } = string.Empty;
    public bool IsSystemDefault { get; set; }
    public bool IsPersonal { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
