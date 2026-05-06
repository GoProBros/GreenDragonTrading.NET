using FluentValidation;
using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Application.UseCases.Alerts.Commands.CreateAlert
{
    public class CreateAlertCommandValidator : AbstractValidator<CreateAlertCommand>
    {
        public CreateAlertCommandValidator()
        {
            RuleFor(x => x.Ticker)
                .NotEmpty().WithMessage("Mã cổ phiếu không được để trống")
                .MaximumLength(20).WithMessage("Mã cổ phiếu không hợp lệ");

            RuleFor(x => x.Type)
                .Must(t => t is AlertType.Price or AlertType.Volume)
                .WithMessage("Loại cảnh báo chỉ hỗ trợ Price(1) hoặc Volume(2)");

            RuleFor(x => x.Condition)
                .Must(c => c is ConditionType.Above or ConditionType.Below or ConditionType.PercentChangeUp or ConditionType.PercentChangeDown)
                .WithMessage("Điều kiện cảnh báo chỉ hỗ trợ 1, 2, 3, 4");

            RuleFor(x => x.ThresholdValue)
                .NotNull().WithMessage("Ngưỡng cảnh báo không được để trống")
                .GreaterThan(0).WithMessage("Ngưỡng cảnh báo phải lớn hơn 0")
                .When(x => x.Condition is ConditionType.Above or ConditionType.Below);

            RuleFor(x => x.ChangePercentage)
                .NotNull().WithMessage("Phần trăm thay đổi không được để trống")
                .GreaterThan(0).WithMessage("Phần trăm thay đổi phải lớn hơn 0")
                .When(x => x.Condition is ConditionType.PercentChangeUp or ConditionType.PercentChangeDown);

            When(x => x.Type == AlertType.Volume, () =>
            {
                RuleFor(x => x.VolumeTimeFrame)
                    .NotNull().WithMessage("Khung thời gian không được để trống")
                    .IsInEnum().WithMessage("Khung thời gian không hợp lệ");

                RuleFor(x => x.VolumeLookbackBars)
                    .NotNull().WithMessage("Số lượng nến so sánh không được để trống")
                    .GreaterThan(0).WithMessage("Số lượng nến so sánh phải lớn hơn 0")
                    .When(x => x.Condition is ConditionType.PercentChangeUp or ConditionType.PercentChangeDown);
            });

            RuleFor(x => x.Name)
                .MaximumLength(255).WithMessage("Tên cảnh báo không được vượt quá 255 ký tự")
                .When(x => !string.IsNullOrWhiteSpace(x.Name));
        }
    }
}
