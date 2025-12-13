using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.Auth.Commands.Login;
using GreenDragonTrading.Application.UseCases.Auth.Commands.Logout;
using GreenDragonTrading.Application.UseCases.Auth.Commands.RefreshToken;
using GreenDragonTrading.Application.UseCases.Auth.Commands.Register;
using GreenDragonTrading.Application.UseCases.Auth.Commands.VerifyEmail;
using GreenDragonTrading.Application.UseCases.Auth.Queries.GetMe;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Register a new user account
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse>> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result;
        }

        /// <summary>
        /// Verify email with verification token
        /// </summary>
        [HttpGet("verify-email")]
        public async Task<ActionResult<ApiResponse>> VerifyEmail([FromQuery] VerifyEmailCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result;
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result;
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result;
        }

        /// <summary>
        /// Logout and invalidate both refresh token and access token
        /// </summary>
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse>> Logout([FromBody] LogoutCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return result;
        }

        /// <summary>
        /// Get current user information from access token
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetMe(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetMeQuery(), cancellationToken);
            return result;
        }
    }
}
