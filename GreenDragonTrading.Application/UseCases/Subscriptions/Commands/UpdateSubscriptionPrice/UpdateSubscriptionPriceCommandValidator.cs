using FluentValidation;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Commands.UpdateSubscriptionPrice
{
    public class UpdateSubscriptionPriceCommandValidator : AbstractValidator<UpdateSubscriptionPriceCommand>
    {
        public UpdateSubscriptionPriceCommandValidator()
        {
            RuleFor(x => x.SubscriptionId)
                .GreaterThan(0).WithMessage("Mã gói đăng ký không hợp lệ");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Giá không được âm")
                .When(x => x.Price.HasValue);

            RuleFor(x => x.AllowedModules)
                .Must(BeValidAllowedModules)
                .WithMessage("AllowedModules phải là một mảng JSON không rỗng")
                .When(x => x.AllowedModules.HasValue);

            RuleFor(x => x)
                .Must(x => x.Price.HasValue || x.AllowedModules.HasValue)
                .WithMessage("Phải cung cấp ít nhất một trường cần cập nhật: Price hoặc AllowedModules");
        }

        private static bool BeValidAllowedModules(JsonElement? allowedModules)
        {
            if (!allowedModules.HasValue)
            {
                return true;
            }

            var value = allowedModules.Value;
            return value.ValueKind == JsonValueKind.Array && value.GetArrayLength() > 0;
        }
    }
}
