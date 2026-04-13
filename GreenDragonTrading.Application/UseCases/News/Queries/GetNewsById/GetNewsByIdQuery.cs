using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.News.Queries.GetNewsById;

public record GetNewsByIdQuery(int Id) : IRequest<ApiResponse<NewsArticleDto>>;
