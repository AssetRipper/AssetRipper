using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using uTinyRipper;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes;
using uTinyRipper.Classes.Textures;
using uTinyRipper.Converter.Textures.DDS;
using uTinyRipper.Converter.Textures.KTX;
using uTinyRipper.Converter.Textures.PVR;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;
using Version = uTinyRipper.Version;

namespace uTinyRipperGUI.Exporters
{
	public class TextureAssetExporter : IAssetExporter
	{
#warning TODO: replace to  other libs
		[DllImport("PVRTexLibWrapper", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool DecompressPVR(byte[] buffer, IntPtr bmp, int len);

		[DllImport("TextureConverterWrapper", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool Ponvert(byte[] buffer, IntPtr bmp, int nWidth, int nHeight, int len, int type, int bmpsize, bool fixAlpha);
		
		[DllImport("texgenpack", CallingConvention = CallingConvention.Cdecl)]
		private static extern void texgenpackdecode(int texturetype, byte[] texturedata, int width, int height, IntPtr bmp, bool fixAlpha);
		
		[DllImport("crunch", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool DecompressCRN(byte[] pSrcFileData, int srcFileSize, out IntPtr uncompressedData, out int uncompressedSize);

		[DllImport("crunchunity.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern bool DecompressUnityCRN(byte[] pSrc_file_data, int src_file_size, out IntPtr uncompressedData, out int uncompressedSize);

		private static bool IsUseUnityCrunch(Version version, TextureFormat format)
		{
			if(version.IsGreaterEqual(2017, 3))
			{
				return true;
			}
			return format == TextureFormat.ETC_RGB4Crunched || format == TextureFormat.ETC2_RGBA8Crunched;
		}

		public bool IsHandle(Object asset)
		{
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
							Logger.Instance.Log(LogType.Warning, LogCategory.Export, $"Can't export '{texture.Name}' because resources file '{resourcePath}' wasn't found");
							return;
						}
					}
				}
			}

			using (FileStream fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
			{
				ExportTexture(container, fileStream, texture);
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

		private void ExportTexture(IExportContainer container, FileStream fileStream, Texture2D texture)
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
			
			using (Bitmap bitmap = ConvertToBitmap(container, texture, buffer))
			{
				if (bitmap != null)
				{
					bitmap.Save(fileStream, ImageFormat.Png);
				}
			}
		}

		public Bitmap ConvertToBitmap(IExportContainer container, Texture2D texture, byte[] data)
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
					return DDSToBitmap(container, texture, data);

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
					using (MemoryStream dstStream = new MemoryStream())
					{
						PVRConvertParameters @params = new PVRConvertParameters
						{
							DataLength = data.Length,
							PixelFormat = texture.PVRPixelFormat,
							Width = texture.Width,
							Height = texture.Height,
							MipMapCount = texture.MipCount,
						};
						using (MemoryStream srcStream = new MemoryStream(data))
						{
							PVRConverter.ExportPVR(dstStream, srcStream, @params);
						}
						return PVRToBitmap(texture, dstStream.ToArray());
					}

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
					return TextureConverter(texture, data);

				case TextureFormat.BC4:
				case TextureFormat.BC5:
				case TextureFormat.BC6H:
				case TextureFormat.BC7:
					return Texgenpack(texture, data);

	            case TextureFormat.DXT1Crunched:
	            case TextureFormat.DXT5Crunched:
				{
					byte[] decompressed = DecompressCRN(texture, data);
					return DDSToBitmap(container, texture, decompressed);
				}

	            case TextureFormat.ETC_RGB4Crunched:
	            case TextureFormat.ETC2_RGBA8Crunched:
	            {
		            byte[] decompressed = DecompressCRN(texture, data);
		            using (MemoryStream dstStream = new MemoryStream())
		            {
			            using (MemoryStream srcStream = new MemoryStream(decompressed))
			            {
				            PVRConvertParameters @params = new PVRConvertParameters
				            {
					            DataLength = decompressed.Length,
					            PixelFormat = texture.PVRPixelFormat,
					            Width = texture.Width,
					            Height = texture.Height,
					            MipMapCount = texture.MipCount,
				            };
				            PVRConverter.ExportPVR(dstStream, srcStream, @params);
			            }
			            return PVRToBitmap(texture, dstStream.ToArray());
		            }
	            }
				
				default:
					Logger.Instance.Log(LogType.Error, LogCategory.Export, $"Unsupported texture format '{texture.TextureFormat}'");
					return null;
            }
        }

		private Bitmap DDSToBitmap(IExportContainer container, Texture2D texture, byte[] data)
		{
			DDSConvertParameters @params = new DDSConvertParameters()
			{
				DataLength = data.LongLength,
				MipMapCount = texture.MipCount,
				Width = texture.Width,
				Height = texture.Height,
				IsPitchOrLinearSize = texture.DDSIsPitchOrLinearSize,
				PixelFormatFlags = texture.DDSPixelFormatFlags,
				FourCC = (DDSFourCCType)texture.DDSFourCC,
				RGBBitCount = texture.DDSRGBBitCount,
				RBitMask = texture.DDSRBitMask,
				GBitMask = texture.DDSGBitMask,
				BBitMask = texture.DDSBBitMask,
				ABitMask = texture.DDSABitMask,
				Caps = texture.DDSCaps(container.Version),
			};

			int width = @params.Width;
			int height = @params.Height;
			int size = width * height * 4;
			byte[] buffer = new byte[size];
			using (MemoryStream destination = new MemoryStream(buffer))
			{
				using (MemoryStream source = new MemoryStream(data))
				{
					EndianType endianess = texture.IsSwapBytes(container.Platform) ? EndianType.BigEndian : EndianType.LittleEndian;
					using (EndianReader sourceReader = new EndianReader(source, endianess))
					{
						DecompressDDS(sourceReader, destination, @params);
					}
				}

				Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
				Rectangle rect = new Rectangle(0, 0, width, height);
				BitmapData bitData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
				IntPtr pointer = bitData.Scan0;
				Marshal.Copy(buffer, 0, pointer, size);
				bitmap.UnlockBits(bitData);
				return bitmap;
			}
		}

		private void DecompressDDS(BinaryReader reader, Stream destination, DDSConvertParameters @params)
		{
			if (@params.PixelFormatFlags.IsFourCC())
			{
				switch (@params.FourCC)
				{
					case DDSFourCCType.DXT1:
						DDSDecompressor.DecompressDXT1(reader, destination, @params);
						break;
					case DDSFourCCType.DXT3:
						DDSDecompressor.DecompressDXT3(reader, destination, @params);
						break;
					case DDSFourCCType.DXT5:
						DDSDecompressor.DecompressDXT5(reader, destination, @params);
						break;

					default:
						throw new NotImplementedException(@params.FourCC.ToString());
				}
			}
			else
			{
				if (@params.PixelFormatFlags.IsLuminace())
				{
					throw new NotSupportedException("Luminace isn't supported");
				}
				else
				{
					if (@params.PixelFormatFlags.IsAlphaPixels())
					{
						DDSDecompressor.DecompressRGBA(reader, destination, @params);
					}
					else
					{
						DDSDecompressor.DecompressRGB(reader, destination, @params);
					}
				}
			}
		}

		private Bitmap PVRToBitmap(Texture2D texture, byte[] data)
		{
			Bitmap bitmap = null;
			BitmapData bmd = null;
			try
			{
				bitmap = new Bitmap(texture.Width, texture.Height);
				Rectangle rect = new Rectangle(0, 0, texture.Width, texture.Height);
				bmd = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
				int len = Math.Abs(bmd.Stride) * bmd.Height;
				if (!DecompressPVR(data, bmd.Scan0, len))
				{
					bitmap.UnlockBits(bmd);
					bitmap.Dispose();
					return null;
				}

				bitmap.UnlockBits(bmd);
				bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
				return bitmap;
			}
			catch
			{
				if (bitmap != null)
				{
					if (bmd != null)
					{
						bitmap.UnlockBits(bmd);
					}
					bitmap.Dispose();
				}

				throw;
			}
		}

		private Bitmap TextureConverter(Texture2D texture, byte[] data)
		{
			Bitmap bitmap = null;
			BitmapData bmd = null;
			try
			{
				bitmap = new Bitmap(texture.Width, texture.Height);
				Rectangle rect = new Rectangle(0, 0, texture.Width, texture.Height);
				bmd = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
				int len = Math.Abs(bmd.Stride) * bmd.Height;
				bool fixAlpha = texture.KTXBaseInternalFormat == KTXBaseInternalFormat.RED || texture.KTXBaseInternalFormat == KTXBaseInternalFormat.RG;
				if (!Ponvert(data, bmd.Scan0, texture.Width, texture.Height, data.Length, (int)ToQFormat(texture.TextureFormat), len, fixAlpha))
				{
					bitmap.UnlockBits(bmd);
					bitmap.Dispose();
					return null;
				}

				bitmap.UnlockBits(bmd);
				bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
				return bitmap;
			}
			catch
			{
				if (bitmap != null)
				{
					if (bmd != null)
					{
						bitmap.UnlockBits(bmd);
					}
					bitmap.Dispose();
				}

				throw;
			}
		}

		private Bitmap Texgenpack(Texture2D texture, byte[] data)
		{
			Bitmap bitmap = null;
			BitmapData bmd = null;
			try
			{
				bitmap = new Bitmap(texture.Width, texture.Height);
				Rectangle rect = new Rectangle(0, 0, texture.Width, texture.Height);
				bmd = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
				bool fixAlpha = texture.KTXBaseInternalFormat == KTXBaseInternalFormat.RED || texture.KTXBaseInternalFormat == KTXBaseInternalFormat.RG;
				texgenpackdecode((int)ToTexgenpackTexturetype(texture.TextureFormat), data, texture.Width, texture.Height, bmd.Scan0, fixAlpha);
				bitmap.UnlockBits(bmd);
				bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
				return bitmap;
			}
			catch
			{
				if (bitmap != null)
				{
					if (bmd != null)
					{
						bitmap.UnlockBits(bmd);
					}
					bitmap.Dispose();
				}

				throw;
			}
		}

		private byte[] DecompressCRN(Texture2D texture, byte[] data)
		{
			IntPtr uncompressedData = default;
			try
			{
				bool result = IsUseUnityCrunch(texture.File.Version, texture.TextureFormat) ?
					DecompressUnityCRN(data, data.Length, out uncompressedData, out int uncompressedSize) :
					DecompressCRN(data, data.Length, out uncompressedData, out uncompressedSize);
				if (result)
				{
					byte[] uncompressedBytes = new byte[uncompressedSize];
					Marshal.Copy(uncompressedData, uncompressedBytes, 0, uncompressedSize);
					return uncompressedBytes;
				}
				else
				{
					return null;
				}
			}
			finally
			{
				Marshal.FreeHGlobal(uncompressedData);
			}
		}

		private QFormat ToQFormat(TextureFormat format)
		{
			switch (format)
            {
                case TextureFormat.DXT1:
                case TextureFormat.DXT1Crunched:
	                return QFormat.Q_FORMAT_S3TC_DXT1_RGB;

				case TextureFormat.DXT3:
					return QFormat.Q_FORMAT_S3TC_DXT3_RGBA;

				case TextureFormat.DXT5:
                case TextureFormat.DXT5Crunched:
	                return QFormat.Q_FORMAT_S3TC_DXT5_RGBA;

                case TextureFormat.RHalf:
	                return QFormat.Q_FORMAT_R_16F;

                case TextureFormat.RGHalf:
	                return QFormat.Q_FORMAT_RG_HF;

                case TextureFormat.RGBAHalf:
	                return QFormat.Q_FORMAT_RGBA_HF;

                case TextureFormat.RFloat:
	                return QFormat.Q_FORMAT_R_F;

                case TextureFormat.RGFloat:
	                return QFormat.Q_FORMAT_RG_F;

                case TextureFormat.RGBAFloat:
	                return QFormat.Q_FORMAT_RGBA_F;
					
                case TextureFormat.RGB9e5Float:
	                return QFormat.Q_FORMAT_RGB9_E5;

                case TextureFormat.ATC_RGB4:
	                return QFormat.Q_FORMAT_ATITC_RGB;

                case TextureFormat.ATC_RGBA8:
	                return QFormat.Q_FORMAT_ATC_RGBA_INTERPOLATED_ALPHA;

                case TextureFormat.EAC_R:
	                return QFormat.Q_FORMAT_EAC_R_UNSIGNED;

                case TextureFormat.EAC_R_SIGNED:
	                return QFormat.Q_FORMAT_EAC_R_SIGNED;

                case TextureFormat.EAC_RG:
	                return QFormat.Q_FORMAT_EAC_RG_UNSIGNED;

                case TextureFormat.EAC_RG_SIGNED:
	                return QFormat.Q_FORMAT_EAC_RG_SIGNED;

				default:
					throw new NotSupportedException(format.ToString());
            }
		}

		private TexgenpackTexturetype ToTexgenpackTexturetype(TextureFormat format)
		{
			switch (format)
			{
				case TextureFormat.BC4:
					return TexgenpackTexturetype.RGTC1;

				case TextureFormat.BC5:
					return TexgenpackTexturetype.RGTC2;

				case TextureFormat.BC6H:
					return TexgenpackTexturetype.BPTC_FLOAT;

				case TextureFormat.BC7:
					return TexgenpackTexturetype.BPTC;

				default:
					throw new NotSupportedException(format.ToString());
			}
		}
	}
}
