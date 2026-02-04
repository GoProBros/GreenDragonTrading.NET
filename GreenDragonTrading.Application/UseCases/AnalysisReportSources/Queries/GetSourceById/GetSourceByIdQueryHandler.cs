using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportSources.Queries.GetSourceById;

/// <summary>
/// Handler for GetSourceByIdQuery
/// </summary>
public class GetSourceByIdQueryHandler : IRequestHandler<GetSourceByIdQuery, ApiResponse<AnalysisReportSourceDto>>
{
    private readonly IUnitOfWork _uow;

    public GetSourceByIdQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ApiResponse<AnalysisReportSourceDto>> Handle(GetSourceByIdQuery request, CancellationToken cancellationToken)
    {
        var source = await _uow.AnalysisReportSources.GetByIdAsync(request.Id, cancellationToken);
        if (source == null)
        {
            throw new NotFoundException($"Nguồn với ID '{request.Id}' không tồn tại");
        }

        var dto = new AnalysisReportSourceDto
        {
            Code = source.Code,
            Name = source.Name,
            Description = source.Description,
            Website = source.Website,
            LogoUrl = source.LogoUrl,
            Status = source.Status,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };

        return ApiResponse<AnalysisReportSourceDto>.Success(dto, "Lấy thông tin nguồn báo cáo thành công");
    }
}
