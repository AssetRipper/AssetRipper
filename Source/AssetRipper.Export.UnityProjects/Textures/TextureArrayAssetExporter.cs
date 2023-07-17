using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Export.UnityProjects.Utils;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated.Classes.ClassID_117;
using AssetRipper.SourceGenerated.Classes.ClassID_187;
using AssetRipper.SourceGenerated.Classes.ClassID_188;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.StreamingInfo;

namespace AssetRipper.Export.UnityProjects.Textures;

public sealed class TextureArrayAssetExporter : BinaryAssetExporter
{
	public ImageExportFormat ImageExportFormat { get; private set; }

	public TextureArrayAssetExporter(LibraryConfiguration configuration)
	{
		ImageExportFormat = configuration.ImageExportFormat;
	}

	public override bool TryCreateCollection(IUnityObjectBase asset, TemporaryAssetCollection temporaryFile, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		exportCollection = asset switch
		{
			ITexture2D texture when texture.CheckAssetIntegrity() => new TextureArrayAssetExportCollection(this, texture),
			_ => null,
		};
		return exportCollection is not null;
	}

	public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
	{
		DirectBitmap? bitmap;
		switch (asset)
		{
			case ICubemapArray cubemapArray:
				{
					if (!cubemapArray.CheckAssetIntegrity())
					{
						WarnResourceFileNotFound(cubemapArray.NameString, cubemapArray.StreamData_C188);
						return false;
					}
					bitmap = TextureConverter.ConvertToBitmap(cubemapArray);
				}
				break;
			case ITexture2DArray texture2DArray:
				{
					if (!texture2DArray.CheckAssetIntegrity())
					{
						WarnResourceFileNotFound(texture2DArray.NameString, texture2DArray.StreamData_C187);
						return false;
					}
					bitmap = TextureConverter.ConvertToBitmap(texture2DArray);
				}
				break;
			case ITexture3D texture3D:
				{
					if (!texture3D.CheckAssetIntegrity())
					{
						WarnResourceFileNotFound(texture3D.NameString, texture3D.StreamData_C117);
						return false;
					}
					bitmap = TextureConverter.ConvertToBitmap(texture3D);
				}
				break;
			default:
				{
					Logger.Log(LogType.Error, LogCategory.Export, $"Texture array '{asset}' has unsupported type '{asset.GetType().Name}'");
				}
				return false;
		}

		if (bitmap is null)
		{
			Logger.Log(LogType.Warning, LogCategory.Export, $"Unable to convert '{asset}' to bitmap");
			return false;
		}
		return bitmap.Save(path, ImageExportFormat);

		static void WarnResourceFileNotFound(string assetName, IStreamingInfo? streamingInfo)
		{
			Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{assetName}' because resources file '{streamingInfo?.Path}' hasn't been found");
		}
	}
}
