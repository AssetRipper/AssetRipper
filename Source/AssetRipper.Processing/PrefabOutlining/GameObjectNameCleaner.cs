using System.Text.RegularExpressions;
using System;

namespace AssetRipper.Processing.PrefabOutlining;

public static partial class GameObjectNameCleaner
{
    // Regex to find and remove suffixes like " (1)", " (2)", etc.
    [GeneratedRegex("\\([0-9]+\\)$", RegexOptions.Compiled)]
    private static partial Regex CopySuffixRegex();

    public static string CleanName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "GameObject";

        // Remove (Clone) suffix
        string noClones = name.Replace("(Clone)", "", StringComparison.Ordinal);
        
        // Remove number suffixes like (1) and trim whitespace
        return CopySuffixRegex().Replace(noClones, "").Trim();
    }
}
