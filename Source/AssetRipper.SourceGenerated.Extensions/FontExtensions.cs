using AssetRipper.SourceGenerated.Classes.ClassID_128;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Classes.ClassID_27;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class FontExtensions
	{
		public static FontStyle GetDefaultStyle(this IFont font)
		{
			return (FontStyle)font.DefaultStyle;
		}

		public static FontRenderingMode GetFontRenderingMode(this IFont font)
		{
			return (FontRenderingMode)font.FontRenderingMode;
		}

		/// <summary>
		/// Font Material is an automatically generated material for each font.
		/// </summary>
		/// <param name="font"></param>
		/// <param name="fontMaterial"></param>
		/// <returns></returns>
		public static bool TryGetFontMaterial(this IFont font, [NotNullWhen(true)] out IMaterial? fontMaterial)
		{
			if (font.DefaultMaterialP is { Name.String: "Font Material" } material)
			{
				fontMaterial = material;
				return true;
			}
			else
			{
				fontMaterial = null;
				return false;
			}
		}

		/// <summary>
		/// Font Texture is an automatically generated texture for each font.
		/// </summary>
		/// <param name="font"></param>
		/// <param name="fontTexture"></param>
		/// <returns></returns>
		public static bool TryGetFontTexture(this IFont font, [NotNullWhen(true)] out ITexture? fontTexture)
		{
			if (font.TextureP is { Name.String: "Font Texture" } texture)
			{
				fontTexture = texture;
				return true;
			}
			else
			{
				fontTexture = null;
				return false;
			}
		}
	}
}
