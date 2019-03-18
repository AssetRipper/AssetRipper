using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using uTinyRipper;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes;
using uTinyRipper.Classes.Textures;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipperGUI.Exporters
{
	public class TextureAssetExporter : IAssetExporter
	{
		public static void ExportTexture(Texture2D texture, Stream exportStream)
		{
			byte[] buffer = null;
			if (Texture2D.IsReadStreamData(texture.File.Version))
			{
				string path = texture.StreamData.Path;
				if (path == string.Empty)
				{
					buffer = (byte[])texture.ImageData;
				}
				else
				{
					if (texture.ImageData.Count != 0)
					{
						throw new Exception("Texture contains data and resource path");
					}

					using (ResourcesFile res = texture.File.Collection.FindResourcesFile(texture.File, path))
					{
						using (PartialStream resStream = new PartialStream(res.Stream, res.Offset, res.Size))
						{
							resStream.Position = texture.StreamData.Offset;
							buffer = new byte[texture.StreamData.Size];
							resStream.Read(buffer, 0, buffer.Length);
						}
					}
				}
			}
			else
			{
				buffer = (byte[])texture.ImageData;
			}

			using (Bitmap bitmap = ConvertToBitmap(texture, buffer))
			{
				if (bitmap != null)
				{
					bitmap.Save(exportStream, ImageFormat.Png);
				}
			}
		}

		private static Bitmap ConvertToBitmap(Texture2D texture, byte[] data)
		{
			switch (texture.TextureFormat)
			{
				case TextureFormat.DXT1:
				case TextureFormat.DXT3:
				case TextureFormat.DXT5:
				case TextureFormat.Alpha8:
				case TextureFormat.ARGB4444:
				case TextureFormat.RGB24:
				case TextureFormat.RGBA32:
				case TextureFormat.ARGB32:
				case TextureFormat.R16:
				case TextureFormat.RGBA4444:
				case TextureFormat.BGRA32:
				case TextureFormat.RG16:
				case TextureFormat.R8:
				case TextureFormat.RGB565:
					return TextureConverter.DDSTextureToBitmap(texture, data);

				case TextureFormat.YUY2:
				case TextureFormat.PVRTC_RGB2:
				case TextureFormat.PVRTC_RGBA2:
				case TextureFormat.PVRTC_RGB4:
				case TextureFormat.PVRTC_RGBA4:
				case TextureFormat.ETC_RGB4:
				case TextureFormat.ETC2_RGB:
				case TextureFormat.ETC2_RGBA1:
				case TextureFormat.ETC2_RGBA8:
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
				case TextureFormat.ETC_RGB4_3DS:
				case TextureFormat.ETC_RGBA8_3DS:
					return TextureConverter.PVRTextureToBitmap(texture, data);

				case TextureFormat.RHalf:
				case TextureFormat.RGHalf:
				case TextureFormat.RGBAHalf:
				case TextureFormat.RFloat:
				case TextureFormat.RGFloat:
				case TextureFormat.RGBAFloat:
				case TextureFormat.RGB9e5Float:
				case TextureFormat.ATC_RGB4:
				case TextureFormat.ATC_RGBA8:
				case TextureFormat.EAC_R:
				case TextureFormat.EAC_R_SIGNED:
				case TextureFormat.EAC_RG:
				case TextureFormat.EAC_RG_SIGNED:
					return TextureConverter.TextureConverterTextureToBitmap(texture, data);

				case TextureFormat.BC4:
				case TextureFormat.BC5:
				case TextureFormat.BC6H:
				case TextureFormat.BC7:
					return TextureConverter.TexgenpackTextureToBitmap(texture, data);

				case TextureFormat.DXT1Crunched:
				case TextureFormat.DXT5Crunched:
					return TextureConverter.DDSCrunchedTextureToBitmap(texture, data);

				case TextureFormat.ETC_RGB4Crunched:
				case TextureFormat.ETC2_RGBA8Crunched:
					return TextureConverter.PVRCrunchedTextureToBitmap(texture, data);

				default:
					Logger.Log(LogType.Error, LogCategory.Export, $"Unsupported texture format '{texture.TextureFormat}'");
					return null;
			}
		}

		public bool IsHandle(Object asset)
		{
			if (asset.ClassID == ClassIDType.Texture2D)
			{
				Texture2D texture = (Texture2D)asset;
				return texture.IsValidData;
			}
			return true;
		}

		public void Export(IExportContainer container, Object asset, string path)
		{
			Export(container, asset, path, null);
		}

		public void Export(IExportContainer container, Object asset, string path, Action<IExportContainer, Object, string> callback)
		{
			Texture2D texture = (Texture2D)asset;
			if (Texture2D.IsReadStreamData(texture.File.Version))
			{
				string resourcePath = texture.StreamData.Path;
				if (resourcePath != string.Empty)
				{
					using (ResourcesFile res = texture.File.Collection.FindResourcesFile(texture.File, resourcePath))
					{
						if (res == null)
						{
							Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{texture.Name}' because resources file '{resourcePath}' wasn't found");
							return;
						}
					}
				}
			}

			using (Stream fileStream = FileUtils.CreateVirtualFile(path))
			{
				ExportTexture(texture, fileStream);
			}

			callback?.Invoke(container, asset, path);
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string path)
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
