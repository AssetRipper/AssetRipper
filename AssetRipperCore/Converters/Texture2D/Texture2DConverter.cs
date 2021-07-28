using AssetRipper.Project;
using AssetRipper.Classes.Meta.Importers;
using AssetRipper.Classes.Meta.Importers.Texture;
using AssetRipper.Classes.Texture2D;
using System;

namespace AssetRipper.Converters.Texture2D
{
	public static class Texture2DConverter
	{
		public static TextureImporter GenerateTextureImporter(IExportContainer container, AssetRipper.Classes.Texture2D.Texture2D origin)
		{
			TextureImporter instance = new TextureImporter(container.ExportLayout);
			instance.EnableMipMap = origin.MipCount > 1 ? 1 : 0;
			instance.SRGBTexture = origin.ColorSpace == ColorSpace.Linear ? 1 : 0;
			instance.StreamingMipmaps = origin.StreamingMipmaps ? 1 : 0;
			instance.StreamingMipmapsPriority = origin.StreamingMipmapsPriority;
			instance.IsReadable = origin.IsReadable ? 1 : 0;
			instance.TextureFormat = origin.TextureFormat;
			instance.MaxTextureSize = Math.Min(2048, Math.Max(origin.Width, origin.Height));
			instance.TextureSettings = origin.TextureSettings;
			instance.NPOTScale = TextureImporterNPOTScale.None;
			instance.AlphaIsTransparency = 1;
			instance.TextureType = origin.LightmapFormat.IsNormalmap() ? TextureImporterType.NormalMap : TextureImporterType.Default;
			return instance;
		}

		public static IHVImageFormatImporter GenerateIHVImporter(IExportContainer container, AssetRipper.Classes.Texture2D.Texture2D origin)
		{
			IHVImageFormatImporter instance = new IHVImageFormatImporter(container.ExportLayout);
			instance.IsReadable = origin.IsReadable;
			instance.SRGBTexture = origin.ColorSpace == ColorSpace.Linear;
			instance.StreamingMipmaps = origin.StreamingMipmaps;
			instance.StreamingMipmapsPriority = origin.StreamingMipmapsPriority;
			return instance;
		}
	}
}
