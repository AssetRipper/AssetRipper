using System.Text.RegularExpressions;

namespace AssetRipper.Processing;

public static partial class GameObjectNameCleaner
{
	[GeneratedRegex("\\([0-9]+\\)$", RegexOptions.Compiled)]
	private static partial Regex CopySuffixRegex();

	public static string CleanName(string name)
	{
		string noClones = name.Replace("(Clone)", "", StringComparison.Ordinal);
		return CopySuffixRegex().Replace(noClones, "").Trim();
	}
}
