using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Commands.CreateAlert
{
    public class CreateAlertCommand : IRequest<ApiResponse<AlertDto>>
    {
        public string Ticker { get; set; } = string.Empty;
        public AlertType Type { get; set; }
        public ConditionType Condition { get; set; }
        public decimal? ChangePercentage { get; set; }
        public decimal? ThresholdValue { get; set; }
        public string? Name { get; set; }
        public bool IsActive { get; set; } = true;
        public int? ChatSessionId { get; set; }
        public string? MessageTemplate { get; set; }
    }
}
