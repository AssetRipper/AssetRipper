namespace AssetRipper.Assets;

internal static class StringPathExtensions
{
	[return: NotNullIfNotNull(nameof(str))]
	public static string? RemovePeriod(this string? str)
	{
		return string.IsNullOrEmpty(str) || str[0] != '.' ? str : str.Substring(1);
	}

	public static string? NotEmpty(this string? str)
	{
		return string.IsNullOrEmpty(str) ? null : str;
	}
}
