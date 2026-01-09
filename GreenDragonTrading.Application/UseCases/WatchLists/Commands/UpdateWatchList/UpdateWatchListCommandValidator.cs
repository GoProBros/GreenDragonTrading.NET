using FluentValidation;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.WatchLists.Commands.UpdateWatchList;

public class UpdateWatchListCommandValidator : AbstractValidator<UpdateWatchListCommand>
{
    public UpdateWatchListCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("ID watchlist không hợp lệ");

        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Tên watchlist không được vượt quá 200 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Tickers)
            .Must(BeValidJsonArray).WithMessage("Danh sách mã chứng khoán phải là một mảng JSON hợp lệ")
            .When(x => x.Tickers.HasValue);
    }

    private static bool BeValidJsonArray(JsonElement? json)
    {
        try
        {
            return json.HasValue && json.Value.ValueKind == JsonValueKind.Array;
        }
        catch
        {
            return false;
        }
    }
}
