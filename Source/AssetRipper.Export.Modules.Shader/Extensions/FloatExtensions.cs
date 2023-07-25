namespace AssetRipper.Export.Modules.Shaders.Extensions;

internal static class FloatExtensions
{
	public static string ToStringInvariant(this float value) => value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
