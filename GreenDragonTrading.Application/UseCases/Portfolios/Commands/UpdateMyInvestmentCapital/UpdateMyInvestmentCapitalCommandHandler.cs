using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GreenDragonTrading.Application.UseCases.Portfolios.Commands.UpdateMyInvestmentCapital;

public class UpdateMyInvestmentCapitalCommandHandler
    : IRequestHandler<UpdateMyInvestmentCapitalCommand, ApiResponse<UserInvestmentCapitalDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateMyInvestmentCapitalCommandHandler> _logger;

    public UpdateMyInvestmentCapitalCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        ILogger<UpdateMyInvestmentCapitalCommandHandler> logger)
    {
        _uow = uow;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<UserInvestmentCapitalDto>> Handle(UpdateMyInvestmentCapitalCommand request, CancellationToken cancellationToken)
    {
        if (!string.Equals(_currentUserService.Role, nameof(UserRole.User), StringComparison.OrdinalIgnoreCase))
        {
            throw new AccessDeniedException("Chỉ người dùng role User mới được cập nhật tiền đầu tư khả dụng.");
        }

        var userId = _currentUserService.GetRequiredUserId();

        var user = await _uow.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("Người dùng không tồn tại.");
        }

        user.InvestmentCapital = request.InvestmentCapital;
        _uow.Users.Update(user);

        await _uow.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} updated available investment capital to {InvestmentCapital}",
            userId,
            request.InvestmentCapital);

        var response = new UserInvestmentCapitalDto
        {
            UserId = userId,
            AvailableCapital = user.InvestmentCapital ?? 0m
        };

        return ApiResponse<UserInvestmentCapitalDto>.Success(response, "Cập nhật tiền đầu tư khả dụng thành công");
    }
}