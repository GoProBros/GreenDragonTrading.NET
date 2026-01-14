using GreenDragonTrading.Application.Common.Models;
using GreenDragonTrading.Application.DTOs;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Exceptions;
using GreenDragonTrading.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text.Json;

namespace GreenDragonTrading.Application.UseCases.Workspace.Commands.ApplySharedWorkspace
{
    public class ApplySharedWorkspaceCommandHandler : IRequestHandler<ApplySharedWorkspaceCommand, ApiResponse<WorkspaceDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJwtService _jwtService;
        private readonly ILogger<ApplySharedWorkspaceCommandHandler> _logger;

        public ApplySharedWorkspaceCommandHandler(
            IUnitOfWork uow,
            IHttpContextAccessor httpContextAccessor,
            IJwtService jwtService,
            ILogger<ApplySharedWorkspaceCommandHandler> logger)
        {
            _uow = uow;
            _httpContextAccessor = httpContextAccessor;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<ApiResponse<WorkspaceDto>> Handle(ApplySharedWorkspaceCommand request, CancellationToken cancellationToken)
        {
            var userId = GetAuthenticatedUserId();

            var sharedWorkspace = await GetSharedWorkspaceAsync(request.SharedWorkspaceCode, cancellationToken);

            ValidateNotOwnWorkspace(sharedWorkspace, userId);

            var newLayoutJson = await DuplicateLayoutsAndUpdateReferencesAsync(
                sharedWorkspace.LayoutJson,
                userId,
                cancellationToken);

            var newWorkspace = await CreateNewWorkspaceAsync(sharedWorkspace, userId, newLayoutJson, cancellationToken);

            var workspaceDto = MapToDto(newWorkspace);

            _logger.LogInformation(
                "Workspace applied successfully from share code {ShareCode}. New workspace ID: {WorkspaceId} for user: {UserId}",
                request.SharedWorkspaceCode,
                newWorkspace.Id,
                userId);

            return ApiResponse<WorkspaceDto>.Success(workspaceDto, "Áp dụng workspace thành công.");
        }

        private Guid GetAuthenticatedUserId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var authHeader = httpContext?.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                _logger.LogWarning("No valid Authorization header found in the request.");
                throw new UnauthenticatedException("Người dùng không xác thực.");
            }

            var token = authHeader["Bearer ".Length..].Trim();
            var tokenInfo = _jwtService.GetTokenInfo(token);

            if (tokenInfo?.UserId == null)
            {
                _logger.LogWarning("User ID could not be determined from the token.");
                throw new UnauthenticatedException("Không thể xác định người dùng.");
            }

