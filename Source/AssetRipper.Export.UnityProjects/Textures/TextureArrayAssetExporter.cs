using AssetRipper.Assets;
using AssetRipper.Export.Modules.Textures;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.SourceGenerated.Classes.ClassID_117;
using AssetRipper.SourceGenerated.Classes.ClassID_187;
using AssetRipper.SourceGenerated.Classes.ClassID_188;
using AssetRipper.SourceGenerated.Classes.ClassID_189;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.StreamingInfo;
using TGASharpLib;
using AssetRipper.TextureDecoder.Rgb;
using AssetRipper.TextureDecoder.Rgb.Formats;
using AssetRipper.TextureDecoder;

namespace AssetRipper.Export.UnityProjects.Textures;

public sealed class TextureArrayAssetExporter : BinaryAssetExporter
{
	public ImageExportFormat ImageExportFormat { get; private set; }
	public string TA_Name = string.Empty;
	public int ImageCount = 0;
	public Dictionary<int, byte[]> ImagesData = new Dictionary<int, byte[]>();

	public TextureArrayAssetExporter(LibraryConfiguration configuration)
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

					string fileName = path;
					TA_Name = texture2DArray.Name;
					ImageCount = texture2DArray.Depth;

					// data length divided by image count = targa image
					byte[] bytes = texture2DArray.StreamData.GetContent(asset.Collection);
					uint uLen = (uint)bytes.Length;

					int imgDataSize = (int)uLen / ImageCount;
					Logger.Info($"Texture name: {texture2DArray.Name}, ImageCount: {ImageCount}, bytes len: {bytes.Length}, uLen: {uLen}, imgDataSize: {imgDataSize}");
					
					int curIndex = 0;

					for (int i = 0; i < ImageCount; i++)
					{
						byte[] imgData = new byte[imgDataSize];
						Array.Copy(bytes, curIndex, imgData, 0, imgDataSize);
						curIndex += imgDataSize;

						if (!ImagesData.TryAdd(i, imgData))
							Logger.Info($"Failed to add index: {i} to the ImagesData dictionary");

						SaveTexture(texture2DArray, imgData, Path.GetDirectoryName(path), i, imgDataSize);
					}
					
					// write the pure targa binary bytes to file
					using FileStream stream = File.Create(fileName);
					stream.Write(bytes, 0, bytes.Length);

					// TODO: write as .asset file if we can figure it out

					// clear the dictionary
					ImagesData = new Dictionary<int, byte[]>();

					return true;
				}

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

	public void SaveTexture(ITexture2DArray texture2DArray, byte[] bytes, string path, int id, int imgDataSize)
	{
		// create Textures folder
		string dirTexs = Path.Combine(path, "Textures");
		Directory.CreateDirectory(dirTexs);

		// create sub folder for each array in the collection
		string dirSubFolder = Path.Combine(dirTexs, TA_Name);
		Directory.CreateDirectory(dirSubFolder);

		// save as image format chosen in the settings
		string filePath = Path.Combine(dirSubFolder, TA_Name + "_" + id.ToString() + ".tga");
		Logger.Info($"Saving texture to path: {filePath}");

		using Stream exportStream = File.Create(filePath);
		DirectBitmap bitmap = ConvertToBitmap(texture2DArray.FormatE, texture2DArray.Width, texture2DArray.Height, bytes, imgDataSize);
		bitmap.SaveAsTga(exportStream);
	}

	public DirectBitmap ConvertToBitmap(TextureFormat texFormat, int width, int height, byte[] data, int imgDataSize)
	{
		DirectBitmap bitmap = new DirectBitmap<ColorBGRA32, byte>(width, height, 1);
		int outputSize = width * height * bitmap.PixelSize;
		int format = (int)texFormat;
		ReadOnlySpan<byte> inputSpan = new ReadOnlySpan<byte>(data, 0, imgDataSize);
		Span<byte> outputSpan = bitmap.Bits.Slice(0, outputSize);
		bool decoded = false;

		switch (format)
		{
			case 96: // 96 = RGBA_DXT1_SRGB
				decoded = TextureConverter.TryDecodeTexture<ColorBGRA32, byte>(TextureFormat.DXT1, width, height, inputSpan, outputSpan);
				break;

			case 100: // 100 = RGBA_DXT5_SRGB
			case 101: // 101 = RGBA_DXT5_UNorm
				decoded = TextureConverter.TryDecodeTexture<ColorBGRA32, byte>(TextureFormat.DXT5, width, height, inputSpan, outputSpan);
				// unpack the normal here
				if (format == 101)
					TextureConverter.UnpackNormal_7(outputSpan);
				break;

			default:
				Logger.Log(LogType.Error, LogCategory.Export, $"Unsupported texture format '{texFormat}'");
				break;
		}

		if (!decoded)
			Logger.Log(LogType.Error, LogCategory.Export, $"Failed to decode texture format '{texFormat}'");

		bitmap.FlipY();
		return bitmap;
	}
}
