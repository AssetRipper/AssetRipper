using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using uTinyRipper;
using uTinyRipper.Classes;
using uTinyRipper.Classes.Textures;
using uTinyRipper.Converters;
using uTinyRipper.Project;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipperGUI.Exporters
{
	public class TextureAssetExporter : IAssetExporter
	{
		public static bool ExportTexture(Texture2D texture, Stream exportStream)
		{
			byte[] buffer = (byte[])texture.GetImageData();
			if (buffer.Length == 0)
			{
				return false;
			}

			using (DirectBitmap bitmap = ConvertToBitmap(texture, buffer))
			{
				if (bitmap == null)
				{
					return false;
				}
				else
				{
					// despite the name, this packing works for different formats
					if (texture.LightmapFormat == TextureUsageMode.NormalmapDXT5nm)
					{
						TextureConverter.UnpackNormal(bitmap.BitsPtr, bitmap.Bits.Length);
					}

					bitmap.Save(exportStream, ImageFormat.Png);
					return true;
				}
			}
		}

		private static DirectBitmap ConvertToBitmap(Texture2D texture, byte[] data)
		{
			switch (texture.TextureFormat)
			{
				case TextureFormat.DXT1:
				case TextureFormat.DXT3:
				case TextureFormat.DXT5:
					return TextureConverter.DXTTextureToBitmap(texture, data);

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
					return TextureConverter.RGBTextureToBitmap(texture, data);

				case TextureFormat.YUY2:
					return TextureConverter.YUY2TextureToBitmap(texture, data);

				case TextureFormat.PVRTC_RGB2:
				case TextureFormat.PVRTC_RGBA2:
				case TextureFormat.PVRTC_RGB4:
				case TextureFormat.PVRTC_RGBA4:
					return TextureConverter.PVRTCTextureToBitmap(texture, data);

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
					return TextureConverter.ETCTextureToBitmap(texture, data);

				case TextureFormat.ATC_RGB4:
				case TextureFormat.ATC_RGBA8:
					return TextureConverter.ATCTextureToBitmap(texture, data);

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
					return TextureConverter.ASTCTextureToBitmap(texture, data);

				case TextureFormat.BC4:
				case TextureFormat.BC5:
				case TextureFormat.BC6H:
				case TextureFormat.BC7:
					return TextureConverter.TexgenpackTextureToBitmap(texture, data);

				case TextureFormat.DXT1Crunched:
				case TextureFormat.DXT5Crunched:
					return TextureConverter.DXTCrunchedTextureToBitmap(texture, data);

				case TextureFormat.ETC_RGB4Crunched:
				case TextureFormat.ETC2_RGBA8Crunched:
					return TextureConverter.ETCCrunchedTextureToBitmap(texture, data);

				default:
					Logger.Log(LogType.Error, LogCategory.Export, $"Unsupported texture format '{texture.TextureFormat}'");
					return null;
			}
		}

		public bool IsHandle(Object asset, ExportOptions options)
		{
			if (asset.ClassID == ClassIDType.Texture2D)
			{
				Texture2D texture = (Texture2D)asset;
				return texture.IsValidData;
			}
			return true;
		}

		public bool Export(IExportContainer container, Object asset, string path)
		{
			Texture2D texture = (Texture2D)asset;
			if (!texture.CheckAssetIntegrity())
			{
				Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{texture.Name}' because resources file '{texture.StreamData.Path}' hasn't been found");
				return false;
			}

			using (Stream fileStream = FileUtils.CreateVirtualFile(path))
			{
				if (!ExportTexture(texture, fileStream))
				{
					Logger.Log(LogType.Warning, LogCategory.Export, $"Unable to convert '{texture.Name}' to bitmap");
					return false;
				}
			}
			return true;
		}

		public void Export(IExportContainer container, Object asset, string path, Action<IExportContainer, Object, string> callback)
		{
			Export(container, asset, path);
			callback?.Invoke(container, asset, path);
		}

		public bool Export(IExportContainer container, IEnumerable<Object> assets, string path)
		{
			throw new NotSupportedException();
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string path, Action<IExportContainer, Object, string> callback)
		{
			throw new NotSupportedException();
		}

		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			if (asset.ClassID == ClassIDType.Sprite)
			{
				return TextureExportCollection.CreateExportCollection(this, (Sprite)asset);
			}
			return new TextureExportCollection(this, (Texture2D)asset, true);
		}

		public AssetType ToExportType(Object asset)
		{
			ToUnknownExportType(asset.ClassID, out AssetType assetType);
			return assetType;
		}

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			assetType = AssetType.Meta;
			return true;
		}
	}
}
