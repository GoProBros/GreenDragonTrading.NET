using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.Chat.Commands.CreateChatSession;
using GreenDragonTrading.Application.UseCases.Chat.Commands.GetOrCreateDirectSession;
using GreenDragonTrading.Application.UseCases.Chat.Commands.MarkSessionAsRead;
using GreenDragonTrading.Application.UseCases.Chat.Commands.SendChatMessage;
using GreenDragonTrading.Application.UseCases.Chat.Commands.SendDirectMessage;
using GreenDragonTrading.Application.UseCases.Chat.Commands.SendSystemNotification;
using GreenDragonTrading.Application.UseCases.Chat.Commands.SummarizeSession;
using GreenDragonTrading.Application.UseCases.Chat.Queries.GetChatJobStatus;
using GreenDragonTrading.Application.UseCases.Chat.Queries.GetChatMessages;
using GreenDragonTrading.Application.UseCases.Chat.Queries.GetChatSessions;
using GreenDragonTrading.Application.UseCases.Chat.Queries.GetDirectChatSessions;
using GreenDragonTrading.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers
{
    /// <summary>
    /// Chat management endpoints for AI conversations
    /// </summary>
    [Route("api/v1/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ChatController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all chat sessions for the current user
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>List of chat sessions with participant count</returns>
        [HttpGet("sessions")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<ChatSessionListItemDto>>>> GetChatSessions(
            CancellationToken cancellationToken)
        {
            var query = new GetChatSessionsQuery();
            var result = await _mediator.Send(query, cancellationToken);
            return result;
        }

        /// <summary>
        /// Create a new AI chat session
        /// </summary>
        /// <param name="request">Optional title for the chat session</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The newly created chat session</returns>
        [HttpPost("sessions")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ChatSessionDto>>> CreateChatSession(
            [FromBody] CreateChatSessionRequestDto? request,
            CancellationToken cancellationToken)
        {
            var command = new CreateChatSessionCommand(request?.Title);
            var result = await _mediator.Send(command, cancellationToken);
            return result;
        }

        /// <summary>
        /// Get all messages from a chat session
        /// </summary>
        /// <param name="sessionId">The chat session ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Chat session with all messages</returns>
        [HttpGet("sessions/{sessionId:int}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ChatSessionDetailDto>>> GetChatMessages(
            [FromRoute] int sessionId,
            CancellationToken cancellationToken)
        {
            var query = new GetChatMessagesQuery(sessionId);
            var result = await _mediator.Send(query, cancellationToken);
            return result;
        }

        /// <summary>
        /// Send a message in an AI chat session. Saves user message, calls AI engine, saves AI response.
        /// </summary>
        /// <param name="sessionId">The chat session ID</param>
        /// <param name="request">Message content</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Completed result (200) or accepted async job metadata (202)</returns>
        [HttpPost("sessions/{sessionId:int}/messages")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<SendChatMessageResultDto>>> SendChatMessage(
            [FromRoute] int sessionId,
            [FromBody] SendChatMessageRequestDto request,
            CancellationToken cancellationToken)
        {
            var command = new SendChatMessageCommand(sessionId, request.Message);
            var result = await _mediator.Send(command, cancellationToken);

            if (result.Data?.Accepted == true)
            {
                return StatusCode(StatusCodes.Status202Accepted, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Poll async AI chat job status.
        /// </summary>
        /// <param name="jobId">The async chat job identifier</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Job status (202 when queued/running, 200 when completed/failed)</returns>
        [HttpGet("jobs/{jobId}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ChatAsyncJobStatusDto>>> GetChatJobStatus(
            [FromRoute] string jobId,
            CancellationToken cancellationToken)
        {
            var query = new GetChatJobStatusQuery(jobId);
            var result = await _mediator.Send(query, cancellationToken);

            if (string.Equals(result.Data?.Status, "queued", StringComparison.OrdinalIgnoreCase)
                || string.Equals(result.Data?.Status, "running", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status202Accepted, result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Mark chat session as read for current user by updating LastReadAt.
        /// </summary>
        /// <param name="sessionId">The chat session ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Operation status</returns>
        [HttpPatch("sessions/{sessionId:int}/read")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> MarkSessionAsRead(
            [FromRoute] int sessionId,
            CancellationToken cancellationToken)
        {
            var command = new MarkSessionAsReadCommand(sessionId);
            var result = await _mediator.Send(command, cancellationToken);
            return result;
        }

        /// <summary>
        /// Trigger a conversation summary update for a chat session.
        /// Summarizes all messages if no summary exists, otherwise only processes new messages since the last summary.
        /// </summary>
        /// <param name="sessionId">The chat session ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Updated summary and the number of messages processed</returns>
        [HttpPost("sessions/{sessionId:int}/summarize")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<SummarizeSessionResponseDto>>> SummarizeSession(
            [FromRoute] int sessionId,
            CancellationToken cancellationToken)
        {
            var command = new SummarizeSessionCommand(sessionId);
            var result = await _mediator.Send(command, cancellationToken);
            return result;
        }

        /// <summary>
        /// Get or create a Direct (1-1) chat session with another user identified by phone or email.
        /// Returns the existing session if one already exists between the two users.
        /// </summary>
        /// <param name="request">Phone number or email of the target user</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Session info and the other participant's details</returns>
        [HttpPost("direct")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<DirectChatSessionDto>>> GetOrCreateDirectSession(
            [FromBody] GetOrCreateDirectSessionRequestDto request,
            CancellationToken cancellationToken)
        {
            var command = new GetOrCreateDirectSessionCommand(request.PhoneOrEmail);
            var result = await _mediator.Send(command, cancellationToken);
            return result;
        }

        /// <summary>
        /// Get all Direct (1-1) chat sessions for the current user, ordered by most recent activity.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>List of Direct sessions with last message preview and unread indicator</returns>
        [HttpGet("direct")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<DirectSessionListItemDto>>>> GetDirectChatSessions(
            CancellationToken cancellationToken)
        {
            var query = new GetDirectChatSessionsQuery();
            var result = await _mediator.Send(query, cancellationToken);
            return result;
        }

        /// <summary>
        /// Send a message in a Direct (1-1) chat session.
        /// </summary>
        /// <param name="sessionId">The Direct chat session ID</param>
        /// <param name="request">Message content</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The newly created message</returns>
        [HttpPost("sessions/{sessionId:int}/direct-messages")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<DirectMessageDto>>> SendDirectMessage(
            [FromRoute] int sessionId,
            [FromBody] SendDirectMessageRequestDto request,
            CancellationToken cancellationToken)
        {
            var command = new SendDirectMessageCommand(sessionId, request.Content);
            var result = await _mediator.Send(command, cancellationToken);
            return result;
        }

        /// <summary>
        /// Sends a system notification to a user. Each user has exactly one System chat session.
        /// If the session does not exist yet, it is created automatically.
        /// </summary>
        /// <param name="request">Target user and notification message</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Notification message metadata with system session id</returns>
        [HttpPost("system-notifications")]
        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Staff)}")]
        public async Task<ActionResult<ApiResponse<SendSystemNotificationResponseDto>>> SendSystemNotification(
            [FromBody] SendSystemNotificationRequestDto request,
            CancellationToken cancellationToken)
        {
            var command = new SendSystemNotificationCommand(request.UserIds, request.SendToAll, request.Message);
            var result = await _mediator.Send(command, cancellationToken);
            return result;
        }
    }
}
