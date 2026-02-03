using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Domain.Enums;
using MediatR;

namespace GreenDragonTrading.Application.UseCases.Files.Commands.DeleteFile;

/// <summary>
/// Command to delete a file by category and entity ID
/// </summary>
public record DeleteFileCommand(
    FileCategory Category,
    string EntityId
) : IRequest<ApiResponse>;
