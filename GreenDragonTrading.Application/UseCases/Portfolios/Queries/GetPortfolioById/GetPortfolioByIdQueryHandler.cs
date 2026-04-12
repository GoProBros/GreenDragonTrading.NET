using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Portfolios.Queries.GetPortfolioById;

public class GetPortfolioByIdQueryHandler : IRequestHandler<GetPortfolioByIdQuery, ApiResponse<PortfolioDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetPortfolioByIdQueryHandler> _logger;

    public GetPortfolioByIdQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<GetPortfolioByIdQueryHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<PortfolioDto>> Handle(GetPortfolioByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var role = _currentUserService.Role;

        Portfolio? portfolio;

        if (string.Equals(role, nameof(UserRole.User), StringComparison.OrdinalIgnoreCase))
        {
            portfolio = await _uow.Portfolios.GetByIdAndUserIdAsync(request.Id, userId, cancellationToken);
            if (portfolio == null)
            {
                throw new NotFoundException("Portfolio không tồn tại hoặc bạn không có quyền truy cập.");
            }
        }
        else if (_currentUserService.IsAdminOrStaff)
        {
            portfolio = await _uow.Portfolios.GetByIdAsync(request.Id, cancellationToken);
            if (portfolio == null)
            {
                throw new NotFoundException("Portfolio không tồn tại.");
            }

            var owner = await _uow.Users.GetByIdAsync(portfolio.UserId, cancellationToken);
            if (owner == null || owner.Role != UserRole.User)
            {
                throw new AccessDeniedException("Staff/Admin chỉ được xem portfolio của tài khoản User.");
            }
        }
        else
        {
            throw new AccessDeniedException("Bạn không có quyền xem portfolio.");
        }

        _logger.LogInformation("Portfolio {PortfolioId} was retrieved by {Role} {UserId}", request.Id, role, userId);

        return ApiResponse<PortfolioDto>.Success(ToDto(portfolio), "Lấy portfolio thành công");
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
