using FluentValidation;

namespace GreenDragonTrading.Application.Common.Validation
{
    public static class PaginationValidatorRules
    {
        public static IRuleBuilderOptions<T, int> ValidPageSize<T>(this IRuleBuilder<T, int> ruleBuilder)
        {
            int[] allowedSizes = { 10, 20, 50, 100, 1000, 5000 };

            return ruleBuilder
                .Must(size => allowedSizes.Contains(size))
                .WithMessage($"PageSize chỉ chấp nhận các giá trị: {string.Join(", ", allowedSizes)}");
        }

    }
}
