using FluentValidation;
using System.Globalization;

namespace GreenDragonTrading.Application.UseCases.Symbols.Queries.GetDailyOhlcv
{
    /// <summary>
    /// Validator for GetDailyOhlcvQuery
    /// </summary>
    public class GetDailyOhlcvQueryValidator : AbstractValidator<GetDailyOhlcvQuery>
    {
        public GetDailyOhlcvQueryValidator()
        {
            RuleFor(x => x.Symbol)
                .NotEmpty()
                .WithMessage("Symbol không được để trống")
                .MaximumLength(20)
                .WithMessage("Symbol không được vượt quá 20 ký tự");

            RuleFor(x => x.FromDate)
                .Must(BeValidDateFormat)
                .When(x => !string.IsNullOrWhiteSpace(x.FromDate))
                .WithMessage("FromDate phải có định dạng dd/MM/yyyy (ví dụ: 01/12/2024)");

            RuleFor(x => x.ToDate)
                .Must(BeValidDateFormat)
                .When(x => !string.IsNullOrWhiteSpace(x.ToDate))
                .WithMessage("ToDate phải có định dạng dd/MM/yyyy (ví dụ: 31/12/2024)");

            RuleFor(x => x)
                .Must(x => IsFromDateBeforeToDate(x.FromDate, x.ToDate))
                .When(x => !string.IsNullOrWhiteSpace(x.FromDate) && !string.IsNullOrWhiteSpace(x.ToDate))
                .WithMessage("FromDate phải nhỏ hơn hoặc bằng ToDate");

            RuleFor(x => x)
                .Must(x => IsDateRangeValid(x.FromDate, x.ToDate))
                .When(x => !string.IsNullOrWhiteSpace(x.FromDate) && !string.IsNullOrWhiteSpace(x.ToDate))
                .WithMessage("Khoảng thời gian không được vượt quá 2 năm");
        }

        private static bool BeValidDateFormat(string? date)
        {
            if (string.IsNullOrWhiteSpace(date))
                return true;

            return DateTime.TryParseExact(
                date,
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _);
        }

        private static bool IsFromDateBeforeToDate(string? fromDate, string? toDate)
        {
            if (string.IsNullOrWhiteSpace(fromDate) || string.IsNullOrWhiteSpace(toDate))
                return true;

            if (!DateTime.TryParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from))
                return true;

            if (!DateTime.TryParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var to))
                return true;

            return from <= to;
        }

        private static bool IsDateRangeValid(string? fromDate, string? toDate)
        {
            if (string.IsNullOrWhiteSpace(fromDate) || string.IsNullOrWhiteSpace(toDate))
                return true;

            if (!DateTime.TryParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from))
                return true;

            if (!DateTime.TryParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var to))
                return true;

            return (to - from).TotalDays <= 730; 
        }
    }
}
