using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportSources.Commands.UpdateSource;

/// <summary>
/// Handler for UpdateSourceCommand
/// </summary>
public class UpdateSourceCommandHandler : IRequestHandler<UpdateSourceCommand, ApiResponse<AnalysisReportSourceDto>>
{
    private readonly IUnitOfWork _uow;

    public UpdateSourceCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ApiResponse<AnalysisReportSourceDto>> Handle(UpdateSourceCommand request, CancellationToken cancellationToken)
    {
        var source = await _uow.AnalysisReportSources.GetByIdAsync(request.Id, cancellationToken);
        if (source == null)
        {
            throw new NotFoundException($"Nguồn với ID '{request.Id}' không tồn tại");
        }

        source.Name = request.Name;
        source.Description = request.Description;
        source.Website = request.Website;
        source.LogoUrl = request.LogoUrl;
        source.Status = request.Status;
        source.UpdatedAt = DateTimeOffset.UtcNow;

        _uow.AnalysisReportSources.Update(source);
        await _uow.SaveChangesAsync(cancellationToken);

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

        return ApiResponse<AnalysisReportSourceDto>.Success(dto, "Cập nhật nguồn báo cáo thành công");
    }
}
