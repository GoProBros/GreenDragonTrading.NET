using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Portfolios.Queries.GetPortfolios;

public class GetPortfoliosQueryHandler : IRequestHandler<GetPortfoliosQuery, ApiResponse<List<PortfolioDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetPortfoliosQueryHandler> _logger;

    public GetPortfoliosQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<GetPortfoliosQueryHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<List<PortfolioDto>>> Handle(GetPortfoliosQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.GetRequiredUserId();
        var role = _currentUserService.Role;

        List<Portfolio> portfolios;

        if (string.Equals(role, nameof(UserRole.User), StringComparison.OrdinalIgnoreCase))
        {
            portfolios = await _uow.Portfolios.GetByUserIdAsync(currentUserId, cancellationToken);
        }
        else if (_currentUserService.IsAdminOrStaff)
        {
            var endUserIds = (await _uow.Users.GetAllAsync(cancellationToken))
                .Where(x => x.Role == UserRole.User)
                .Select(x => x.Id)
                .ToHashSet();

            portfolios = (await _uow.Portfolios.GetAllAsync(cancellationToken))
                .Where(x => endUserIds.Contains(x.UserId))
                .ToList();

            if (request.UserId.HasValue)
            {
                portfolios = portfolios
                    .Where(x => x.UserId == request.UserId.Value)
                    .ToList();
            }
        }
        else
        {
            throw new AccessDeniedException("Bạn không có quyền xem danh sách portfolio.");
        }

        var result = portfolios
            .OrderByDescending(x => x.CreatedAt)
            .Select(ToDto)
            .ToList();

        _logger.LogInformation("Retrieved {Count} portfolios for {Role} {UserId}", result.Count, role, currentUserId);

        return ApiResponse<List<PortfolioDto>>.Success(result, "Lấy danh sách portfolio thành công");
    }

    private static PortfolioDto ToDto(Portfolio portfolio)
    {
        return new PortfolioDto
        {
            Id = portfolio.Id,
            UserId = portfolio.UserId,
            Name = portfolio.Name,
            Description = portfolio.Description,
            Status = portfolio.Status,
            CreatedAt = portfolio.CreatedAt
        };
    }
}
