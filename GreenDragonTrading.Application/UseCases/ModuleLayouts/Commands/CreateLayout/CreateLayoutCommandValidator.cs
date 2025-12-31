using FluentValidation;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.ModuleLayouts.Commands.CreateLayout;

public class CreateLayoutCommandValidator : AbstractValidator<CreateLayoutCommand>
{
    public CreateLayoutCommandValidator()
    {
        RuleFor(x => x.LayoutName)
            .NotEmpty().WithMessage("Tên layout không được để trống")
            .MaximumLength(100).WithMessage("Tên layout không được vượt quá 100 ký tự");

        RuleFor(x => x.ModuleType)
            .IsInEnum().WithMessage("Loại module không hợp lệ");

        RuleFor(x => x.ConfigJson)
            .NotEmpty().WithMessage("Cấu hình JSON không được để trống")
            .Must(BeValidJsonElement).WithMessage("Cấu hình JSON không hợp lệ");
    }

    private static bool BeValidJsonElement(JsonElement json)
    {
        try
        {
            return json.ValueKind != JsonValueKind.Undefined;
        }
        catch
        {
            return false;
        }
    }
}
