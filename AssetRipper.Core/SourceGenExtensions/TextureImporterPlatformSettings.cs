using AssetRipper.Core.Classes.Meta.Importers.Texture;
using AssetRipper.SourceGenerated.Subclasses.TextureImporterPlatformSettings;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class TextureImporterPlatformSettings
	{
		public static TextureResizeAlgorithm GetResizeAlgorithm(this ITextureImporterPlatformSettings settings)
		{
			return (TextureResizeAlgorithm)settings.ResizeAlgorithm;
		}

		public static SourceGenerated.Enums.TextureFormat GetTextureFormat(this ITextureImporterPlatformSettings settings)
		{
			/*if (settings.ToSerializedVersion() > 1)// TextureFormat.ATC_RGB4/ATC_RGBA8 has been replaced by ETC_RGB4/ETC2_RGBA8
			{
				if (settings.TextureFormat == TextureFormat.ATC_RGB4)
				{
					return TextureFormat.ETC_RGB4;
				}
				if (settings.TextureFormat == TextureFormat.ATC_RGBA8)
				{
					return TextureFormat.ETC2_RGBA8;
				}
			}*/
			return (SourceGenerated.Enums.TextureFormat)settings.Format;
		}

		public static TextureImporterCompression GetTextureCompression(this ITextureImporterPlatformSettings settings)
		{
			return (TextureImporterCompression)settings.TextureCompression;
		}

		public static AndroidETC2FallbackOverride GetAndroidETC2FallbackOverride(this ITextureImporterPlatformSettings settings)
		{
			return (AndroidETC2FallbackOverride)settings.AndroidETC2FallbackOverride;
		}
	}
}
