using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Meta.Importers;
using AssetRipper.Core.Classes.Meta.Importers.Texture;
using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.Project;

namespace AssetRipper.Core.Converters.Texture2D
{
	public static class Texture2DConverter
	{
		public static TextureImporter GenerateTextureImporter(IExportContainer container, ITexture2D origin)
		{
			TextureImporter instance = new TextureImporter(container.ExportLayout);
			instance.EnableMipMap = origin.MipCount > 1 ? 1 : 0;
			instance.SRGBTexture = origin.ColorSpace == ColorSpace.Linear ? 1 : 0;
			instance.StreamingMipmaps = origin.StreamingMipmaps ? 1 : 0;
			instance.StreamingMipmapsPriority = origin.StreamingMipmapsPriority;
			instance.IsReadable = origin.IsReadable ? 1 : 0;
			instance.TextureFormat = origin.TextureFormat;
			instance.MaxTextureSize = System.Math.Min(2048, System.Math.Max(origin.Width, origin.Height));
			instance.TextureSettings.CopyValues(origin.TextureSettings);
			instance.NPOTScale = TextureImporterNPOTScale.None;
			instance.AlphaIsTransparency = 1;
			instance.TextureType = origin.LightmapFormat.IsNormalmap() ? TextureImporterType.NormalMap : TextureImporterType.Default;
			instance.TextureShape = origin is ICubemap ? TextureImporterShape.TextureCube : TextureImporterShape.Texture2D;
			return instance;
		}

		public static ISpriteImporter GenerateIHVImporter(IExportContainer container, ITexture2D origin)
		{
			IHVImageFormatImporter instance = new IHVImageFormatImporter(container.ExportLayout);
			instance.SetToDefault();
			instance.IsReadable = origin.IsReadable;
			instance.SRGBTexture = origin.ColorSpace == ColorSpace.Linear;
			instance.StreamingMipmaps = origin.StreamingMipmaps;
			instance.StreamingMipmapsPriority = origin.StreamingMipmapsPriority;
			return instance;
		}
	}
}
