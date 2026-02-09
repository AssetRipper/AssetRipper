using AssetRipper.Assets;
using AssetRipper.Export.Configuration;
using AssetRipper.Export.Modules.Textures;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated.Classes.ClassID_117;
using AssetRipper.SourceGenerated.Classes.ClassID_187;
using AssetRipper.SourceGenerated.Classes.ClassID_188;
using AssetRipper.SourceGenerated.Classes.ClassID_189;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.StreamingInfo;

namespace AssetRipper.Export.UnityProjects.Textures;

public sealed class TextureArrayAssetExporter : BinaryAssetExporter
{
	public ImageExportFormat ImageExportFormat { get; private set; }

	public TextureArrayAssetExporter(FullConfiguration configuration)
	{
		ImageExportFormat = configuration.ExportSettings.ImageExportFormat;
	}

	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		exportCollection = asset switch
		{
			IImageTexture texture when texture.CheckAssetIntegrity() && texture.MainAsset is null => new TextureArrayAssetExportCollection(this, texture),
			_ => null,
		};
		return exportCollection is not null;
	}

	public override bool Export(IExportContainer container, IUnityObjectBase asset, string path, FileSystem fileSystem)
	{
		bool success;
		DirectBitmap bitmap;
		switch (asset)
		{
			case ICubemapArray cubemapArray:
				{
					if (!cubemapArray.CheckAssetIntegrity())
					{
						WarnResourceFileNotFound(cubemapArray.Name, cubemapArray.StreamData);
						return false;
					}
					success = TextureConverter.TryConvertToBitmap(cubemapArray, out bitmap);
				}
				break;
			case ITexture2DArray texture2DArray:
				{
					if (!texture2DArray.CheckAssetIntegrity())
					{
						WarnResourceFileNotFound(texture2DArray.Name, texture2DArray.StreamData);
						return false;
					}
					success = TextureConverter.TryConvertToBitmap(texture2DArray, out bitmap);
				}
				break;
			case ITexture3D texture3D:
				{
					if (!texture3D.CheckAssetIntegrity())
					{
						WarnResourceFileNotFound(texture3D.Name, texture3D.StreamData);
						return false;
					}
					success = TextureConverter.TryConvertToBitmap(texture3D, out bitmap);
				}
				break;
			default:
				{
					Logger.Log(LogType.Error, LogCategory.Export, $"Texture array '{asset}' has unsupported type '{asset.GetType().Name}'");
				}
				return false;
		}

		if (success)
		{
			using Stream stream = fileSystem.File.Create(path);
			bitmap.Save(stream, ImageExportFormat);
			return true;
		}
		else
		{
			Logger.Log(LogType.Warning, LogCategory.Export, $"Unable to convert '{asset}' to bitmap");
			return false;
		}

		static void WarnResourceFileNotFound(Utf8String assetName, IStreamingInfo? streamingInfo)
		{
			Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{assetName}' because resources file '{streamingInfo?.Path}' hasn't been found");
		}
	}
}
