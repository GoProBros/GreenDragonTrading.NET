using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Constants;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Alerts.Commands.DeleteAlert
{
    public class DeleteAlertCommandHandler(
        IUnitOfWork uow,
        ICurrentUserService currentUserService,
        IRedisService redisService) : IRequestHandler<DeleteAlertCommand, ApiResponse>
    {
        private readonly IUnitOfWork _uow = uow;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IRedisService _redisService = redisService;

        public async Task<ApiResponse> Handle(DeleteAlertCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetRequiredUserId();

            var alert = await _uow.Alerts.GetByIdAsync(request.Id, cancellationToken);
            if (alert == null || alert.UserId != userId)
            {
                throw new NotFoundException("Không tìm thấy cảnh báo");
            }

            await _redisService.SortedSetRemoveAsync(
                RedisConstants.AlertsByTypeAndCondition(alert.Ticker, alert.Type, alert.Condition),
                alert.Id.ToString());

            // Backward-compatible cleanup for old redis key layout.
            await _redisService.SortedSetRemoveAsync(RedisConstants.AlertsAbove(alert.Ticker), alert.Id.ToString());
            await _redisService.SortedSetRemoveAsync(RedisConstants.AlertsBelow(alert.Ticker), alert.Id.ToString());

            _uow.Alerts.Remove(alert);
            await _uow.SaveChangesAsync(cancellationToken);

            return ApiResponse.Success("Xóa cảnh báo thành công");
        }
    }
}
