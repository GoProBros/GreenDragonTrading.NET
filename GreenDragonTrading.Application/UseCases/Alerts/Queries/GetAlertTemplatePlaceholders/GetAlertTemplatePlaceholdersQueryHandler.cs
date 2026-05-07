using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Common.Utils;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Queries.GetAlertTemplatePlaceholders;

public class GetAlertTemplatePlaceholdersQueryHandler
    : IRequestHandler<GetAlertTemplatePlaceholdersQuery, ApiResponse<AlertTemplatePlaceholdersDto>>
{
    public Task<ApiResponse<AlertTemplatePlaceholdersDto>> Handle(
        GetAlertTemplatePlaceholdersQuery request,
        CancellationToken cancellationToken)
    {
        var placeholders = AlertTemplatePlaceholderCatalog
            .GetDefinitions()
            .Select(definition => new AlertTemplatePlaceholderDto
            {
                Key = definition.Key,
                Token = AlertTemplatePlaceholderCatalog.ToToken(definition.TokenKey),
                Category = definition.Category,
                Description = definition.Description
            })
            .ToList();

        var dto = new AlertTemplatePlaceholdersDto
        {
            Placeholders = placeholders
        };

        return Task.FromResult(ApiResponse<AlertTemplatePlaceholdersDto>.Success(
            dto,
            "Lấy danh sách placeholder thành công."));
    }
}
