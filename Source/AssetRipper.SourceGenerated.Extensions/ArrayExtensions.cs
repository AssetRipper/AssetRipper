namespace AssetRipper.SourceGenerated.Extensions;

public static class ArrayExtensions
{
	public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this T[]? array) => array is null || array.Length == 0;
}
