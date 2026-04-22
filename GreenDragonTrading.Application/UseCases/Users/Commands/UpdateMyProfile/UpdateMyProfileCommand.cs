using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Users.Commands.UpdateMyProfile;

/// <summary>
/// Command to update current user profile information.
/// Allows updating full name, phone number, and avatar.
/// </summary>
public record UpdateMyProfileCommand(
    string? FullName,
    string? PhoneNumber,
    string? AvatarUrl
) : IRequest<ApiResponse>;
