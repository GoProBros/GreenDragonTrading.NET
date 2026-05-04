using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Users.Commands.CreateStaffUser;

/// <summary>
/// Command to create a new staff account.
/// </summary>
public record CreateStaffUserCommand(
    string Email,
    string Password,
    string FullName,
    string PhoneNumber,
    string? AvatarUrl,
    bool RequireEmailVerification = true
) : IRequest<ApiResponse>;