using FluentValidation;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.WatchLists.Commands.CreateWatchList;

public class CreateWatchListCommandValidator : AbstractValidator<CreateWatchListCommand>
{
    public CreateWatchListCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên watchlist không được để trống")
            .MaximumLength(200).WithMessage("Tên watchlist không được vượt quá 200 ký tự");

        RuleFor(x => x.Tickers)
            .NotEmpty().WithMessage("Danh sách mã chứng khoán không được để trống")
            .Must(BeValidJsonArray).WithMessage("Danh sách mã chứng khoán phải là một mảng JSON hợp lệ");
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
