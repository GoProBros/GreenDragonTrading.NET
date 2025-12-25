namespace GreenDragonTrading.Application.DTOs
{
    public class WorkspaceDto
    {
        public int Id { get; set; }
        public string WorkspaceName { get; set; } = string.Empty;
        public string LayoutJson { get; set; } = "{}";
        public bool IsDefault { get; set; } = false;
        public string? ShareCode { get; set; }
    }
}
