using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Domain.Entities;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Auth.Commands.Register
{
    public record RegisterCommand(
        string Email,
        string Password,
        string FullName,
        string PhoneNumber
    ) : IRequest<ApiResponse>;
}
