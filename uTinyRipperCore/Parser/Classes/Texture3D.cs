using System;
using System.Collections.Generic;
using uTinyRipper.Classes.Textures;
using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper.Classes;
using uTinyRipper.Classes.Misc;

namespace uTinyRipper.Classes
{
	public sealed class Texture3D : Texture
	{
		public Texture3D(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
		{
			// ColorSpace has been added which affects on Format
			if (version.IsGreaterEqual(2019))
			{
				return 3;
			}
			// MipMap converted to MipCount
			if (version.IsGreaterEqual(5, 2))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasDepth(Version version) => version.IsGreaterEqual(4);
		/// <summary>
		/// 2.6.0 to 4.0.0 exclusize or 5.4.0 and greater
		/// </summary>
		public static bool HasIsReadable(Version version) => IsReadableFirst(version) || IsReadableSecond(version);
		/// <summary>
		/// From 3.0.0 to 4.0.0 exclusive
		/// </summary>
		public static bool HasReadAllowed(Version version) => version.IsGreaterEqual(3) && version.IsLess(4);
		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		public static bool HasImageCount(Version version) => version.IsLess(4);
		/// <summary>
		/// 3.0.0 to 4.0.0 exclusive
		/// </summary>
		public static bool HasLightmapFormat(Version version) => version.IsGreaterEqual(3) && version.IsLess(4);
		/// <summary>
		/// 3.5.0 to 4.0.0 exclusive or 2019.1 and greater
		/// </summary>
		public static bool HasColorSpace(Version version)
		{
			return version.IsGreaterEqual(2019) || version.IsGreaterEqual(3, 5) && version.IsLess(4);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasStreamData(Version version) => version.IsGreaterEqual(5, 6);

		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		private static bool IsColorSpaceFirst(Version version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		private static bool IsFormatFirst(Version version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		private static bool IsDataSizeFirst(Version version) => version.IsLess(4);
		/// <summary>
		/// Less than 5.2.0
		/// </summary>
		private static bool IsBoolMinMap(Version version) => version.IsLess(5, 2);
		/// <summary>
		/// 2.6.0 to 4.0.0 exclusize
		/// </summary>
		private static bool IsReadableFirst(Version version) => version.IsGreaterEqual(2, 6) && version.IsLess(4);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		private static bool IsReadableSecond(Version version) => version.IsGreaterEqual(5, 4);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasColorSpace(reader.Version))
			{
				if (IsColorSpaceFirst(reader.Version))
				{
					ColorSpace = (ColorSpace)reader.ReadInt32();
				}
			}
			if (IsFormatFirst(reader.Version))
			{
				Format = (TextureFormat)reader.ReadInt32();
			}

			Width = reader.ReadInt32();
			Height = reader.ReadInt32();
			if (HasDepth(reader.Version))
			{
				Depth = reader.ReadInt32();
			}
			if (IsDataSizeFirst(reader.Version))
			{
				DataSize = reader.ReadInt32();
			}
			if (!IsFormatFirst(reader.Version))
			{
				Format = (TextureFormat)reader.ReadInt32();
			}

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

			if (!IsDataSizeFirst(reader.Version))
			{
				DataSize = reader.ReadInt32();
			}
			if (IsReadableFirst(reader.Version))
			{
				IsReadable = reader.ReadBoolean();
			}
			if (HasReadAllowed(reader.Version))
			{
				ReadAllowed = reader.ReadBoolean();
			}
			reader.AlignStream();

			if (HasImageCount(reader.Version))
			{
				ImageCount = reader.ReadInt32();
				TextureDimension = (TextureDimension)reader.ReadInt32();
			}
			TextureSettings.Read(reader);

			if (IsReadableSecond(reader.Version))
			{
				IsReadable = reader.ReadBoolean();
				reader.AlignStream();
			}
			if (HasLightmapFormat(reader.Version))
			{
				LightmapFormat = (TextureUsageMode)reader.ReadInt32();
			}
			if (HasColorSpace(reader.Version))
			{
				if (!IsColorSpaceFirst(reader.Version))
				{
					ColorSpace = (ColorSpace)reader.ReadInt32();
				}
			}

			m_imageData = reader.ReadByteArray();
			reader.AlignStream();
			if (HasStreamData(reader.Version))
			{
				StreamData.Read(reader);
			}
		}

		protected sealed override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			if (HasColorSpace(container.ExportVersion))
			{
				node.Add(ColorSpaceName, (int)ColorSpace);
			}
			node.Add(FormatName, (int)Format);
			node.Add(WidthName, Width);
			node.Add(HeightName, Height);
			node.Add(DepthName, Depth);
			node.Add(MipCountName, MipCount);
			node.Add(DataSizeName, DataSize);
			node.Add(TextureSettingsName, TextureSettings.ExportYAML(container));
			node.Add(IsReadableName, IsReadable);
			byte[] imageData = GetImageData(container.Version);
			node.Add(ImageDataName, imageData.Length);
			node.Add(container.Layout.TypelessdataName, imageData.ExportYAML());
			StreamingInfo streamData = new StreamingInfo(true);
			node.Add(StreamDataName, streamData.ExportYAML(container));
			return node;
		}

		private byte[] GetImageData(Version version)
		{
			if (HasStreamData(version) && StreamData.IsSet)
			{
				byte[] data = StreamData.GetContent(File);
				if (data == null)
				{
					Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{ValidName}' because resources file '{StreamData.Path}' wasn't found");
					return m_imageData;
				}
				return data;
			}

			return m_imageData;
		}

		public ColorSpace ColorSpace { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		/// <summary>
		/// CompleteImageSize previously
		/// </summary>
		public int DataSize { get; set; }
		public bool IsReadable { get; set; }
		public bool ReadAllowed { get; set; }
		public int Depth { get; set; }
		/// <summary>
		/// TextureFormat previously
		/// </summary>
		public TextureFormat Format { get; set; }
		/// <summary>
		/// bool MipMap previously
		/// </summary>
		public int MipCount { get; set; }
		public int ImageCount { get; set; }
		public TextureDimension TextureDimension { get; set; }
		public TextureUsageMode LightmapFormat { get; set; }
		public IReadOnlyCollection<byte> ImageData => m_imageData;

		public const string ColorSpaceName = "m_ColorSpace";
		public const string FormatName = "m_Format";
		public const string WidthName = "m_Width";
		public const string HeightName = "m_Height";
		public const string DepthName = "m_Depth";
		public const string CompleteImageSizeName = "m_CompleteImageSize";
		public const string TextureFormatName = "m_TextureFormat";
		public const string MipMapName = "m_MipMap";
		public const string MipCountName = "m_MipCount";
		public const string IsReadableName = "m_IsReadable";
		public const string DataSizeName = "m_DataSize";
		public const string ImageCountName = "m_ImageCount";
		public const string TextureDimensionName = "m_TextureDimension";
		public const string TextureSettingsName = "m_TextureSettings";
		public const string ImageDataName = "image data";
		public const string StreamDataName = "m_StreamData";

		public GLTextureSettings TextureSettings;
		public StreamingInfo StreamData;

		private byte[] m_imageData;
	}
}