            return tokenInfo.UserId;
        }

        private async Task<Domain.Entities.Workspace> GetSharedWorkspaceAsync(string shareCode, CancellationToken cancellationToken)
        {
            var sharedWorkspace = await _uow.Workspaces.GetByShareCodeAsync(shareCode, cancellationToken);

            if (sharedWorkspace == null)
            {
                _logger.LogWarning("Shared workspace not found for code: {ShareCode}", shareCode);
                throw new NotFoundException("Workspace chia sẻ không tồn tại.");
            }

            return sharedWorkspace;
        }

        private void ValidateNotOwnWorkspace(Domain.Entities.Workspace sharedWorkspace, Guid userId)
        {
            if (sharedWorkspace.UserId == userId)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to apply their own workspace share code: {ShareCode}",
                    userId,
                    sharedWorkspace.ShareCode);
                throw new BusinessRuleException("Bạn không thể áp dụng mã chia sẻ của chính mình.");
            }
        }

        private async Task<Domain.Entities.Workspace> CreateNewWorkspaceAsync(
            Domain.Entities.Workspace sharedWorkspace,
            Guid userId,
            string newLayoutJson,
            CancellationToken cancellationToken)
        {
            var newShareCode = await GenerateUniqueShareCodeAsync(cancellationToken);

            var newWorkspace = new Domain.Entities.Workspace
            {
                UserId = userId,
                WorkspaceName = $"{sharedWorkspace.WorkspaceName} (Sao chép)",
                LayoutJson = newLayoutJson,
                IsDefault = false,
                ShareCode = newShareCode,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _uow.Workspaces.AddAsync(newWorkspace, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return newWorkspace;
        }

        private static WorkspaceDto MapToDto(Domain.Entities.Workspace workspace)
        {
            return new WorkspaceDto
            {
                Id = workspace.Id,
                WorkspaceName = workspace.WorkspaceName,
                LayoutJson = JsonSerializer.Deserialize<JsonElement>(workspace.LayoutJson),
                IsDefault = workspace.IsDefault,
                ShareCode = workspace.ShareCode
            };
        }

        private async Task<string> DuplicateLayoutsAndUpdateReferencesAsync(
            string layoutJson,
            Guid userId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(layoutJson) || layoutJson == "{}")
            {
                return "{}";
            }

            var parseResult = TryParseLayoutJson(layoutJson);
            if (parseResult == null)
            {
                return layoutJson;
            }

            var (layoutObject, modulesArray) = parseResult.Value;

            var layoutIdsToDuplicate = CollectLayoutIdsToDuplicate(modulesArray);
            var layoutIdMapping = await DuplicateLayoutsAsync(layoutIdsToDuplicate, userId, cancellationToken);

            UpdateModuleLayoutReferences(modulesArray, layoutIdMapping);

            layoutObject["modules"] = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(modulesArray));

            return JsonSerializer.Serialize(layoutObject);
        }

        private static (Dictionary<string, JsonElement> layoutObject, List<Dictionary<string, JsonElement>> modulesArray)? TryParseLayoutJson(string layoutJson)
        {
            JsonElement rootElement;
            try
            {
                rootElement = JsonSerializer.Deserialize<JsonElement>(layoutJson);
            }
            catch
            {
                return null;
            }

            if (!rootElement.TryGetProperty("modules", out var modulesElement) ||
                modulesElement.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            var layoutObject = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(layoutJson)
                ?? [];

            var modulesArray = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(modulesElement.GetRawText())
                ?? [];

            return (layoutObject, modulesArray);
        }

        private static HashSet<long> CollectLayoutIdsToDuplicate(List<Dictionary<string, JsonElement>> modulesArray)
        {
            var layoutIdsToDuplicate = new HashSet<long>();

            foreach (var module in modulesArray)
            {
                var layoutId = TryGetActiveLayoutId(module);
                if (layoutId.HasValue)
                {
                    layoutIdsToDuplicate.Add(layoutId.Value);
                }
            }

            return layoutIdsToDuplicate;
        }

        private static long? TryGetActiveLayoutId(Dictionary<string, JsonElement> module)
        {
            if (!module.TryGetValue("activeLayoutId", out var activeLayoutIdElement))
            {
                return null;
            }

            if (activeLayoutIdElement.ValueKind != JsonValueKind.Number)
            {
                return null;
            }

            if (!activeLayoutIdElement.TryGetInt64(out var activeLayoutId) || activeLayoutId <= 0)
            {
                return null;
            }

            return activeLayoutId;
        }

        private async Task<Dictionary<long, long>> DuplicateLayoutsAsync(
            HashSet<long> layoutIdsToDuplicate,
            Guid userId,
            CancellationToken cancellationToken)
        {
            var layoutIdMapping = new Dictionary<long, long>();

            foreach (var originalLayoutId in layoutIdsToDuplicate)
            {
                var newLayoutId = await DuplicateSingleLayoutAsync(originalLayoutId, userId, cancellationToken);
                if (newLayoutId.HasValue)
                {
                    layoutIdMapping[originalLayoutId] = newLayoutId.Value;
                }
            }

            return layoutIdMapping;
        }

        private async Task<long?> DuplicateSingleLayoutAsync(
            long originalLayoutId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            var originalLayout = await _uow.ModuleLayouts.GetByIdAsync(originalLayoutId, cancellationToken);
            if (originalLayout == null)
            {
                return null;
            }

            var duplicatedLayout = new ModuleLayout
            {
                UserId = userId,
                ModuleType = originalLayout.ModuleType,
                LayoutName = $"{originalLayout.LayoutName} (Sao chép)",
                ConfigJson = originalLayout.ConfigJson,
                IsSystemDefault = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            await _uow.ModuleLayouts.AddAsync(duplicatedLayout, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogDebug(
                "Duplicated layout {OriginalLayoutId} -> {NewLayoutId} for user {UserId}",
                originalLayoutId,
                duplicatedLayout.Id,
                userId);

            return duplicatedLayout.Id;
        }

        private static void UpdateModuleLayoutReferences(
            List<Dictionary<string, JsonElement>> modulesArray,
            Dictionary<long, long> layoutIdMapping)
        {
            foreach (var module in modulesArray)
            {
                var originalId = TryGetActiveLayoutId(module);
                if (originalId.HasValue && layoutIdMapping.TryGetValue(originalId.Value, out var newId))
                {
                    module["activeLayoutId"] = JsonSerializer.Deserialize<JsonElement>(newId.ToString());
                }
            }
        }

        private async Task<string> GenerateUniqueShareCodeAsync(CancellationToken cancellationToken)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            const int codeLength = 8;
            const int maxAttempts = 10;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var shareCode = GenerateRandomCode(chars, codeLength);
                var exists = await _uow.Workspaces.ShareCodeExistsAsync(shareCode, cancellationToken);

                if (!exists)
                {
                    return shareCode;
                }
            }

            throw new InvalidOperationException("Hệ thống hiện không thể tạo mã chia sẻ duy nhất. Vui lòng thử lại.");
        }

        private static string GenerateRandomCode(string chars, int length)
        {
            var codeBuffer = new char[length];
            for (int i = 0; i < length; i++)
            {
                codeBuffer[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
            }
            return new string(codeBuffer);
        }
    }
}