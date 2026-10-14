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
	public ImageExportFormat ImageExportFormat { get; }
	public bool PreferOriginalTextureExtension { get; }

	public TextureArrayAssetExporter(FullConfiguration configuration)
	{
		ImageExportFormat = configuration.ExportSettings.ImageExportFormat;
		PreferOriginalTextureExtension = configuration.ExportSettings.PreferOriginalTextureExtension;
	}

	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		exportCollection = asset switch
		{
			IImageTexture texture when texture.CheckAssetIntegrity() && texture.MainAsset is null && CanBeImported(texture) => new TextureArrayAssetExportCollection(this, texture),
			_ => null,
		};
		return exportCollection is not null;
	}

	/// <summary>
	/// Whether the layers fit in an image that Unity can import. If they don't, the texture is exported as a native asset instead.
	/// </summary>
	private static bool CanBeImported(IImageTexture texture)
	{
		return !TryGetLayerSize(texture, out int width, out int height, out int depth)
			|| FlipbookLayout.TryCreate(width, height, depth, out _);
	}

	/// <summary>
	/// Get the size of each layer and the number of layers, for textures that are imported from a flipbook.
	/// </summary>
	internal static bool TryGetLayerSize(IUnityObjectBase asset, out int width, out int height, out int depth)
	{
		switch (asset)
		{
			case ITexture2DArray texture2DArray:
				(width, height, depth) = (texture2DArray.Width, texture2DArray.Height, texture2DArray.Depth);
				return true;
			case ITexture3D texture3D:
				(width, height, depth) = (texture3D.Width, texture3D.Height, texture3D.Depth);
				return true;
			default:
				(width, height, depth) = (0, 0, 0);
				return false;
		}
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

		if (success && TryGetLayerSize(asset, out int layerWidth, out int layerHeight, out int layerCount)
			&& FlipbookLayout.TryCreate(layerWidth, layerHeight, layerCount, out FlipbookLayout layout)
			&& layout.Columns > 1)
		{
			// A single column would be too tall for Unity to import.
			bitmap = bitmap.ArrangeLayersInGrid(layout.Columns, layout.Rows);
		}

		if (success)
		{
			using Stream stream = fileSystem.File.Create(path);
			bitmap.Save(stream, asset.GetTextureExportFormat(PreferOriginalTextureExtension, ImageExportFormat));
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
