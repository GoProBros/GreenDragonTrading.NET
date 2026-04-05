using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Enums;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Queries.GetSubscriptions
{
    public class GetSubscriptionsQueryHandler : IRequestHandler<GetSubscriptionsQuery, ApiResponse<List<SubscriptionDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetSubscriptionsQueryHandler> _logger;

        public GetSubscriptionsQueryHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUserService,
            ILogger<GetSubscriptionsQueryHandler> logger)
        {
            _uow = uow;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ApiResponse<List<SubscriptionDto>>> Handle(
            GetSubscriptionsQuery request,
            CancellationToken cancellationToken)
        {
            var includeInactive = _currentUserService.IsAdminOrStaff;

            _logger.LogInformation(
                "Getting subscription packages. IncludeInactive={IncludeInactive}, Role={Role}",
                includeInactive,
                _currentUserService.Role ?? "Anonymous");

            var subscriptions = await _uow.Subscriptions.GetAllAsync(cancellationToken);

            if (!includeInactive)
            {
                subscriptions = subscriptions
                    .Where(s => s.IsActive == CommonStatus.Active)
                    .ToList();
            }

            var result = subscriptions
                .OrderBy(s => s.LevelOrder)
                .Select(s => new SubscriptionDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    LevelOrder = s.LevelOrder,
                    MaxWorkspaces = s.MaxWorkspaces,
                    Price = s.Price,
                    DurationInDays = s.DurationInDays,
                    IsActive = s.IsActive,
                    AllowedModules = JsonDocument.Parse(s.AllowedModules).RootElement.Clone()
                })
                .ToList();

            _logger.LogInformation("Retrieved {Count} subscription packages", result.Count);
            return ApiResponse<List<SubscriptionDto>>.Success(result, "Lấy danh sách gói thành công.");
        }
    }
}
