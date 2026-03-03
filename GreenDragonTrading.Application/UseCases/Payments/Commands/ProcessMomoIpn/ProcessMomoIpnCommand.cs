using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Payments.Commands.ProcessMomoIpn
{
    public record ProcessMomoIpnCommand(MomoIpnRequest IpnRequest) : IRequest<ApiResponse>;
}
