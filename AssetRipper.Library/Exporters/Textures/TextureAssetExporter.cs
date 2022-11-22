using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Core;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.Library.Configuration;
using AssetRipper.Library.Exporters.Textures.Extensions;
using AssetRipper.Library.Utils;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.Library.Exporters.Textures
{
	public class TextureAssetExporter : BinaryAssetExporter
	{
		private ImageExportFormat ImageExportFormat { get; set; }
		private SpriteExportMode SpriteExportMode { get; set; }

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
			if (OperatingSystem.IsWindows())
			{
				bitmap.Save(path, ImageExportFormat);
			}
			else
			{
				TaskManager.AddTask(bitmap.SaveAsync(path, ImageExportFormat));
			}
			return true;
		}

		public override IExportCollection CreateCollection(TemporaryAssetCollection virtualFile, IUnityObjectBase asset)
		{
			if (asset is ISprite sprite)
			{
				return TextureExportCollection.CreateExportCollection(this, sprite);
			}

			TextureExportCollection collection = new TextureExportCollection(this, (ITexture2D)asset, true, SpriteExportMode != SpriteExportMode.Yaml);
			collection.FileExtension = ImageExportFormat.GetFileExtension();
			return collection;
		}
	}
}
