using GreenDragonTrading.Application.Common.Models;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.ModuleLayouts.Commands.DeleteLayout;

public record DeleteLayoutCommand(long Id) : IRequest<ApiResponse>;
