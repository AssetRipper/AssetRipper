using AssetRipper.Core.Classes.Font;
using AssetRipper.SourceGenerated.Classes.ClassID_128;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class FontExtensions
	{
		public static FontStyle GetDefaultStyle(this IFont font)
		{
			return (FontStyle)font.DefaultStyle_C128;
		}

		public static FontRenderingMode GetFontRenderingMode(this IFont font)
		{
			return (FontRenderingMode)font.FontRenderingMode_C128;
		}
	}
}
