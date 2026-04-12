using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Application.DTOs;

public class PortfolioDto
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public CommonStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
