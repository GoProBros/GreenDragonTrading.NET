using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.Auth.Commands.Login;
using GreenDragonTrading.Application.UseCases.Auth.Commands.Logout;
using GreenDragonTrading.Application.UseCases.Auth.Commands.RefreshToken;
using GreenDragonTrading.Application.UseCases.Auth.Commands.Register;
using GreenDragonTrading.Application.UseCases.Auth.Commands.VerifyEmail;
using MediatR;
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
        public async Task<ActionResult<ApiResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            var command = new RegisterCommand(request.Email, request.Password, request.FullName, request.PhoneNumber);
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(ApiResponse.FailResponse(result.Message));
            }

            return Ok(ApiResponse.SuccessResponse(result.Message));
        }

        /// <summary>
        /// Verify email with verification token
        /// </summary>
        [HttpGet("verify-email")]
        public async Task<ActionResult<ApiResponse>> VerifyEmail([FromQuery] string token, CancellationToken cancellationToken)
        {
            var command = new VerifyEmailCommand(token);
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(ApiResponse.FailResponse(result.Message));
            }

            return Ok(ApiResponse.SuccessResponse(result.Message));
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var command = new LoginCommand(request.Email, request.Password);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(result.Data!, result.Message));
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var command = new RefreshTokenCommand(request.RefreshToken, request.UserId);
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(ApiResponse<AuthResponse>.FailResponse(result.Message));
            }

            return Ok(ApiResponse<AuthResponse>.SuccessResponse(result.Data!, result.Message));
        }

        /// <summary>
        /// Logout and invalidate refresh token
        /// </summary>
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse>> Logout([FromBody] string refreshToken, CancellationToken cancellationToken)
        {
            var command = new LogoutCommand(refreshToken);
            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(ApiResponse.FailResponse(result.Message));
            }

            return Ok(ApiResponse.SuccessResponse(result.Message));
        }
    }
}
