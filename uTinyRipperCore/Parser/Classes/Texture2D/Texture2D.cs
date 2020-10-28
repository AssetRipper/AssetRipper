using System;
using System.Collections.Generic;
using uTinyRipper.Classes.Textures;
using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper.Classes;
using uTinyRipper.Classes.Misc;

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

		public static int ToSerializedVersion(Version version)
		{
			// MipMap has been converted to MipCount
			if (version.IsGreaterEqual(5, 2))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// Less than 5.2.0
		/// </summary>
		public static bool IsBoolMinMap(Version version) => version.IsLess(5, 2);
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool HasReadable(Version version) => version.IsGreaterEqual(2, 6);
		/// <summary>
		/// 2019.3.1 and greater
		/// </summary>
		public static bool HasIgnoreMasterTextureLimit(Version version) => version.IsGreaterEqual(2019, 3, 1);
		/// <summary>
		/// 2019.4.9 and greater
		/// </summary>
		public static bool HasIsPreProcessed(Version version) => version.IsGreaterEqual(2019, 4, 9);
		/// <summary>
		/// From 3.0.0 to 5.5.0 exclusive
		/// </summary>
		public static bool HasReadAllowed(Version version) => version.IsGreaterEqual(3) && version.IsLess(5, 5);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasStreamingMipmaps(Version version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasStreamingMipmapsPriority(Version version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 4.2.0 and greater and not Release
		/// </summary>
		public static bool HasAlphaIsTransparency(Version version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(4, 2);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasLightmapFormat(Version version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasColorSpace(Version version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasStreamData(Version version) => version.IsGreaterEqual(5, 3);

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

#if UNIVERSAL
		/// <summary>
		/// <para>0 - less than 5.0.0</para>
		/// <para>1 - less than 2018.2</para>
		/// <para>2 - 2018.2 and greater</para>
		/// </summary>
		private static int GetAlphaIsTransparencyOrder(Version version)
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
#endif

		public virtual TextureImporter GenerateTextureImporter(IExportContainer container)
		{
			return Texture2DConverter.GenerateTextureImporter(container, this);
		}

		public virtual IHVImageFormatImporter GenerateIHVImporter(IExportContainer container)
		{
			return Texture2DConverter.GenerateIHVImporter(container, this);
		}

		public bool CheckAssetIntegrity()
		{
			if (HasStreamData(File.Version))
			{
				return StreamData.CheckIntegrity(File);
			}
			return true;
		}

		public byte[] GetImageData()
		{
			byte[] data = m_imageData;
			if (HasStreamData(File.Version) && StreamData.IsSet)
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
			bool hasAlphaIsTransparency = HasAlphaIsTransparency(reader.Version, reader.Flags);
			int alphaIsTransparencyOrder = GetAlphaIsTransparencyOrder(reader.Version);
			if (hasAlphaIsTransparency && alphaIsTransparencyOrder == 0)
			{
				AlphaIsTransparency = reader.ReadBoolean();
				reader.AlignStream();
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
					MipCount = System.Convert.ToInt32(Math.Log(maxSide) / Math.Log(2));
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

			if (HasReadable(reader.Version))
			{
				IsReadable = reader.ReadBoolean();
			}
			if (HasIgnoreMasterTextureLimit(reader.Version))
			{
				IgnoreMasterTextureLimit = reader.ReadBoolean();
			}
			if (HasIsPreProcessed(reader.Version))
			{
				IsPreProcessed = reader.ReadBoolean();
			}
			if (HasReadAllowed(reader.Version))
			{
				ReadAllowed = reader.ReadBoolean();
			}
			if (HasStreamingMipmaps(reader.Version))
			{
				StreamingMipmaps = reader.ReadBoolean();
			}
#if UNIVERSAL
			if (hasAlphaIsTransparency && alphaIsTransparencyOrder == 1)
			{
				AlphaIsTransparency = reader.ReadBoolean();
			}
#endif
			reader.AlignStream();

			if (HasStreamingMipmapsPriority(reader.Version))
			{
				StreamingMipmapsPriority = reader.ReadInt32();
#if UNIVERSAL
				if (hasAlphaIsTransparency && alphaIsTransparencyOrder == 2)
				{
					AlphaIsTransparency = reader.ReadBoolean();
				}
#endif
				reader.AlignStream();
			}

			ImageCount = reader.ReadInt32();
			TextureDimension = (TextureDimension)reader.ReadInt32();
			TextureSettings.Read(reader);

			if (HasLightmapFormat(reader.Version))
			{
				LightmapFormat = (TextureUsageMode)reader.ReadInt32();
			}
			if (HasColorSpace(reader.Version))
			{
				ColorSpace = (ColorSpace)reader.ReadInt32();
			}

			m_imageData = reader.ReadByteArray();
			reader.AlignStream();
			if (HasStreamData(reader.Version))
			{
				StreamData.Read(reader);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(WidthName, Width);
			node.Add(HeightName, Height);
			node.Add(CompleteImageSizeName, CompleteImageSize);
			node.Add(TextureFormatName, (int)TextureFormat);
			node.Add(MipCountName, MipCount);
			node.Add(IsReadableName, IsReadable);
			if (HasIgnoreMasterTextureLimit(container.ExportVersion))
			{
				node.Add(IgnoreMasterTextureLimitName, IgnoreMasterTextureLimit);
			}
			if (HasIsPreProcessed(container.ExportVersion))
			{
				node.Add(IsPreProcessedName, IsPreProcessed);
			}
			if (HasStreamingMipmaps(container.ExportVersion))
			{
				node.Add(StreamingMipmapsName, StreamingMipmaps);
			}
			if (HasStreamingMipmapsPriority(container.ExportVersion))
			{
				node.Add(StreamingMipmapsPriorityName, StreamingMipmapsPriority);
			}
			node.Add(AlphaIsTransparencyName, GetAlphaIsTransparency(container.Version, container.Flags));
			node.Add(ImageCountName, ImageCount);
			node.Add(TextureDimensionName, (int)TextureDimension);
			node.Add(TextureSettingsName, TextureSettings.ExportYAML(container));
			node.Add(LightmapFormatName, (int)LightmapFormat);
			node.Add(ColorSpaceName, (int)ColorSpace);
			byte[] imageData = GetExportImageData();
			node.Add(ImageDataName, imageData.Length);
			node.Add(container.Layout.TypelessdataName, imageData.ExportYAML());
			StreamingInfo streamData = new StreamingInfo(true);
			node.Add(StreamDataName, streamData.ExportYAML(container));
			return node;
		}

		private bool GetAlphaIsTransparency(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			return HasAlphaIsTransparency(version, flags) ? AlphaIsTransparency : true;
#else
			return true;
#endif
		}
		private byte[] GetExportImageData()
		{
			if (CheckAssetIntegrity())
			{
				return GetImageData();
			}

			Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{ValidName}' because resources file '{StreamData.Path}' wasn't found");
			return Array.Empty<byte>();
		}

		public bool IsValidData
		{
			get
			{
				if (HasStreamData(File.Version))
				{
					if (StreamData.IsSet)
					{
						return true;
					}
				}
				return ImageData.Count > 0;
			}
		}

		public int Width { get; set; }
		public int Height { get; set; }
		public int CompleteImageSize { get; set; }
		public TextureFormat TextureFormat { get; set; }
		public int MipCount { get; set; }
		public bool IsReadable { get; set; }
		public bool IgnoreMasterTextureLimit { get; set; }
		public bool IsPreProcessed { get; set; }
		public bool ReadAllowed { get; set; }
		public bool StreamingMipmaps { get; set; }
		public int StreamingMipmapsPriority { get; set; }
#if UNIVERSAL
		public bool AlphaIsTransparency { get; set; }
#endif
		public int ImageCount { get; set; }
		public TextureDimension TextureDimension { get; set; }
		public TextureUsageMode LightmapFormat { get; set; }
		public ColorSpace ColorSpace { get; set; }
		public IReadOnlyCollection<byte> ImageData => m_imageData;

		public const string WidthName = "m_Width";
		public const string HeightName = "m_Height";
		public const string CompleteImageSizeName = "m_CompleteImageSize";
		public const string TextureFormatName = "m_TextureFormat";
		public const string MipCountName = "m_MipCount";
		public const string IsReadableName = "m_IsReadable";
		public const string IgnoreMasterTextureLimitName = "m_IgnoreMasterTextureLimit";
		public const string IsPreProcessedName = "m_IsPreProcessed";
		public const string StreamingMipmapsName = "m_StreamingMipmaps";
		public const string StreamingMipmapsPriorityName = "m_StreamingMipmapsPriority";
		public const string AlphaIsTransparencyName = "m_AlphaIsTransparency";
		public const string ImageCountName = "m_ImageCount";
		public const string TextureDimensionName = "m_TextureDimension";
		public const string TextureSettingsName = "m_TextureSettings";
		public const string LightmapFormatName = "m_LightmapFormat";
		public const string ColorSpaceName = "m_ColorSpace";
		public const string ImageDataName = "image data";
		public const string StreamDataName = "m_StreamData";

		public GLTextureSettings TextureSettings;
		public StreamingInfo StreamData;

		private byte[] m_imageData;
	}
}
