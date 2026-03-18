namespace AssetRipper.Processing;

internal static class OriginalPathHelper
{
	private const string DirectorySeparator = "/";
	private const string AssetsDirectory = AssetsKeyword + DirectorySeparator;
	private const string AssetsKeyword = "Assets";

	internal static string EnsurePathNotRooted(string assetPath)
	{
		if (Path.IsPathRooted(assetPath))
		{
			string[] splitPath = assetPath.Split('/');
			for (int i = 0; i < splitPath.Length; i++)
			{
				string pathSection = splitPath[i];
				if (string.Equals(pathSection, AssetsKeyword, StringComparison.OrdinalIgnoreCase))
				{
					return string.Join(DirectorySeparator, new ReadOnlySpan<string?>(splitPath, i, splitPath.Length - i));
				}
			}
			return string.Empty;
		}
		else
		{
			return assetPath;
		}
	}

	internal static string EnsureStartsWithAssets(string assetPath)
	{
		if (assetPath.StartsWith(AssetsDirectory, StringComparison.Ordinal))
		{
			return assetPath;
		}
		else if (assetPath.StartsWith(AssetsDirectory, StringComparison.OrdinalIgnoreCase))
		{
			return $"{AssetsDirectory}{assetPath.AsSpan(AssetsDirectory.Length)}";
		}
		else
		{
			return AssetsDirectory + assetPath;
		}
	}
}
