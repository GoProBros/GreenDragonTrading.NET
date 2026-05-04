using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Application.DTOs
{
    public class SectorDtos
    {
        public class SectorDto {
            public string Id { get; set; } = null!;
            public string? EnName { get; set; }
            public string? ViName { get; set; }
            public int? Level { get; set; }
            public CommonStatus Status { get; set; }
            public List<string>? Symbols { get; set; }
        }
    }
}
