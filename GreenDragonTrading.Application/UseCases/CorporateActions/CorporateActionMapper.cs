using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Entities;

namespace GreenDragonTrading.Application.UseCases.CorporateActions;

internal static class CorporateActionMapper
{
    public static CorporateActionDto ToDto(CorporateAction entity)
    {
        return new CorporateActionDto
        {
            EventId = entity.EventId,
            Ticker = entity.Ticker,
            Name = entity.Name,
            Title = entity.Title,
            TitleEvent = entity.TitleEvent,
            Content = entity.Content,
            Note = entity.Note,
            Url = entity.Url,
            ExRightsDate = entity.ExRightsDate,
            RecordDate = entity.RecordDate,
            ActionDate = entity.ActionDate,
            EventType = entity.EventType,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
