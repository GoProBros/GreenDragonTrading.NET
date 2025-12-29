using FluentValidation;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.ModuleLayouts.Commands.UpdateLayout;

public class UpdateLayoutCommandValidator : AbstractValidator<UpdateLayoutCommand>
{
    public UpdateLayoutCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Layout ID phải lớn hơn 0");

        When(x => !string.IsNullOrEmpty(x.LayoutName), () =>
        {
            RuleFor(x => x.LayoutName)
                .MaximumLength(100).WithMessage("Tên layout không được vượt quá 100 ký tự");
        });

        When(x => x.ConfigJson.HasValue, () =>
        {
            RuleFor(x => x.ConfigJson)
                .Must(BeValidJsonElement).WithMessage("Cấu hình JSON không hợp lệ");
        });

        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.LayoutName) || x.ConfigJson.HasValue || x.IsSystemDefault.HasValue)
            .WithMessage("Phải cung cấp ít nhất một trường để cập nhật (LayoutName, ConfigJson hoặc IsSystemDefault)");
    }

    private static bool BeValidJsonElement(JsonElement? json)
    {
        if (!json.HasValue) return true;

        try
        {
            return json.Value.ValueKind != JsonValueKind.Undefined;
        }
        catch
        {
            return false;
        }
    }
}
