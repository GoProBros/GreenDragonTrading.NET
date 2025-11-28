using System.Security.Claims;
using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs.Auth;
using GreenDragonTrading.Application.UseCases.Auth.Commands.Login;
using GreenDragonTrading.Application.UseCases.Auth.Commands.Logout;
using GreenDragonTrading.Application.UseCases.Auth.Commands.RefreshToken;
using GreenDragonTrading.Application.UseCases.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand(request.Username, request.Email, request.Password);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Registration successful"));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Login successful"));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse>> Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse.FailResponse("Unauthorized", "Invalid user token"));
        }

        var command = new LogoutCommand(userId);
        await _mediator.Send(command);

        return Ok(ApiResponse.SuccessResponse("Logged out successfully"));
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var authHeader = Request.Headers.Authorization.ToString();
        var accessToken = authHeader.Replace("Bearer ", "");

        if (string.IsNullOrEmpty(accessToken))
        {
            return BadRequest(ApiResponse<AuthResponse>.FailResponse("Bad request", "Access token is required in Authorization header"));
        }

        var command = new RefreshTokenCommand(accessToken, request.RefreshToken);
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Token refreshed successfully"));
    }
}
