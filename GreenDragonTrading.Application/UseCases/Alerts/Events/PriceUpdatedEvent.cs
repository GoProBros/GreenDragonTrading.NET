using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Events
{
    public record PriceUpdatedEvent(
        string Ticker,
        decimal CurrentPrice,
        decimal? ReferencePrice = null,
        decimal? CurrentVolume = null,
        decimal? PreviousVolume = null) : INotification;
}
