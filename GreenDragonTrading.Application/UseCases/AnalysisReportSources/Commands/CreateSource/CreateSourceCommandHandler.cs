using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportSources.Commands.CreateSource;

/// <summary>
/// Handler for CreateSourceCommand
/// </summary>
public class CreateSourceCommandHandler : IRequestHandler<CreateSourceCommand, ApiResponse<AnalysisReportSourceDto>>
{
    private readonly IUnitOfWork _uow;

    public CreateSourceCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ApiResponse<AnalysisReportSourceDto>> Handle(CreateSourceCommand request, CancellationToken cancellationToken)
    {
        // Check if source Code already exists
        var existingSource = await _uow.AnalysisReportSources.GetByIdAsync(request.Code, cancellationToken);
        if (existingSource != null)
        {
            throw new ConflictException($"Nguồn với Code '{request.Code}' đã tồn tại");
        }

        var source = new AnalysisReportSource
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Website = request.Website,
            LogoUrl = request.LogoUrl,
            Status = CommonStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _uow.AnalysisReportSources.AddAsync(source, cancellationToken);
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

        return ApiResponse<AnalysisReportSourceDto>.Success(dto, "Tạo nguồn báo cáo thành công");
    }
}
