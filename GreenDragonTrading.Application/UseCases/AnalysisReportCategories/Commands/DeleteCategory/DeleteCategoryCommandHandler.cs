using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.AnalysisReportCategories.Commands.DeleteCategory;

/// <summary>
/// Handler for DeleteCategoryCommand
/// </summary>
public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, ApiResponse>
{
    private readonly IUnitOfWork _uow;

    public DeleteCategoryCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ApiResponse> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _uow.AnalysisReportCategories.GetByIdAsync(request.Id, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException($"Danh mục với ID '{request.Id}' không tồn tại");
        }

        // Check if category has children
        var allCategories = await _uow.AnalysisReportCategories.GetAllAsync(cancellationToken);
        var hasChildren = allCategories.Any(c => c.ParentId == request.Id);
        if (hasChildren)
        {
            throw new BusinessRuleException("Không thể xóa danh mục có danh mục con");
        }

        // Soft delete
        category.Status = CommonStatus.InActive;
        category.UpdatedAt = DateTimeOffset.UtcNow;

        _uow.AnalysisReportCategories.Update(category);
        await _uow.SaveChangesAsync(cancellationToken);

        return ApiResponse.Success("Xóa danh mục thành công");
    }
}
