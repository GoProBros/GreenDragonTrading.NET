using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Users.Commands.UpdateMyProfile;

/// <summary>
/// Command to update current user profile information.
/// Only full name and avatar are allowed.
/// </summary>
public record UpdateMyProfileCommand(
    string FullName,
    string? AvatarUrl
) : IRequest<ApiResponse>;
