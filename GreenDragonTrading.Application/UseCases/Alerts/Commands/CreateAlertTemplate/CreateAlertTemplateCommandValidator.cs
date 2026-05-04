using FluentValidation;
using GreenDragonTrading.Application.Common.Utils;
using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Application.UseCases.Alerts.Commands.CreateAlertTemplate;

public class CreateAlertTemplateCommandValidator : AbstractValidator<CreateAlertTemplateCommand>
{
    private readonly HashSet<string> _allowedKeys;

    public CreateAlertTemplateCommandValidator()
    {
        _allowedKeys = AlertTemplatePlaceholderCatalog
            .GetDefinitions()
            .Select(x => x.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        RuleFor(x => x.TitleTemplate)
            .NotEmpty().WithMessage("TitleTemplate la bat buoc.")
            .MaximumLength(500).WithMessage("TitleTemplate khong duoc vuot qua 500 ky tu.");

        RuleFor(x => x.BodyTemplate)
            .NotEmpty().WithMessage("BodyTemplate la bat buoc.")
            .MaximumLength(4000).WithMessage("BodyTemplate khong duoc vuot qua 4000 ky tu.");

        RuleFor(x => x)
            .Must(HasTypeAndConditionWhenNotDefault)
            .WithMessage("Type va Condition la bat buoc khi khong phai template mac dinh.");

        RuleFor(x => x.TitleTemplate)
            .Custom((value, context) => ValidatePlaceholders(value, context, "TitleTemplate"));

        RuleFor(x => x.BodyTemplate)
            .Custom((value, context) => ValidatePlaceholders(value, context, "BodyTemplate"));
    }

    private bool HasTypeAndConditionWhenNotDefault(CreateAlertTemplateCommand command)
    {
        if (command.IsDefault)
        {
            return true;
        }

        return command.Type.HasValue && command.Condition.HasValue;
    }

    private void ValidatePlaceholders(string template, ValidationContext<CreateAlertTemplateCommand> context, string field)
    {
        var placeholders = AlertTemplateRenderingHelper.ExtractPlaceholders(template);
        var invalid = placeholders.Where(x => !_allowedKeys.Contains(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (invalid.Count == 0)
        {
            return;
        }

        context.AddFailure(field, $"Placeholder khong hop le: {string.Join(", ", invalid)}");
    }
}
