using FluentValidation;
using GreenDragonTrading.Application.Common.Utils;

namespace GreenDragonTrading.Application.UseCases.Alerts.Commands.UpdateAlertTemplate;

public class UpdateAlertTemplateCommandValidator : AbstractValidator<UpdateAlertTemplateCommand>
{
    private readonly HashSet<string> _allowedKeys;

    public UpdateAlertTemplateCommandValidator()
    {
        _allowedKeys = AlertTemplatePlaceholderCatalog
            .GetDefinitions()
            .SelectMany(x => new[] { x.TokenKey, x.Key })
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id template Không hợp lệ.");

        RuleFor(x => x.TitleTemplate)
            .NotEmpty().WithMessage("TitleTemplate là bắt buộc.")
            .MaximumLength(500).WithMessage("TitleTemplate không được vượt qua 500 ký tự.");

        RuleFor(x => x.BodyTemplate)
            .NotEmpty().WithMessage("BodyTemplate là bắt buộc.")
            .MaximumLength(4000).WithMessage("BodyTemplate không được vượt qua 4000 ký tự.");

        RuleFor(x => x)
            .Must(HasTypeAndConditionWhenNotDefault)
            .WithMessage("Type và Condition là bắt buộc khi không phải template mặc định.");

        RuleFor(x => x.TitleTemplate)
            .Custom((value, context) => ValidatePlaceholders(value, context, "TitleTemplate"));

        RuleFor(x => x.BodyTemplate)
            .Custom((value, context) => ValidatePlaceholders(value, context, "BodyTemplate"));
    }

    private bool HasTypeAndConditionWhenNotDefault(UpdateAlertTemplateCommand command)
    {
        if (command.IsDefault)
        {
            return true;
        }

        return command.Type.HasValue && command.Condition.HasValue;
    }

    private void ValidatePlaceholders(string template, ValidationContext<UpdateAlertTemplateCommand> context, string field)
    {
        var placeholders = AlertTemplateRenderingHelper.ExtractPlaceholders(template);
        var invalid = placeholders.Where(x => !_allowedKeys.Contains(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (invalid.Count == 0)
        {
            return;
        }

        context.AddFailure(field, $"Placeholder không hợp lệ: {string.Join(", ", invalid)}");
    }
}
