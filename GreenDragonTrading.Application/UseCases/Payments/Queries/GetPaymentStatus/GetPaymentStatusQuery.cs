using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Payments.Queries.GetPaymentStatus
{
    public record GetPaymentStatusQuery(long OrderCode) : IRequest<ApiResponse<PaymentStatusResponse>>;
}
