using FluentValidation;

namespace GreenDragonTrading.Application.UseCases.FinancialReports.Queries.GetFinancialReports
{
    public class GetFinancialReportsQueryValidator : AbstractValidator<GetFinancialReportsQuery>
    {
        public GetFinancialReportsQueryValidator()
        {
            RuleFor(x => x.PageIndex)
                .GreaterThan(0).WithMessage("PageIndex phải lớn hơn 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("PageSize phải lớn hơn 0.")
                .LessThanOrEqualTo(100).WithMessage("PageSize không được vượt quá 100.");

            When(x => !string.IsNullOrEmpty(x.Ticker), () =>
            {
                RuleFor(x => x.Ticker)
                    .MaximumLength(20).WithMessage("Ticker không được vượt quá 20 ký tự.");
            });

            When(x => x.Year.HasValue, () =>
            {
                RuleFor(x => x.Year!.Value)
                    .GreaterThan(2000).WithMessage("Năm phải lớn hơn 2000.")
                    .LessThanOrEqualTo(DateTime.Now.Year + 1).WithMessage("Năm không được lớn hơn năm hiện tại.");
            });

            When(x => x.Period.HasValue, () =>
            {
                RuleFor(x => x.Period!.Value)
                    .IsInEnum().WithMessage("Kỳ báo cáo không hợp lệ.");
            });

            When(x => x.Status.HasValue, () =>
            {
                RuleFor(x => x.Status!.Value)
                    .IsInEnum().WithMessage("Trạng thái không hợp lệ.");
            });
        }
    }
}
