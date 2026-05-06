using System.Text.Json;
using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Application.DTOs
{
    public class WorkspaceDto
    {
        public int Id { get; set; }
        public string WorkspaceName { get; set; } = string.Empty;
        public JsonElement? LayoutJson { get; set; }
        public WorkspaceType Type { get; set; }
        public bool IsDefault { get; set; } = false;
        public bool IsLocked { get; set; } = false;
        public string? ShareCode { get; set; }
    }

    public class MyWorkspacesDto
    {
        public List<WorkspaceDto> WebWorkspaces { get; set; } = [];
        public List<WorkspaceDto> MobileWorkspaces { get; set; } = [];
    }
}
