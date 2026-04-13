using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.News;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.News.Queries.GetNewsById;

public class GetNewsByIdQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetNewsByIdQuery, ApiResponse<NewsArticleDto>>
{
    private readonly IUnitOfWork _uow = uow;

    public async Task<ApiResponse<NewsArticleDto>> Handle(GetNewsByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.Id <= 0)
        {
            throw new BusinessRuleException("Id bài viết không hợp lệ.");
        }

        var article = await _uow.NewsArticles.GetByIdWithTagsAsync(request.Id, cancellationToken);
        if (article == null)
        {
            throw new NotFoundException("Không tìm thấy bài viết.");
        }

        var dto = NewsArticleMapper.ToDto(article);
        return ApiResponse<NewsArticleDto>.Success(dto, "Lấy chi tiết tin tức thành công.");
    }
}
