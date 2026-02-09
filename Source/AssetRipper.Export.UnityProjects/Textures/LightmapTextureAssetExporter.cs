using AssetRipper.Assets;
using AssetRipper.Export.Configuration;
using AssetRipper.Export.Modules.Textures;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated.Classes.ClassID_1120;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.Textures;

public class LightmapTextureAssetExporter : BinaryAssetExporter
{
	public ImageExportFormat ImageExportFormat { get; private set; }

	public LightmapTextureAssetExporter(ImageExportFormat imageExportFormat)
	{
		ImageExportFormat = imageExportFormat;
	}

	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		if (asset.MainAsset is ILightingDataAsset)
		{
			exportCollection = new LightmapExportCollection(this, (ITexture2D)asset);
			return true;
		}
		else
		{
			exportCollection = null;
			return false;
		}
	}

	public override bool Export(IExportContainer container, IUnityObjectBase asset, string path, FileSystem fileSystem)
	{
		ITexture2D texture = (ITexture2D)asset;
		if (!texture.CheckAssetIntegrity())
		{
			Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{texture.Name}' because resources file '{texture.StreamData_C28?.Path}' hasn't been found");
			return false;
		}

		if (TextureConverter.TryConvertToBitmap(texture, out DirectBitmap bitmap))
		{
			using Stream stream = fileSystem.File.Create(path);
			bitmap.Save(stream, ImageExportFormat);
			return true;
		}
		else
		{
			Logger.Log(LogType.Warning, LogCategory.Export, $"Unable to convert '{texture.Name}' to bitmap");
			return false;
		}
	}

	private sealed class LightmapExportCollection(LightmapTextureAssetExporter exporter, ITexture2D lightmap) : AssetExportCollection<ITexture2D>(exporter, lightmap)
	{
		protected override string GetExportExtension(IUnityObjectBase asset)
		{
			return ((LightmapTextureAssetExporter)AssetExporter).ImageExportFormat.GetFileExtension();
		}

		protected override IUnityObjectBase CreateImporter(IExportContainer container)
		{
			return ImporterFactory.GenerateTextureImporter(container, Asset);
		}
	}
}
