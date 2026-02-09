using AssetRipper.SourceGenerated.Subclasses.ColorRGBAf;

namespace AssetRipper.SourceGenerated.Extensions;

public static class ColorRGBAfExtensions
{
	public static void SetAsBlack(this IColorRGBAf color) => color.SetValues(0f, 0f, 0f, 1f);

	public static void SetAsWhite(this IColorRGBAf color) => color.SetValues(1f, 1f, 1f, 1f);
}
