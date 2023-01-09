using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Export.UnityProjects.Project.Exporters;
using AssetRipper.Export.UnityProjects.Utils;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.Textures
{
	public class TextureAssetExporter : BinaryAssetExporter
	{
		public ImageExportFormat ImageExportFormat { get; private set; }
		public SpriteExportMode SpriteExportMode { get; private set; }

		public TextureAssetExporter(LibraryConfiguration configuration)
		{
			ImageExportFormat = configuration.ImageExportFormat;
			SpriteExportMode = configuration.SpriteExportMode;
		}

		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset switch
			{
				ITexture2D texture => texture.CheckAssetIntegrity(),
				ISprite => SpriteExportMode == SpriteExportMode.Texture2D,
				_ => false
			};
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			ITexture2D texture = (ITexture2D)asset;
			if (!texture.CheckAssetIntegrity())
			{
				Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{texture.NameString}' because resources file '{texture.StreamData_C28?.Path}' hasn't been found");
				return false;
			}

			using DirectBitmap? bitmap = TextureConverter.ConvertToBitmap(texture);
			if (bitmap is null)
			{
				Logger.Log(LogType.Warning, LogCategory.Export, $"Unable to convert '{texture.NameString}' to bitmap");
				return false;
			}
			return bitmap.Save(path, ImageExportFormat);
		}

		public override IExportCollection CreateCollection(TemporaryAssetCollection virtualFile, IUnityObjectBase asset)
		{
			if (asset is ISprite sprite)
			{
				return TextureExportCollection.CreateExportCollection(this, sprite);
			}

			TextureExportCollection collection = new TextureExportCollection(this, (ITexture2D)asset, SpriteExportMode != SpriteExportMode.Yaml);
			return collection;
		}
	}
}
