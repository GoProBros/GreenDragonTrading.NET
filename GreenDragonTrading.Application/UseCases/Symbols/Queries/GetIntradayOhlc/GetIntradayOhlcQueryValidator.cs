using FluentValidation;
using GreenDragonTrading.Application.Common.Validation;
using System.Globalization;

namespace GreenDragonTrading.Application.UseCases.Symbols.Queries.GetIntradayOhlc
{
    /// <summary>
    /// Validator for GetIntradayOhlcQuery
    /// </summary>
    public class GetIntradayOhlcQueryValidator : AbstractValidator<GetIntradayOhlcQuery>
    {
        private static readonly int[] AllowedResolutions = { 1, 3, 5, 15, 30, 60, 240 };

        public GetIntradayOhlcQueryValidator()
        {
            RuleFor(x => x.Symbol)
                .NotEmpty()
                .WithMessage("Symbol không được để trống")
                .MaximumLength(20)
                .WithMessage("Symbol không được vượt quá 20 ký tự");

            RuleFor(x => x.FromDate)
                .NotEmpty()
                .WithMessage("FromDate không được để trống")
                .Must(BeValidDateFormat)
                .WithMessage("FromDate phải có định dạng dd/MM/yyyy (ví dụ: 01/12/2024)");

            RuleFor(x => x.ToDate)
                .NotEmpty()
                .WithMessage("ToDate không được để trống")
                .Must(BeValidDateFormat)
                .WithMessage("ToDate phải có định dạng dd/MM/yyyy (ví dụ: 31/12/2024)");

            RuleFor(x => x)
                .Must(x => IsFromDateBeforeToDate(x.FromDate, x.ToDate))
                .When(x => BeValidDateFormat(x.FromDate) && BeValidDateFormat(x.ToDate))
                .WithMessage("FromDate phải nhỏ hơn hoặc bằng ToDate");

            RuleFor(x => x.Resolution)
                .Must(BeValidResolution)
                .When(x => x.Resolution.HasValue)
                .WithMessage($"Resolution phải là một trong các giá trị: {string.Join(", ", AllowedResolutions)} (phút)");

            RuleFor(x => x.PageIndex)
                .GreaterThan(0)
                .WithMessage("PageIndex phải lớn hơn 0");

            RuleFor(x => x.PageSize)
                .ValidPageSize();
        }

        private static bool BeValidDateFormat(string? date)
        {
            if (string.IsNullOrWhiteSpace(date))
                return false;

            return DateTime.TryParseExact(
                date,
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _);
        }

        private static bool IsFromDateBeforeToDate(string fromDate, string toDate)
        {
            if (!DateTime.TryParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from))
                return false;

            if (!DateTime.TryParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var to))
                return false;

            return from <= to;
        }

        private static bool BeValidResolution(int? resolution)
        {
            if (!resolution.HasValue)
                return true;

            return AllowedResolutions.Contains(resolution.Value);
        }
    }
}
