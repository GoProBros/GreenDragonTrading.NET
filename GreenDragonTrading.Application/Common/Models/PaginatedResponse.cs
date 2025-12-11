namespace GreenDragonTrading.Application.Common.Models
{
    public record PaginatedResponse<T>
    {
        public IReadOnlyCollection<T> Items { get; init; }
        public int PageIndex { get; init; }
        public int TotalPages { get; init; }
        public int TotalCount { get; init; }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        private PaginatedResponse(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            TotalCount = count;
            Items = items;
        }

        public static PaginatedResponse<T> Create(List<T> items, int count, int pageIndex, int pageSize)
        {
            return new PaginatedResponse<T>(items, count, pageIndex, pageSize);
        }
    }
}
