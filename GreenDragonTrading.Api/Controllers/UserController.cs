using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.UseCases.Users.Commands.ActivateUser;
using GreenDragonTrading.Application.UseCases.Users.Commands.CreateStaffUser;
using GreenDragonTrading.Application.UseCases.Users.Commands.DeactivateUser;
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
    /// Staff can see only User role.
    /// Admin can see Staff and User roles.
    /// Supports optional role filter and keyword search by name/email/phone.
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
    /// Update user account status.
    /// Admin can update Staff/User. Staff can update User only.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse>> UpdateUserStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        ApiResponse result;
        if (request.Status == CommonStatus.Active)
        {
            result = await _mediator.Send(new ActivateUserCommand(id), cancellationToken);
        }
        else if (request.Status == CommonStatus.InActive)
        {
            result = await _mediator.Send(new DeactivateUserCommand(id), cancellationToken);
        }
        else
        {
            return BadRequest(ApiResponse.Failure("Status không hợp lệ. Chỉ hỗ trợ Active hoặc InActive."));
        }

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