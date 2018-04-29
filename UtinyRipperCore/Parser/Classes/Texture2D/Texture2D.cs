using System;
using System.IO;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.Textures;
using UtinyRipper.Converter.Textures.DDS;
using UtinyRipper.Converter.Textures.KTX;
using UtinyRipper.Converter.Textures.PVR;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
#warning TODO: exclusive exporter (for meta)
	/// <summary>
	/// FileTexture previously
	/// </summary>
	public class Texture2D : Texture
	{
		public Texture2D(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// Less than 5.2.0
		/// </summary>
		public static bool IsBoolMinMap(Version version)
		{
			return version.IsLess(5, 2);
		}
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool IsReadIsReadable(Version version)
		{
			return version.IsGreaterEqual(2, 6);
		}
		/// <summary>
		/// From 3.0.0 to 5.5.0 exclusive
		/// </summary>
		public static bool IsReadReadAllowed(Version version)
		{
			return version.IsGreaterEqual(3) && version.IsLess(5, 5);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadLightmapFormat(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool IsReadColorSpace(Version version)
		{
			return version.IsGreaterEqual(3, 5);
		}
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadStreamData(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		
		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			Width = stream.ReadInt32();
			Height = stream.ReadInt32();
			CompleteImageSize = stream.ReadInt32();
			TextureFormat = (TextureFormat)stream.ReadInt32();

			if(IsBoolMinMap(stream.Version))
			{
				MipMap = stream.ReadBoolean();
				if(MipMap)
				{
					int maxSide = Math.Max(Width, Height);
					MipCount = Convert.ToInt32(Math.Log(maxSide) / Math.Log(2));
				}
				else
				{
					MipCount = 1;
				}
			}
			else
			{
				MipCount = stream.ReadInt32();
			}

			if(IsReadIsReadable(stream.Version))
			{
				IsReadable = stream.ReadBoolean();
			}
			if(IsReadReadAllowed(stream.Version))
			{
				ReadAllowed = stream.ReadBoolean();
			}
			stream.AlignStream(AlignType.Align4);

			ImageCount = stream.ReadInt32();
			TextureDimension = stream.ReadInt32();
			TextureSettings.Read(stream);

			if(IsReadLightmapFormat(stream.Version))
			{
				LightmapFormat = stream.ReadInt32();
			}
			if(IsReadColorSpace(stream.Version))
			{
				ColorSpace = (ColorSpace)stream.ReadInt32();
			}

			m_imageData = stream.ReadByteArray();
			stream.AlignStream(AlignType.Align4);
			if(IsReadStreamData(stream.Version))
			{
				StreamData.Read(stream);
			}
		}

		public override void ExportBinary(IAssetsExporter exporter, Stream stream)
		{
			if (CompleteImageSize == 0)
			{
				return;
			}

			if (IsReadStreamData(exporter.Version))
			{
				string path = StreamData.Path;
				if (path != string.Empty)
				{
					if (m_imageData.Length != 0)
					{
						throw new Exception("Texture contains data and resource path");
					}
					
					ResourcesFile res = File.Collection.FindResourcesFile(File, path);
					if (res == null)
					{
						Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{Name}' because resources file '{path}' wasn't found");
						return;
					}
					
					res.Stream.Position = StreamData.Offset;
					Export(exporter, stream, res.Stream, StreamData.Size);
					return;
				}
			}

			using (MemoryStream memStream = new MemoryStream(m_imageData))
			{
				Export(exporter, stream, memStream, m_imageData.Length);
			}
		}

		public DDSCapsFlags DDSCaps(Version version)
		{
			if (IsBoolMinMap(version))
			{
				if (!MipMap)
				{
					return DDSCapsFlags.DDSCAPS_TEXTURE;
				}
			}
			return DDSCapsFlags.DDSCAPS_TEXTURE | DDSCapsFlags.DDSCAPS_MIPMAP | DDSCapsFlags.DDSCAPS_COMPLEX;
		}

		public bool IsSwapBytes(Platform platform)
		{
			if (platform == Platform.XBox360)
			{
				switch (TextureFormat)
				{
					case TextureFormat.ARGB4444:
					case TextureFormat.RGB565:
					case TextureFormat.DXT1:
					case TextureFormat.DXT1Crunched:
					case TextureFormat.DXT5:
					case TextureFormat.DXT5Crunched:
						return true;
				}
			}
			return false;
		}

		protected sealed override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			throw new NotSupportedException();
		}

		private void Export(IAssetsExporter exporter, Stream destination, Stream source, long length)
		{
			switch (TextureFormat.ToContainerType())
			{
				case ContainerType.None:
					source.CopyStream(destination, length);
					break;

				case ContainerType.DDS:
					ExportDDS(exporter, destination, source, length);
					break;

				case ContainerType.PVR:
					ExportPVR(destination, source, length);
					break;

				case ContainerType.KTX:
					ExportKTX(destination, source, length);
					break;

				default:
					throw new NotSupportedException($"Unsupported texture container {TextureFormat.ToContainerType()}");
			}
		}

		private void ExportDDS(IAssetsExporter exporter, Stream destination, Stream source, long length)
		{
			DDSConvertParameters @params = new DDSConvertParameters()
			{
				DataLength = length,
				MipMapCount = MipCount,
				Width = Width,
				Height = Height,
				IsPitchOrLinearSize = DDSIsPitchOrLinearSize,
				PixelFormatFlags = DDSPixelFormatFlags,
				FourCC = (DDSFourCCType)DDSFourCC,
				RGBBitCount = DDSRGBBitCount,
				RBitMask = DDSRBitMask,
				GBitMask = DDSGBitMask,
				BBitMask = DDSBBitMask,
				ABitMask = DDSABitMask,
				Caps = DDSCaps(exporter.Version),
			};
			
			if (IsSwapBytes(exporter.Platform))
			{
				using (ReverseStream reverse = new ReverseStream(source, source.Position, length, true))
				{
					if (Config.IsConvertTexturesToPNG)
					{
						DDSConverter.ExportBitmap(destination, reverse, @params);
					}
					else
					{
						DDSConverter.ExportDDS(destination, reverse, @params);
					}
				}
			}
			else
			{
				if (Config.IsConvertTexturesToPNG)
				{
					DDSConverter.ExportBitmap(destination, source, @params);
				}
				else
				{
					DDSConverter.ExportDDS(destination, source, @params);
				}
			}
		}
		
		private void ExportPVR(Stream writer, Stream reader, long length)
		{
			PVRConvertParameters @params = new PVRConvertParameters()
			{
				DataLength = length,
				PixelFormat = PVRPixelFormat,
				Width = Width,
				Height = Height,
				MipMapCount = MipCount,
			};

			if (Config.IsConvertTexturesToPNG)
			{
				throw new NotImplementedException();
			}
			else
			{
				PVRConverter.ExportPVR(writer, reader, @params);
			}
		}

		private void ExportKTX(Stream writer, Stream reader, long length)
		{
			KTXConvertParameters @params = new KTXConvertParameters()
			{
				DataLength = length,
				InternalFormat = KTXInternalFormat,
				BaseInternalFormat = KTXBaseInternalFormat,
				Width = Width,
				Height = Height,
			};
			
			if (Config.IsConvertTexturesToPNG)
			{
				throw new NotImplementedException();
			}
			else
			{
				KTXConverter.ExportKXT(writer, reader, @params);
			}
		}

		public override string ExportExtension
		{
			get
			{
				if(Config.IsConvertTexturesToPNG)
				{
					return "png";
				}
				else
				{
					switch(TextureFormat.ToContainerType())
					{
						case ContainerType.None:
							switch(TextureFormat)
							{
								case TextureFormat.DXT1Crunched:
								case TextureFormat.DXT5Crunched:
								case TextureFormat.ETC_RGB4Crunched:
								case TextureFormat.ETC2_RGBA8Crunched:
									return "crn";
							}
							return "tex";

						case ContainerType.DDS:
							return "dds";

						case ContainerType.PVR:
							return "pvr";

						case ContainerType.KTX:
							return "ktx";

						default:
							throw new NotSupportedException($"Unsupported container type {TextureFormat.ToContainerType()}");
					}
				}
			}
		}
		
		public bool DDSIsPitchOrLinearSize
		{
			get
			{
				if (MipMap)
				{
					switch (TextureFormat)
					{
						case TextureFormat.DXT1:
						case TextureFormat.DXT1Crunched:
						case TextureFormat.DXT5:
						case TextureFormat.DXT5Crunched:
							return true;
					}
				}
				return false;
			}
		}

		public DDPFFlags DDSPixelFormatFlags
		{
			get
			{
				switch (TextureFormat)
				{
					case TextureFormat.Alpha8:
					case TextureFormat.ARGB4444:
					case TextureFormat.RGBA32:
					case TextureFormat.ARGB32:
					case TextureFormat.RGBA4444:
					case TextureFormat.BGRA32:
						return DDPFFlags.DDPF_RGB | DDPFFlags.DDPF_ALPHAPIXELS;

					case TextureFormat.R16:
					case TextureFormat.RGB24:
					case TextureFormat.RGB565:
						return DDPFFlags.DDPF_RGB;

					case TextureFormat.DXT1:
					case TextureFormat.DXT1Crunched:
					case TextureFormat.DXT5:
					case TextureFormat.DXT5Crunched:
						return DDPFFlags.DDPF_FOURCC;

					default:
						throw new NotSupportedException($"Texture format {TextureFormat} isn't supported");
				}
			}
		}

		public uint DDSFourCC
		{
			get
			{
				switch (TextureFormat)
				{
					case TextureFormat.DXT1:
					case TextureFormat.DXT1Crunched:
						// ASCII - 'DXT1'
						return 0x31545844;
					case TextureFormat.DXT5:
					case TextureFormat.DXT5Crunched:
						// ASCII - 'DXT5'
						return 0x35545844;

					default:
						return 0;
				}
			}
		}

		public int DDSRGBBitCount
		{
			get
			{
				switch (TextureFormat)
				{
					case TextureFormat.RGBA32:
						return 32;
					case TextureFormat.ARGB32:
						return 32;
					case TextureFormat.BGRA32:
						return 32;

					case TextureFormat.ARGB4444:
						return 16;
					case TextureFormat.RGBA4444:
						return 16;

					case TextureFormat.RGB24:
						return 24;

					case TextureFormat.Alpha8:
						return 8;

					case TextureFormat.R16:
						return 32;

					case TextureFormat.RGB565:
						return 16;

					case TextureFormat.DXT1:
					case TextureFormat.DXT1Crunched:
					case TextureFormat.DXT5:
					case TextureFormat.DXT5Crunched:
						return 0;

					default:
						throw new NotSupportedException($"Texture format {TextureFormat} isn't supported");
				}
			}
		}

		public uint DDSRBitMask
		{
			get
			{
				switch (TextureFormat)
				{
					case TextureFormat.RGBA32:
						return 0x000000FF;
					case TextureFormat.BGRA32:
						return 0x00FF0000;
					case TextureFormat.ARGB32:
						return 0x0000FF00;

					case TextureFormat.ARGB4444:
						return 0x0F00;
					case TextureFormat.RGBA4444:
						return 0xF000;

					case TextureFormat.RGB24:
						return Config.IsConvertTexturesToPNG ? unchecked((uint)0x0000FF) : 0xFF0000;
						
					case TextureFormat.RGB565:
						return 0xF800;

					case TextureFormat.Alpha8:
						return 0;

					case TextureFormat.R8:
						return 0xFF;
					case TextureFormat.R16:
						return 0xFFFF;
					case TextureFormat.RG16:
						return 0xFF;

					case TextureFormat.DXT1:
					case TextureFormat.DXT1Crunched:
					case TextureFormat.DXT5:
					case TextureFormat.DXT5Crunched:
						return 0;

					default:
						throw new NotSupportedException($"Texture format {TextureFormat} isn't supported");
				}
			}
		}

		public uint DDSGBitMask
		{
			get
			{
				switch (TextureFormat)
				{
					case TextureFormat.RGBA32:
						return 0x0000FF00;
					case TextureFormat.BGRA32:
						return 0x0000FF00;
					case TextureFormat.ARGB32:
						return 0x00FF0000;

					case TextureFormat.ARGB4444:
						return 0x00F0;
					case TextureFormat.RGBA4444:
						return 0x0F00;

					case TextureFormat.RGB24:
						return 0xFF00;

					case TextureFormat.RGB565:
						return 0x07E0;

					case TextureFormat.Alpha8:

						return 0;
					case TextureFormat.R8:
						return 0;
					case TextureFormat.R16:
						return 0;
					case TextureFormat.RG16:
						return 0xFF00;

					case TextureFormat.DXT1:
					case TextureFormat.DXT1Crunched:
					case TextureFormat.DXT5:
					case TextureFormat.DXT5Crunched:
						return 0;

					default:
						throw new NotSupportedException($"Texture format {TextureFormat} isn't supported");
				}
			}
		}

		public uint DDSBBitMask
		{
			get
			{
				switch (TextureFormat)
				{
					case TextureFormat.RGBA32:
						return 0x00FF0000;
					case TextureFormat.BGRA32:
						return 0x000000FF;
					case TextureFormat.ARGB32:
						return 0xFF000000;

					case TextureFormat.ARGB4444:
						return 0x000F;
					case TextureFormat.RGBA4444:
						return 0x00F0;

					case TextureFormat.RGB24:
						return Config.IsConvertTexturesToPNG ? unchecked((uint)0xFF0000) : 0x0000FF;
						
					case TextureFormat.RGB565:
						return 0x001F;

					case TextureFormat.Alpha8:
						return 0;

					case TextureFormat.R8:
						return 0;
					case TextureFormat.R16:
						return 0;
					case TextureFormat.RG16:
						return 0;

					case TextureFormat.DXT1:
					case TextureFormat.DXT1Crunched:
					case TextureFormat.DXT5:
					case TextureFormat.DXT5Crunched:
						return 0;

					default:
						throw new NotSupportedException($"Texture format {TextureFormat} isn't supported");
				}
			}
		}

		public uint DDSABitMask
		{
			get
			{
				switch (TextureFormat)
				{
					case TextureFormat.RGBA32:
						return 0xFF000000;
					case TextureFormat.BGRA32:
						return 0xFF000000;

					case TextureFormat.ARGB32:
						return 0xFF;
					case TextureFormat.Alpha8:
						return 0xFF;

					case TextureFormat.ARGB4444:
						return 0xF000;
					case TextureFormat.RGBA4444:
						return 0x000F;

					case TextureFormat.RGB24:
						return 0x0;
						
					case TextureFormat.R8:
						return 0;
					case TextureFormat.R16:
						return 0;
					case TextureFormat.RGB565:
						return 0;
					case TextureFormat.RG16:
						return 0;
					case TextureFormat.DXT1:
					case TextureFormat.DXT1Crunched:
					case TextureFormat.DXT5:
					case TextureFormat.DXT5Crunched:
						return 0;

					default:
						throw new NotSupportedException($"Texture format {TextureFormat} isn't supported");
				}
			}
		}

		public KTXInternalFormat KTXInternalFormat
		{
			get
			{
				switch (TextureFormat)
				{
					case TextureFormat.RHalf:
						return KTXInternalFormat.R16F;

					case TextureFormat.RGHalf:
						return KTXInternalFormat.RG16F;

					case TextureFormat.RGBAHalf:
						return KTXInternalFormat.RGBA16F;

					case TextureFormat.RFloat:
						return KTXInternalFormat.R32F;

					case TextureFormat.RGFloat:
						return KTXInternalFormat.RG32F;

					case TextureFormat.RGBAFloat:
						return KTXInternalFormat.RGBA32F;

					case TextureFormat.BC4:
						return KTXInternalFormat.COMPRESSED_RED_RGTC1;

					case TextureFormat.BC5:
						return KTXInternalFormat.COMPRESSED_RG_RGTC2;

					case TextureFormat.BC6H:
						return KTXInternalFormat.COMPRESSED_RGB_BPTC_UNSIGNED_FLOAT;

					case TextureFormat.BC7:
						return KTXInternalFormat.COMPRESSED_RGBA_BPTC_UNORM;

					case TextureFormat.PVRTC_RGB2:
						return KTXInternalFormat.COMPRESSED_RGB_PVRTC_2BPPV1_IMG;

					case TextureFormat.PVRTC_RGBA2:
						return KTXInternalFormat.COMPRESSED_RGBA_PVRTC_2BPPV1_IMG;

					case TextureFormat.PVRTC_RGB4:
						return KTXInternalFormat.COMPRESSED_RGB_PVRTC_4BPPV1_IMG;

					case TextureFormat.PVRTC_RGBA4:
						return KTXInternalFormat.COMPRESSED_RGBA_PVRTC_4BPPV1_IMG;

					case TextureFormat.ETC_RGB4Crunched:
					case TextureFormat.ETC_RGB4_3DS:
					case TextureFormat.ETC_RGB4:
						return KTXInternalFormat.ETC1_RGB8_OES;

					case TextureFormat.ATC_RGB4:
						return KTXInternalFormat.ATC_RGB_AMD;

					case TextureFormat.ATC_RGBA8:
						return KTXInternalFormat.ATC_RGBA_INTERPOLATED_ALPHA_AMD;

					case TextureFormat.EAC_R:
						return KTXInternalFormat.COMPRESSED_R11_EAC;

					case TextureFormat.EAC_R_SIGNED:
						return KTXInternalFormat.COMPRESSED_SIGNED_R11_EAC;

					case TextureFormat.EAC_RG:
						return KTXInternalFormat.COMPRESSED_RG11_EAC;

					case TextureFormat.EAC_RG_SIGNED:
						return KTXInternalFormat.COMPRESSED_SIGNED_RG11_EAC;

					case TextureFormat.ETC2_RGB:
						return KTXInternalFormat.COMPRESSED_RGB8_ETC2;

					case TextureFormat.ETC2_RGBA1:
						return KTXInternalFormat.COMPRESSED_RGB8_PUNCHTHROUGH_ALPHA1_ETC2;

					case TextureFormat.ETC2_RGBA8Crunched:
					case TextureFormat.ETC_RGBA8_3DS:
					case TextureFormat.ETC2_RGBA8:
						return KTXInternalFormat.COMPRESSED_RGBA8_ETC2_EAC;

					default:
						throw new NotSupportedException();
				}
			}
		}

		public KTXBaseInternalFormat KTXBaseInternalFormat
		{
			get
			{
				switch (TextureFormat)
				{
					case TextureFormat.RHalf:
					case TextureFormat.RFloat:
					case TextureFormat.BC4:
					case TextureFormat.EAC_R:
					case TextureFormat.EAC_R_SIGNED:
						return KTXBaseInternalFormat.RED;

					case TextureFormat.RGHalf:
					case TextureFormat.RGFloat:
					case TextureFormat.BC5:
					case TextureFormat.EAC_RG:
					case TextureFormat.EAC_RG_SIGNED:
						return KTXBaseInternalFormat.RG;

					case TextureFormat.BC6H:
					case TextureFormat.PVRTC_RGB2:
					case TextureFormat.PVRTC_RGB4:
					case TextureFormat.ETC_RGB4Crunched:
					case TextureFormat.ETC_RGB4_3DS:
					case TextureFormat.ETC_RGB4:
					case TextureFormat.ATC_RGB4:
					case TextureFormat.ETC2_RGB:
						return KTXBaseInternalFormat.RGB;

					case TextureFormat.RGBAHalf:
					case TextureFormat.RGBAFloat:
					case TextureFormat.BC7:
					case TextureFormat.PVRTC_RGBA2:
					case TextureFormat.PVRTC_RGBA4:
					case TextureFormat.ATC_RGBA8:
					case TextureFormat.ETC2_RGBA8Crunched:
					case TextureFormat.ETC_RGBA8_3DS:
					case TextureFormat.ETC2_RGBA8:
					case TextureFormat.ETC2_RGBA1:
						return KTXBaseInternalFormat.RGBA;

					default:
						throw new NotSupportedException();
				}
			}
		}

		public PVRPixelFormat PVRPixelFormat
		{
			get
			{
				switch (TextureFormat)
				{
					case TextureFormat.YUY2:
						return PVRPixelFormat.YUY2;

					case TextureFormat.PVRTC_RGB2:
						return PVRPixelFormat.PVRTC2bppRGB;

					case TextureFormat.PVRTC_RGBA2:
						return PVRPixelFormat.PVRTC2bppRGBA;

					case TextureFormat.PVRTC_RGB4:
						return PVRPixelFormat.PVRTC4bppRGB;

					case TextureFormat.PVRTC_RGBA4:
						return PVRPixelFormat.PVRTC4bppRGBA;

					case TextureFormat.ETC_RGB4Crunched:
					case TextureFormat.ETC_RGB4_3DS:
					case TextureFormat.ETC_RGB4:
						return PVRPixelFormat.ETC1;

					case TextureFormat.ETC2_RGB:
						return PVRPixelFormat.ETC2RGB;

					case TextureFormat.ETC2_RGBA1:
						return PVRPixelFormat.ETC2RGBA1;

					case TextureFormat.ETC2_RGBA8Crunched:
					case TextureFormat.ETC_RGBA8_3DS:
					case TextureFormat.ETC2_RGBA8:
						return PVRPixelFormat.ETC2RGBA;

					case TextureFormat.ASTC_RGB_4x4:
					case TextureFormat.ASTC_RGBA_4x4:
						return PVRPixelFormat.ASTC_4x4;

					case TextureFormat.ASTC_RGB_5x5:
					case TextureFormat.ASTC_RGBA_5x5:
						return PVRPixelFormat.ASTC_5x5;

					case TextureFormat.ASTC_RGB_6x6:
					case TextureFormat.ASTC_RGBA_6x6:
						return PVRPixelFormat.ASTC_6x6;

					case TextureFormat.ASTC_RGB_8x8:
					case TextureFormat.ASTC_RGBA_8x8:
						return PVRPixelFormat.ASTC_8x8;

					case TextureFormat.ASTC_RGB_10x10:
					case TextureFormat.ASTC_RGBA_10x10:
						return PVRPixelFormat.ASTC_10x10;

					case TextureFormat.ASTC_RGB_12x12:
					case TextureFormat.ASTC_RGBA_12x12:
						return PVRPixelFormat.ASTC_12x12;

					default:
						throw new NotSupportedException();
				}
			}
		}

		public int Width { get; private set; }
		public int Height { get; private set; }
		public int CompleteImageSize { get; private set; }
		public TextureFormat TextureFormat { get; private set; }
		public int MipCount { get; private set; }
		public bool MipMap { get; private set; }
		public bool IsReadable { get; private set; }
		public bool ReadAllowed { get; private set; }
		public int ImageCount { get; private set; }
		/// <summary>
		/// TextureDimension enum has beed changed at least one time so it's impossible to cast to enum directly
		/// </summary>
		public int TextureDimension { get; private set; }
		public int LightmapFormat { get; private set; }
		public ColorSpace ColorSpace { get; private set; }
		public IReadOnlyCollection<byte> ImageData => m_imageData;

		public TextureSettings TextureSettings;
		public StreamingInfo StreamData;

		private byte[] m_imageData;
	}
}
