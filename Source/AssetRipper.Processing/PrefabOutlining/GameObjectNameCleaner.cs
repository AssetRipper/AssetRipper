using System;
using System.Text.RegularExpressions;

namespace AssetRipper.Processing.PrefabOutlining;

public static partial class GameObjectNameCleaner
{
	[GeneratedRegex(@"\s*\(Clone\)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
	private static partial Regex CloneSuffixRegex();

	// Removes one or more trailing copy suffixes like " (1)", "(2)", " (3) (4)".
	[GeneratedRegex(@"(?:\s*\(\d+\))+$", RegexOptions.Compiled)]
	private static partial Regex CopySuffixRegex();

	public static string CleanName(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return "GameObject";
		}

		string cleanedName = name.Trim();
		string previousName;
		do
		{
			previousName = cleanedName;
			cleanedName = CopySuffixRegex().Replace(cleanedName, "");
			cleanedName = CloneSuffixRegex().Replace(cleanedName, "");
			cleanedName = cleanedName.Trim();
		}
		while (cleanedName.Length > 0 && !string.Equals(cleanedName, previousName, StringComparison.Ordinal));

		return cleanedName.Length == 0 ? "GameObject" : cleanedName;
	}
}
