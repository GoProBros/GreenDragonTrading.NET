using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Exceptions;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Chat.Queries.GetChatJobStatus
{
    public class GetChatJobStatusQueryHandler(
        ICurrentUserService currentUserService,
        IChatAsyncJobService chatAsyncJobService)
        : IRequestHandler<GetChatJobStatusQuery, ApiResponse<ChatAsyncJobStatusDto>>
    {
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IChatAsyncJobService _chatAsyncJobService = chatAsyncJobService;

        public async Task<ApiResponse<ChatAsyncJobStatusDto>> Handle(
            GetChatJobStatusQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthenticatedException("Bạn cần đăng nhập để kiểm tra trạng thái xử lý chat.");

            var jobState = await _chatAsyncJobService.GetStatusAsync(request.JobId, cancellationToken)
                ?? throw new NotFoundException("Async chat job không tồn tại hoặc đã hết hạn.");

            if (jobState.UserId != userId)
            {
                throw new AccessDeniedException("Bạn không có quyền truy cập job chat này.");
            }

            var dto = new ChatAsyncJobStatusDto
            {
                Success = jobState.Success,
                Accepted = jobState.Accepted,
                JobId = jobState.JobId,
                Status = jobState.Status,
                Result = jobState.Result,
                Error = jobState.Error
            };

            return ApiResponse<ChatAsyncJobStatusDto>.Success(dto, "Lấy trạng thái xử lý chat thành công.");
        }
    }
}
