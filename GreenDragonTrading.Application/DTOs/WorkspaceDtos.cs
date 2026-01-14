using System.Text.Json;

namespace GreenDragonTrading.Application.DTOs
{
    public class WorkspaceDto
    {
        public int Id { get; set; }
        public string WorkspaceName { get; set; } = string.Empty;
        public JsonElement? LayoutJson { get; set; }
        public bool IsDefault { get; set; } = false;
        public string? ShareCode { get; set; }
    }
}
