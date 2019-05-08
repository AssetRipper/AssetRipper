using System;
using System.IO;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Textures;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
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
		/// 2018.2 and greater
		/// </summary>
		public static bool IsReadStreamingMipmaps(Version version)
		{
			return version.IsGreaterEqual(2018, 2);
		}
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool IsReadStreamingMipmapsPriority(Version version)
		{
			return version.IsGreaterEqual(2018, 2);
		}
		/// <summary>
		/// 4.2.0 and greater and not Release
		/// </summary>
		public static bool IsReadAlphaIsTransparency(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(4, 2);
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

		public static bool IsSwapBytes(Platform platform, TextureFormat format)
		{
			if (platform == Platform.XBox360)
			{
				switch (format)
				{
					case TextureFormat.ARGB4444:
					case TextureFormat.RGB565:
					case TextureFormat.DXT1:
					case TextureFormat.DXT1Crunched:
					case TextureFormat.DXT3:
					case TextureFormat.DXT5:
					case TextureFormat.DXT5Crunched:
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// <para>0 - less than 5.0.0</para>
		/// <para>1 - less than 2018.2</para>
		/// <para>2 - 2018.2 and greater</para>
		/// </summary>
		private static int GetAlphaIsTransparencyPosition(Version version)
		{
			if (version.IsLess(5))
			{
				return 0;
			}
			if (version.IsLess(2018, 2))
			{
				return 1;
			}
			return 2;
		}

		private static int GetSerializedVersion(Version version)
		{
			// MipMap has been converted to MipCount
			if (version.IsGreaterEqual(5, 2))
			{
				return 2;
			}
			return 1;
		}

		public bool CheckAssetIntegrity()
		{
			if (IsReadStreamData(File.Version))
			{
				return StreamData.CheckIntegrity(File);
			}
			return true;
		}

		public IReadOnlyList<byte> GetImageData()
		{
			byte[] data = m_imageData;
			if (IsReadStreamData(File.Version) && StreamData.IsValid)
			{
				data = StreamData.GetContent(File) ?? m_imageData;
			}

			if (IsSwapBytes(File.Platform, TextureFormat))
			{
				for (int i = 0; i < data.Length; i += 2)
				{
					byte b = data[i];
					data[i] = data[i + 1];
					data[i + 1] = b;
				}
			}

			return data;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

#if UNIVERSAL
			if (IsReadAlphaIsTransparency(reader.Version, reader.Flags))
			{
				if (GetAlphaIsTransparencyPosition(reader.Version) == 0)
				{
					AlphaIsTransparency = reader.ReadBoolean();
					reader.AlignStream(AlignType.Align4);
				}
			}
#endif
			Width = reader.ReadInt32();
			Height = reader.ReadInt32();
			CompleteImageSize = reader.ReadInt32();
			TextureFormat = (TextureFormat)reader.ReadInt32();

			if (IsBoolMinMap(reader.Version))
			{
				bool mipMap = reader.ReadBoolean();
				if (mipMap)
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
				MipCount = reader.ReadInt32();
			}

			if (IsReadIsReadable(reader.Version))
			{
				IsReadable = reader.ReadBoolean();
			}
			if (IsReadReadAllowed(reader.Version))
			{
				ReadAllowed = reader.ReadBoolean();
			}
			if (IsReadStreamingMipmaps(reader.Version))
			{
				StreamingMipmaps = reader.ReadBoolean();
			}
#if UNIVERSAL
			if (IsReadAlphaIsTransparency(reader.Version, reader.Flags))
			{
				if (GetAlphaIsTransparencyPosition(reader.Version) == 1)
				{
					AlphaIsTransparency = reader.ReadBoolean();
				}
			}
#endif
			reader.AlignStream(AlignType.Align4);

			if (IsReadStreamingMipmapsPriority(reader.Version))
			{
				StreamingMipmapsPriority = reader.ReadInt32();
#if UNIVERSAL
				if (IsReadAlphaIsTransparency(reader.Version, reader.Flags))
				{
					if (GetAlphaIsTransparencyPosition(reader.Version) == 2)
					{
						AlphaIsTransparency = reader.ReadBoolean();
					}
				}
#endif
				reader.AlignStream(AlignType.Align4);
			}

			ImageCount = reader.ReadInt32();
			TextureDimension = (TextureDimension)reader.ReadInt32();
			TextureSettings.Read(reader);

			if (IsReadLightmapFormat(reader.Version))
			{
				LightmapFormat = (TextureUsageMode)reader.ReadInt32();
			}
			if (IsReadColorSpace(reader.Version))
			{
				ColorSpace = (ColorSpace)reader.ReadInt32();
			}

			m_imageData = reader.ReadByteArray();
			reader.AlignStream(AlignType.Align4);
			if (IsReadStreamData(reader.Version))
			{
				StreamData.Read(reader);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(WidthName, Width);
			node.Add(HeightName, Height);
			node.Add(CompleteImageSizeName, CompleteImageSize);
			node.Add(TextureFormatName, (int)TextureFormat);
			node.Add(MipCountName, MipCount);
			node.Add(IsReadableName, IsReadable);
			if (IsReadStreamingMipmaps(container.ExportVersion))
			{
				node.Add(StreamingMipmapsName, StreamingMipmaps);
			}
			if (IsReadStreamingMipmapsPriority(container.ExportVersion))
			{
				node.Add(StreamingMipmapsPriorityName, StreamingMipmapsPriority);
			}
			node.Add(AlphaIsTransparencyName, GetAlphaIsTransparency(container.Version, container.Flags));
			node.Add(ImageCountName, ImageCount);
			node.Add(TextureDimensionName, (int)TextureDimension);
			node.Add(TextureSettingsName, TextureSettings.ExportYAML(container));
			node.Add(LightmapFormatName, (int)LightmapFormat);
			node.Add(ColorSpaceName, (int)ColorSpace);
			IReadOnlyList<byte> imageData = GetExportImageData();
			node.Add(ImageDataName, imageData.Count);
			node.Add(TypelessdataName, imageData.ExportYAML());
			StreamingInfo streamData = new StreamingInfo(true);
			node.Add(StreamDataName, streamData.ExportYAML(container));
			return node;
		}

		private bool GetAlphaIsTransparency(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			return IsReadAlphaIsTransparency(version, flags) ? AlphaIsTransparency : true;
#else
			return true;
#endif
		}
		private IReadOnlyList<byte> GetExportImageData()
		{
			if (CheckAssetIntegrity())
			{
				return GetImageData();
			}

			Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{ValidName}' because resources file '{StreamData.Path}' wasn't found");
			return new byte[0];
		}

		public bool IsValidData
		{
			get
			{
				if (IsReadStreamData(File.Version))
				{
					string path = StreamData.Path;
					if (path != string.Empty)
					{
						return true;
					}
				}
				return ImageData.Count > 0;
			}
		}

		public int Width { get; private set; }
		public int Height { get; private set; }
		public int CompleteImageSize { get; private set; }
		public TextureFormat TextureFormat { get; private set; }
		public int MipCount { get; private set; }
		public bool IsReadable { get; private set; }
		public bool ReadAllowed { get; private set; }
		public bool StreamingMipmaps { get; private set; }
		public int StreamingMipmapsPriority { get; private set; }
#if UNIVERSAL
		public bool AlphaIsTransparency { get; private set; }
#endif
		public int ImageCount { get; private set; }
		public TextureDimension TextureDimension { get; private set; }
		public TextureUsageMode LightmapFormat { get; private set; }
		public ColorSpace ColorSpace { get; private set; }
		public IReadOnlyCollection<byte> ImageData => m_imageData;

		public const string WidthName = "m_Width";
		public const string HeightName = "m_Height";
		public const string CompleteImageSizeName = "m_CompleteImageSize";
		public const string TextureFormatName = "m_TextureFormat";
		public const string MipCountName = "m_MipCount";
		public const string IsReadableName = "m_IsReadable";
		public const string StreamingMipmapsName = "m_StreamingMipmaps";
		public const string StreamingMipmapsPriorityName = "m_StreamingMipmapsPriority";
		public const string AlphaIsTransparencyName = "m_AlphaIsTransparency";
		public const string ImageCountName = "m_ImageCount";
		public const string TextureDimensionName = "m_TextureDimension";
		public const string TextureSettingsName = "m_TextureSettings";
		public const string LightmapFormatName = "m_LightmapFormat";
		public const string ColorSpaceName = "m_ColorSpace";
		public const string ImageDataName = "image data";
		public const string TypelessdataName = "_typelessdata";
		public const string StreamDataName = "m_StreamData";

		public TextureSettings TextureSettings;
		public StreamingInfo StreamData;

		private byte[] m_imageData;
	}
}
