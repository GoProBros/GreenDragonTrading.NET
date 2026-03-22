using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.Users.Commands.CreateStaffUser;
using GreenDragonTrading.Application.UseCases.Users.Queries.GetUserDetail;
using GreenDragonTrading.Application.UseCases.Users.Queries.GetUsers;
using GreenDragonTrading.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenDragonTrading.Api.Controllers;

[Route("api/v1/users")]
[ApiController]
[Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Staff)}")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get users list for management.
    /// Admin can get all users. Staff can get only end users.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<UserManagementListItemDto>>>> GetUsers(
        [FromQuery] GetUsersQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get detail of a user by id for management.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserManagementDetailDto>>> GetUserDetail(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserDetailQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new staff account (Admin only).
    /// Set RequireEmailVerification to false to skip email verification step.
    /// </summary>
    [HttpPost("staff")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<ApiResponse>> CreateStaffUser(
        [FromBody] CreateStaffUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateStaffUserCommand(
            request.Email,
            request.Password,
            request.FullName,
            request.PhoneNumber,
            request.AvatarUrl,
            request.RequireEmailVerification);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}