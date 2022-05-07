using AssetRipper.Core.Classes.Meta.Importers.Asset;
using AssetRipper.Core.Classes.Texture2D;

namespace AssetRipper.Core.Classes.Meta.Importers
{
	public interface ISpriteImporter : IAssetImporter
	{
		bool IsReadable { get; set; }
		bool SRGBTexture { get; set; }
		bool StreamingMipmaps { get; set; }
		int StreamingMipmapsPriority { get; set; }
		IGLTextureSettings TextureSettings { get; }
	}

	public static class SpriteImporterExtensions
	{
		public static void SetToDefault(this ISpriteImporter importer)
		{
			importer.TextureSettings.FilterMode = FilterMode.Bilinear;
			importer.TextureSettings.Aniso = 1;
			importer.SRGBTexture = true;
		}
	}
}
