using FluentValidation;
using GreenDragonTrading.Application.Common.Validation;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Enums;

namespace GreenDragonTrading.Application.UseCases.Symbols.Queries.GetSymbols
{
    /// <summary>
    /// GetSymbolsQuery Validator
    /// </summary>
    public class GetSymbolsQueryValidator : AbstractValidator<GetSymbolsQuery>
    {
        /// <summary>
        /// GetSymbolsQuery Validator
        /// </summary>
        public GetSymbolsQueryValidator()
        {
            RuleFor(x => x.PageIndex)
                .GreaterThan(0)
                .WithMessage("PageIndex phải lớn hơn 0");

            RuleFor(x => x.PageSize)
                .ValidPageSize();

            RuleFor(x => x.Type)
                .Must(BeValidSymbolType)
                .When(x => x.Type.HasValue)
                .WithMessage("Type không hợp lệ");

            RuleFor(x => x.Exchange)
                .Must(BeValidExchange)
                .When(x => !string.IsNullOrWhiteSpace(x.Exchange))
                .WithMessage("Exchange phải là HSX, HNX hoặc UPCOM");

            RuleFor(x => x.Sector)
                .MaximumLength(10)
                .When(x => !string.IsNullOrWhiteSpace(x.Sector))
                .WithMessage("Sector không được vượt quá 10 ký tự");
        }

        /// <summary>
        /// Determines whether the specified value represents a valid symbol type or is unspecified.
        /// </summary>
        /// <param name="type">The symbol type value to validate. If null, the value is considered unspecified and valid.</param>
        /// <returns>true if the value is null or corresponds to a defined member of the SymbolType enumeration; otherwise, false.</returns>
        private static bool BeValidSymbolType(int? type)
        {
            if (!type.HasValue) return true;
            return Enum.IsDefined(typeof(SymbolType), (short)type.Value);
        }

        /// <summary>
        /// Determines whether the specified value represents a valid exchange or is unspecified.
        /// </summary>
        /// <param name="exchange">The exchange value to validate. If null, the value is considered unspecified and valid.</param>
        /// <returns>true if the value is null or corresponds to a defined member of the Exchange enumeration; otherwise, false.</returns>
        private static bool BeValidExchange(string? exchange)
        {
            if (string.IsNullOrWhiteSpace(exchange)) return true;
            
            var validExchanges = new[] 
            { 
                ExchangeConstant.EXCHANGE_HSX, 
                ExchangeConstant.EXCHANGE_HNX, 
                ExchangeConstant.EXCHANGE_UPCOM 
            };
            
            return validExchanges.Contains(exchange.ToUpper());
        }
    }
}
