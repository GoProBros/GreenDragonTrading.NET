using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.Chat.Commands.CreateChatSession;
using GreenDragonTrading.Application.UseCases.Chat.Commands.SendChatMessage;
using GreenDragonTrading.Application.UseCases.Chat.Commands.SummarizeSession;
using GreenDragonTrading.Application.UseCases.Chat.Queries.GetChatMessages;
using GreenDragonTrading.Application.UseCases.Chat.Queries.GetChatSessions;
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
        /// <returns>User message and AI response with intent classification</returns>
        [HttpPost("sessions/{sessionId:int}/messages")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<SendChatMessageResponseDto>>> SendChatMessage(
            [FromRoute] int sessionId,
            [FromBody] SendChatMessageRequestDto request,
            CancellationToken cancellationToken)
        {
            var command = new SendChatMessageCommand(sessionId, request.Message);
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
    }
}
