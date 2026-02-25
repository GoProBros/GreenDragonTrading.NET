using FluentValidation;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Commands.CreateSubscription;

/// <summary>
/// Validator for CreateSubscriptionCommand
/// </summary>
public class CreateSubscriptionCommandValidator : AbstractValidator<CreateSubscriptionCommand>
{
    public CreateSubscriptionCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên gói đăng ký không được để trống")
            .MaximumLength(100).WithMessage("Tên gói đăng ký không được vượt quá 100 ký tự");

        RuleFor(x => x.LevelOrder)
            .IsInEnum().WithMessage("Cấp độ gói đăng ký không hợp lệ");

        RuleFor(x => x.MaxWorkspaces)
            .GreaterThan(0).WithMessage("Số lượng workspace tối đa phải lớn hơn 0");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Giá không được âm");

        RuleFor(x => x.DurationInDays)
            .GreaterThan(0).WithMessage("Số ngày sử dụng phải lớn hơn 0");

        RuleFor(x => x.AllowedModules)
            .NotEmpty().WithMessage("Danh sách module cho phép không được để trống")
            .Must(BeValidJsonArray).WithMessage("Danh sách module cho phép phải là một mảng JSON hợp lệ");
    }

    private static bool BeValidJsonArray(JsonElement json)
    {
        try
        {
            return json.ValueKind == JsonValueKind.Array;
        }
        catch
        {
            return false;
        }
    }
}
