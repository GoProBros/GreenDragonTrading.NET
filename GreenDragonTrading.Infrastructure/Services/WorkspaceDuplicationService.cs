using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Entities;
using GreenDragonTrading.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text.Json;

namespace GreenDragonTrading.Infrastructure.Services;

/// <summary>
/// Service for duplicating workspaces and their associated module layouts
/// </summary>
public class WorkspaceDuplicationService : IWorkspaceDuplicationService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<WorkspaceDuplicationService> _logger;

    public WorkspaceDuplicationService(
        IUnitOfWork uow,
        ILogger<WorkspaceDuplicationService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Workspace> DuplicateWorkspaceAsync(
        Workspace sourceWorkspace,
        Guid targetUserId,
        string workspaceNameSuffix = "(Sao chép)",
        CancellationToken cancellationToken = default)
    {
        var newLayoutJson = await DuplicateLayoutsAndUpdateReferencesAsync(
            sourceWorkspace.LayoutJson,
            targetUserId,
            cancellationToken);

        var newShareCode = await GenerateUniqueShareCodeAsync(cancellationToken);

        var newWorkspace = new Workspace
        {
            UserId = targetUserId,
            WorkspaceName = $"{sourceWorkspace.WorkspaceName} {workspaceNameSuffix}".Trim(),
            LayoutJson = newLayoutJson,
            IsDefault = false,
            ShareCode = newShareCode,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _uow.Workspaces.AddAsync(newWorkspace, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Workspace duplicated successfully. Source workspace ID: {SourceId}, New workspace ID: {NewId} for user: {UserId}",
            sourceWorkspace.Id,
            newWorkspace.Id,
            targetUserId);

        return newWorkspace;
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
