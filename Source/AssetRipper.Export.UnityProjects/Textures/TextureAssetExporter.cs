using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Processing.Textures;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Extensions;
using DirectBitmap = AssetRipper.Export.UnityProjects.Utils.DirectBitmap<AssetRipper.TextureDecoder.Rgb.Formats.ColorBGRA32, byte>;

namespace AssetRipper.Export.UnityProjects.Textures
{
	public class TextureAssetExporter : BinaryAssetExporter
	{
		public ImageExportFormat ImageExportFormat { get; private set; }
		public SpriteExportMode SpriteExportMode { get; private set; }

		public TextureAssetExporter(LibraryConfiguration configuration)
		{
			ImageExportFormat = configuration.ExportSettings.ImageExportFormat;
			SpriteExportMode = configuration.ExportSettings.SpriteExportMode;
		}

		public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			if (asset.MainAsset is SpriteInformationObject spriteInformationObject)
			{
				exportCollection = new TextureExportCollection(this, spriteInformationObject, SpriteExportMode != SpriteExportMode.Yaml);
				return true;
			}
			else
			{
				exportCollection = null;
				return false;
			}
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			ITexture2D texture = (ITexture2D)asset;
			if (!texture.CheckAssetIntegrity())
			{
				Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{texture.Name}' because resources file '{texture.StreamData_C28?.Path}' hasn't been found");
				return false;
			}

			if (TextureConverter.TryConvertToBitmap(texture, out DirectBitmap bitmap))
			{
				using FileStream stream = File.Create(path);
				bitmap.Save(stream, ImageExportFormat);
				return true;
			}
			else
			{
				Logger.Log(LogType.Warning, LogCategory.Export, $"Unable to convert '{texture.Name}' to bitmap");
				return false;
			}
		}
	}
}
