using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Queries.QueryFinancialReportsByFields
{
    public class QueryFinancialReportsByFieldsQueryValidator : AbstractValidator<QueryFinancialReportsByFieldsQuery>
    {
        private static readonly HashSet<string> SupportedOperators =
        [
            "eq", "neq", "gt", "gte", "lt", "lte", "contains", "exists", "notexists", "isnull", "isnotnull"
        ];

        private static readonly HashSet<string> SupportedSortFields =
        [
            "ticker", "year", "period", "status", "createdAt", "updatedAt"
        ];

        public QueryFinancialReportsByFieldsQueryValidator()
        {
            RuleFor(x => x.PageIndex)
                .GreaterThan(0).WithMessage("PageIndex phải lớn hơn 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("PageSize phải lớn hơn 0.")
                .LessThanOrEqualTo(200).WithMessage("PageSize không được vượt quá 200.");

            RuleFor(x => x.MaxScanRecords)
                .GreaterThan(0).WithMessage("MaxScanRecords phải lớn hơn 0.")
                .LessThanOrEqualTo(50000).WithMessage("MaxScanRecords không được vượt quá 50000.");

            When(x => !string.IsNullOrWhiteSpace(x.Ticker), () =>
            {
                RuleFor(x => x.Ticker!)
                    .MaximumLength(20).WithMessage("Ticker không được vượt quá 20 ký tự.");
            });

            When(x => x.YearFrom.HasValue, () =>
            {
                RuleFor(x => x.YearFrom!.Value)
                    .GreaterThan(2000).WithMessage("YearFrom phải lớn hơn 2000.");
            });

            When(x => x.YearTo.HasValue, () =>
            {
                RuleFor(x => x.YearTo!.Value)
                    .GreaterThan(2000).WithMessage("YearTo phải lớn hơn 2000.");
            });

            RuleFor(x => x)
                .Must(x => !x.YearFrom.HasValue || !x.YearTo.HasValue || x.YearFrom <= x.YearTo)
                .WithMessage("YearFrom phải nhỏ hơn hoặc bằng YearTo.");

            RuleFor(x => x.SortBy)
                .NotEmpty().WithMessage("SortBy là bắt buộc.")
                .Must(IsSupportedSortBy)
                .WithMessage("SortBy không hợp lệ. Chỉ hỗ trợ: ticker, year, period, status, createdAt, updatedAt.");

            RuleFor(x => x.SortDirection)
                .Must(direction => direction.Equals("asc", StringComparison.OrdinalIgnoreCase)
                    || direction.Equals("desc", StringComparison.OrdinalIgnoreCase))
                .WithMessage("SortDirection chỉ chấp nhận asc hoặc desc.");

            RuleForEach(x => x.Filters).ChildRules(filter =>
            {
                filter.RuleFor(f => f.Path)
                    .NotEmpty().WithMessage("Filter.Path là bắt buộc.");

                filter.RuleFor(f => f.Operator)
                    .NotEmpty().WithMessage("Filter.Operator là bắt buộc.")
                    .Must(@operator => SupportedOperators.Contains(@operator.Trim().ToLowerInvariant()))
                    .WithMessage("Filter.Operator không hợp lệ.");

                filter.RuleFor(f => f.Value)
                    .NotEmpty().WithMessage("Filter.Value là bắt buộc với operator hiện tại.")
                    .When(f =>
                    {
                        var op = f.Operator.Trim().ToLowerInvariant();
                        return op != "exists" && op != "notexists" && op != "isnull" && op != "isnotnull";
                    });
            });
        }

        private static bool IsSupportedSortBy(string sortBy)
        {
            var normalized = sortBy.Trim();
            return SupportedSortFields.Contains(normalized);
        }
    }
}
