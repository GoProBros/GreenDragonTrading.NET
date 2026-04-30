using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.CorporateActions.Queries.GetCorporateActionById;

/// <summary>
/// Handler for GetCorporateActionByIdQuery.
/// </summary>
public class GetCorporateActionByIdQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetCorporateActionByIdQuery, ApiResponse<CorporateActionDto>>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<ApiResponse<CorporateActionDto>> Handle(
        GetCorporateActionByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (request.EventId <= 0)
        {
            throw new BusinessRuleException("EventId không hợp lệ.");
        }

        var entity = await _uow.CorporateActions.GetByIdAsync(request.EventId, cancellationToken);
        if (entity == null)
        {
            throw new NotFoundException("Không tìm thấy sự kiện cổ tức.");
        }

        var dto = CorporateActionMapper.ToDto(entity);
        return ApiResponse<CorporateActionDto>.Success(dto, "Lấy chi tiết sự kiện cổ tức thành công.");
    }
}
