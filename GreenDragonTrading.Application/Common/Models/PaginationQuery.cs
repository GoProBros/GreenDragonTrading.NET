namespace GreenDragonTrading.Application.Common.Models
{
    public abstract record PaginationQuery
    {
        public int PageIndex { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }
}
