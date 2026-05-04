using System.Text.Json;
using System.Text.Json.Nodes;

namespace GreenDragonTrading.Application.Common.Utils;

public static class WorkspaceLayoutLockingHelper
{
    public static List<string> ParseAllowedModuleKeys(string? allowedModulesJson)
    {
        if (string.IsNullOrWhiteSpace(allowedModulesJson))
        {
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(allowedModulesJson);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            var keys = new List<string>();

            foreach (var element in document.RootElement.EnumerateArray())
            {
                if (element.ValueKind == JsonValueKind.String)
                {
                    var value = element.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        keys.Add(value.Trim());
                    }

                    continue;
                }

                if (element.ValueKind == JsonValueKind.Number)
                {
                    keys.Add(element.GetRawText());
                }
            }

            return keys.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
        catch
        {
            return [];
        }
    }

    public static JsonElement? ApplyModuleLocks(
        JsonElement? layoutJson,
        IReadOnlyCollection<string> allowedModules,
        bool allowAll)
    {
        if (!layoutJson.HasValue)
        {
            return null;
        }

        var allowedSet = new HashSet<string>(allowedModules, StringComparer.OrdinalIgnoreCase);

        JsonNode? rootNode;
        try
        {
            rootNode = JsonNode.Parse(layoutJson.Value.GetRawText());
        }
        catch
        {
            return layoutJson;
        }

        if (rootNode is not JsonObject rootObject)
        {
            return layoutJson;
        }

        if (rootObject["modules"] is not JsonArray modules)
        {
            return layoutJson;
        }

        foreach (var moduleNode in modules.OfType<JsonObject>())
        {
            var typeValue = moduleNode["type"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(typeValue))
            {
                continue;
            }

            var isLocked = !allowAll && (allowedSet.Count == 0 || !allowedSet.Contains(typeValue));
            moduleNode["isLocked"] = isLocked;
        }

        return JsonSerializer.Deserialize<JsonElement>(rootObject.ToJsonString());
    }
}
