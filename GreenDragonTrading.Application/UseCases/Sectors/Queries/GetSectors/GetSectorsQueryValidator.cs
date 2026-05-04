using FluentValidation;
using GreenDragonTrading.Application.Common.Validation;
using GreenDragonTrading.Application.UseCases.Symbols.Queries.GetSymbols;

namespace GreenDragonTrading.Application.UseCases.Sectors.Queries.GetSectors
{
    public class GetSectorsQueryValidator : AbstractValidator<GetSectorsQuery>
    {
        /// <summary>
        /// GetSectorsQuery Validator
        /// </summary>
        public GetSectorsQueryValidator()
        {
            RuleFor(x => x.PageIndex)
                .GreaterThan(0)
                .WithMessage("PageIndex phải lớn hơn 0");

            RuleFor(x => x.PageSize)
                .ValidPageSize();

            RuleFor(x => x.Level)
                .InclusiveBetween(1, 5)
                .WithMessage("Level phải nằm trong khoảng từ 1 đến 5");
        }
    }
}
