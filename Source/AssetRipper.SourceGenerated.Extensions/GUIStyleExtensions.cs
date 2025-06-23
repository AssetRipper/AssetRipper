using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.GUIStyle;

namespace AssetRipper.SourceGenerated.Extensions;

public static class GUIStyleExtensions
{
	public static ImagePosition GetImagePosition(this IGUIStyle style)
	{
		return (ImagePosition)style.ImagePosition;
	}

	public static TextAnchor GetAlignment(this IGUIStyle style)
	{
		return (TextAnchor)style.Alignment;
	}

	public static FontStyle GetFontStyle(this IGUIStyle style)
	{
		return (FontStyle)style.FontStyle;
	}
}
