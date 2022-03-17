using AssetRipper.Core;
using AssetRipper.Core.Classes.Sprite;
using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Library.Configuration;
using AssetRipper.Library.Exporters.Textures.Enums;
using AssetRipper.Library.Exporters.Textures.Extensions;
using AssetRipper.Library.Utils;

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
			if (asset is Texture2D texture)//Texture2D for now
			{
				return texture.CheckAssetIntegrity();
			}
			if (asset is Sprite)
			{
				return SpriteExportMode == SpriteExportMode.Texture2D;
			}
			return false;
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			ITexture2D texture = (ITexture2D)asset;
			if (!texture.CheckAssetIntegrity())
			{
				Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{texture.Name}' because resources file '{texture.StreamData.Path}' hasn't been found");
				return false;
			}

			using DirectBitmap bitmap = ConvertToBitmap(texture);
			if (bitmap == null)
			{
				Logger.Log(LogType.Warning, LogCategory.Export, $"Unable to convert '{texture.Name}' to bitmap");
				return false;
			}
			if (System.OperatingSystem.IsWindows())
			{
				bitmap.Save(path, ImageExportFormat);
			}
			else
			{
				TaskManager.AddTask(bitmap.SaveAsync(path, ImageExportFormat));
			}
			return true;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			if (asset is Sprite)
			{
				return TextureExportCollection.CreateExportCollection(this, (Sprite)asset);
			}
			var collection = new TextureExportCollection(this, (ITexture2D)asset, true);
			collection.FileExtension = ImageExportFormat.GetFileExtension();
			return collection;
		}

		public static DirectBitmap ConvertToBitmap(ITexture2D texture)
		{
			byte[] buffer = texture.GetImageData();
			if (buffer.Length == 0)
			{
				return null;
			}

			int pvrtcBitCount = texture.PVRTCBitCount(true);
			int astcBlockSize = texture.ASTCBlockSize(true);
			KTXBaseInternalFormat baseInternalFormat = texture.GetKTXBaseInternalFormat(true);

			DirectBitmap bitmap = ConvertToBitmap(texture.TextureFormat, texture.Width, texture.Height, texture.SerializedFile.Version, buffer, pvrtcBitCount, astcBlockSize, baseInternalFormat);

			if (bitmap == null)
			{
				return null;
			}

			// despite the name, this packing works for different formats
			if (texture.LightmapFormat == TextureUsageMode.NormalmapDXT5nm)
			{
				TextureConverter.UnpackNormal(bitmap.BitsPtr, bitmap.Bits.Length);
			}

			return bitmap;
		}

		public static DirectBitmap ConvertToBitmap(TextureFormat textureFormat, int width, int height, UnityVersion version, byte[] data, int pvrtcBitCount, int astcBlockSize, KTXBaseInternalFormat ktxBaseInternalFormat)
		{
			if (width == 0 || height == 0)
				return new DirectBitmap(1, 1);

			switch (textureFormat)
			{
				case TextureFormat.DXT1:
				case TextureFormat.DXT3:
				case TextureFormat.DXT5:
					return TextureConverter.DXTTextureToBitmap(textureFormat, width, height, data);

				case TextureFormat.Alpha8:
				case TextureFormat.ARGB4444:
				case TextureFormat.RGB24:
				case TextureFormat.RGBA32:
				case TextureFormat.ARGB32:
				case TextureFormat.RGB565:
				case TextureFormat.R16:
				case TextureFormat.RGBA4444:
				case TextureFormat.BGRA32:
				case TextureFormat.RHalf:
				case TextureFormat.RGHalf:
				case TextureFormat.RGBAHalf:
				case TextureFormat.RFloat:
				case TextureFormat.RGFloat:
				case TextureFormat.RGBAFloat:
				case TextureFormat.RGB9e5Float:
				case TextureFormat.RG16:
				case TextureFormat.R8:
					return TextureConverter.RGBTextureToBitmap(textureFormat, width, height, data);

				case TextureFormat.YUY2:
					return TextureConverter.YUY2TextureToBitmap(textureFormat, width, height, data);

				case TextureFormat.PVRTC_RGB2:
				case TextureFormat.PVRTC_RGBA2:
				case TextureFormat.PVRTC_RGB4:
				case TextureFormat.PVRTC_RGBA4:
					return TextureConverter.PVRTCTextureToBitmap(pvrtcBitCount, textureFormat, width, height, data);

				case TextureFormat.ETC_RGB4:
				case TextureFormat.EAC_R:
				case TextureFormat.EAC_R_SIGNED:
				case TextureFormat.EAC_RG:
				case TextureFormat.EAC_RG_SIGNED:
				case TextureFormat.ETC2_RGB:
				case TextureFormat.ETC2_RGBA1:
				case TextureFormat.ETC2_RGBA8:
				case TextureFormat.ETC_RGB4_3DS:
				case TextureFormat.ETC_RGBA8_3DS:
					return TextureConverter.ETCTextureToBitmap(textureFormat, width, height, data);

				case TextureFormat.ATC_RGB4:
				case TextureFormat.ATC_RGBA8:
					return TextureConverter.ATCTextureToBitmap(textureFormat, width, height, data);

				case TextureFormat.ASTC_RGB_4x4:
				case TextureFormat.ASTC_RGB_5x5:
				case TextureFormat.ASTC_RGB_6x6:
				case TextureFormat.ASTC_RGB_8x8:
				case TextureFormat.ASTC_RGB_10x10:
				case TextureFormat.ASTC_RGB_12x12:
				case TextureFormat.ASTC_RGBA_4x4:
				case TextureFormat.ASTC_RGBA_5x5:
				case TextureFormat.ASTC_RGBA_6x6:
				case TextureFormat.ASTC_RGBA_8x8:
				case TextureFormat.ASTC_RGBA_10x10:
				case TextureFormat.ASTC_RGBA_12x12:
					return TextureConverter.ASTCTextureToBitmap(astcBlockSize, width, height, data);

				case TextureFormat.BC4:
				case TextureFormat.BC5:
				case TextureFormat.BC6H:
				case TextureFormat.BC7:
					return TextureConverter.TexgenpackTextureToBitmap(ktxBaseInternalFormat, textureFormat, width, height, data);

				case TextureFormat.DXT1Crunched:
				case TextureFormat.DXT5Crunched:
					return TextureConverter.DXTCrunchedTextureToBitmap(textureFormat, width, height, version, data);

				case TextureFormat.ETC_RGB4Crunched:
				case TextureFormat.ETC2_RGBA8Crunched:
					return TextureConverter.ETCCrunchedTextureToBitmap(textureFormat, width, height, version, data);

				default:
					Logger.Log(LogType.Error, LogCategory.Export, $"Unsupported texture format '{textureFormat}'");
					return null;
			}
		}
	}
}
