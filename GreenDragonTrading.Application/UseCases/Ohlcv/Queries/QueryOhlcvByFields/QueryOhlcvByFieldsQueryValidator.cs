using FluentValidation;
using GreenDragonTrading.Domain.Constants;

namespace GreenDragonTrading.Application.UseCases.Ohlcv.Queries.QueryOhlcvByFields
{
    public class QueryOhlcvByFieldsQueryValidator : AbstractValidator<QueryOhlcvByFieldsQuery>
    {
        private static readonly HashSet<string> SupportedOperators =
        [
            "eq", "neq", "gt", "gte", "lt", "lte", "contains", "isnull", "isnotnull"
        ];

        private static readonly HashSet<string> SupportedFields =
        [
            "time", "ticker", "timeframe", "open", "high", "low", "close", "volume", "value",
            "tradesCount", "createdAt", "source", "isPreliminary"
        ];

        public QueryOhlcvByFieldsQueryValidator()
        {
            RuleFor(x => x.Ticker)
                .NotEmpty().WithMessage("Ticker là bắt buộc.")
                .MaximumLength(20).WithMessage("Ticker không được vượt quá 20 ký tự.");

            RuleFor(x => x.Timeframe)
                .NotEmpty().WithMessage("Timeframe là bắt buộc.")
                .Must(OhlcvConstants.Timeframes.IsValid)
                .WithMessage($"Timeframe không hợp lệ. Phải là: {string.Join(", ", OhlcvConstants.Timeframes.All)}");

            RuleFor(x => x.PageIndex)
                .GreaterThan(0).WithMessage("PageIndex phải lớn hơn 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("PageSize phải lớn hơn 0.")
                .LessThanOrEqualTo(1000).WithMessage("PageSize không được vượt quá 1000.");

            RuleFor(x => x.Limit)
                .GreaterThan(0).WithMessage("Limit phải lớn hơn 0.")
                .LessThanOrEqualTo(50000).WithMessage("Limit không được vượt quá 50000.")
                .When(x => x.Limit.HasValue);

            RuleFor(x => x.SortBy)
                .Must(field => SupportedFields.Contains(field.Trim()))
                .WithMessage("SortBy không hợp lệ.");

            RuleFor(x => x.SortDirection)
                .Must(direction => direction.Equals("asc", StringComparison.OrdinalIgnoreCase)
                    || direction.Equals("desc", StringComparison.OrdinalIgnoreCase))
                .WithMessage("SortDirection chỉ chấp nhận asc hoặc desc.");

            RuleForEach(x => x.Filters).ChildRules(filter =>
            {
                filter.RuleFor(f => f.Field)
                    .NotEmpty().WithMessage("Filter.Field là bắt buộc.")
                    .Must(field => SupportedFields.Contains(field.Trim()))
                    .WithMessage("Filter.Field không hợp lệ.");

                filter.RuleFor(f => f.Operator)
                    .NotEmpty().WithMessage("Filter.Operator là bắt buộc.")
                    .Must(@operator => SupportedOperators.Contains(@operator.Trim().ToLowerInvariant()))
                    .WithMessage("Filter.Operator không hợp lệ.");

                filter.RuleFor(f => f.Value)
                    .NotEmpty().WithMessage("Filter.Value là bắt buộc với operator hiện tại.")
                    .When(f =>
                    {
                        var op = f.Operator.Trim().ToLowerInvariant();
                        return op != "isnull" && op != "isnotnull";
                    });
            });

            RuleFor(x => x)
                .Must(x => !x.FromTime.HasValue || !x.ToTime.HasValue || x.FromTime <= x.ToTime)
                .WithMessage("FromTime phải nhỏ hơn hoặc bằng ToTime.");
        }
    }
}
