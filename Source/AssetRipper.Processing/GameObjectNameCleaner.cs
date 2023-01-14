using System.Text.RegularExpressions;

namespace AssetRipper.Processing;

internal static class GameObjectNameCleaner
{
	private static readonly Regex copySuffixRegex = new Regex(@"\([0-9]+\)$", RegexOptions.Compiled);

	public static string CleanName(string name)
	{
		string noClones = name.Replace("(Clone)", "", StringComparison.Ordinal);
		return copySuffixRegex.Replace(noClones, "").Trim();
	}
}