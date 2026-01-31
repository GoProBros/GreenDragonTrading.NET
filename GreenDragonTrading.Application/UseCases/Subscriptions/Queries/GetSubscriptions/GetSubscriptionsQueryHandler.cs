using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Subscriptions.Queries.GetSubscriptions
{
    public class GetSubscriptionsQueryHandler : IRequestHandler<GetSubscriptionsQuery, ApiResponse<List<SubscriptionDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<GetSubscriptionsQueryHandler> _logger;

        public GetSubscriptionsQueryHandler(
            IUnitOfWork uow,
            ILogger<GetSubscriptionsQueryHandler> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<ApiResponse<List<SubscriptionDto>>> Handle(
            GetSubscriptionsQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all subscription packages");

            var subscriptions = await _uow.Subscriptions.GetAllAsync(cancellationToken);

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
                    AllowedModules = JsonDocument.Parse(s.AllowedModules).RootElement.Clone()
                })
                .ToList();

            _logger.LogInformation("Retrieved {Count} subscription packages", result.Count);
            return ApiResponse<List<SubscriptionDto>>.Success(result, "Lấy danh sách gói thành công.");
        }
    }
}
