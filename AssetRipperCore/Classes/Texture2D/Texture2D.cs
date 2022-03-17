using AssetRipper.Core.Classes.Meta.Importers;
using AssetRipper.Core.Classes.Meta.Importers.Texture;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Converters.Texture2D;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System;
using UnityVersion = AssetRipper.Core.Parser.Files.UnityVersion;

namespace AssetRipper.Core.Classes.Texture2D
{
	/// <summary>
	/// FileTexture previously
	/// </summary>
	public class Texture2D : Texture, ITexture2D
	{
		public Texture2D(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			// MipMap has been converted to MipCount
			if (version.IsGreaterEqual(5, 2))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 2020 and greater
		/// </summary>
		public static bool HasUnsignedCompleteImageSize(UnityVersion version) => version.IsGreaterEqual(2020);
		/// <summary>
		/// 2020 and greater
		/// </summary>
		public static bool HasMipsStripped(UnityVersion version) => version.IsGreaterEqual(2020);
		/// <summary>
		/// Less than 5.2.0
		/// </summary>
		public static bool IsBoolMinMap(UnityVersion version) => version.IsLess(5, 2);
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool HasReadable(UnityVersion version) => version.IsGreaterEqual(2, 6);
		/// <summary>
		/// 2019.3.1 and greater
		/// </summary>
		public static bool HasIgnoreMasterTextureLimit(UnityVersion version) => version.IsGreaterEqual(2019, 3, 1);
		/// <summary>
		/// 2019.4.9 and greater
		/// </summary>
		public static bool HasIsPreProcessed(UnityVersion version) => version.IsGreaterEqual(2019, 4, 9);
		/// <summary>
		/// From 3.0.0 to 5.5.0 exclusive
		/// </summary>
		public static bool HasReadAllowed(UnityVersion version) => version.IsGreaterEqual(3) && version.IsLess(5, 5);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasStreamingMipmaps(UnityVersion version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasStreamingMipmapsPriority(UnityVersion version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 4.2.0 and greater and not Release
		/// </summary>
		public static bool HasAlphaIsTransparency(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(4, 2);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasLightmapFormat(UnityVersion version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasColorSpace(UnityVersion version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 2020.2 and greater
		/// </summary>
		public static bool HasPlatformBlob(UnityVersion version) => version.IsGreaterEqual(2020, 2);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasStreamData(UnityVersion version) => version.IsGreaterEqual(5, 3);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Width = reader.ReadInt32();
			Height = reader.ReadInt32();
			CompleteImageSize = HasUnsignedCompleteImageSize(reader.Version) ? reader.ReadUInt32() : reader.ReadInt32();
			if (HasMipsStripped(reader.Version))
			{
				var m_MipsStripped = reader.ReadInt32();
			}
			TextureFormat = (TextureFormat)reader.ReadInt32();

			if (IsBoolMinMap(reader.Version))
			{
				bool mipMap = reader.ReadBoolean();
				if (mipMap)
				{
					int maxSide = System.Math.Max(Width, Height);
					MipCount = System.Convert.ToInt32(System.Math.Log(maxSide) / System.Math.Log(2));
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
			//NOTE: Original code by Mafaca claims IgnoreMasterTextureLimit comes first, and claims this field is present on 2019.4.9+
			//AssetStudio, and a structure dump from 2020.1, show it is in this order.
			//If this breaks, at all, check if it's different for some reason between 2019.4.9 and 2020 full release.
			if (HasIsPreProcessed(reader.Version))
			{
				IsPreProcessed = reader.ReadBoolean();
			}
			if (HasIgnoreMasterTextureLimit(reader.Version))
			{
				IgnoreMasterTextureLimit = reader.ReadBoolean();
			}
			if (HasReadAllowed(reader.Version))
			{
				ReadAllowed = reader.ReadBoolean();
			}
			if (HasStreamingMipmaps(reader.Version))
			{
				StreamingMipmaps = reader.ReadBoolean();
			}
			reader.AlignStream();

			if (HasStreamingMipmapsPriority(reader.Version))
			{
				StreamingMipmapsPriority = reader.ReadInt32();

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
			if (HasPlatformBlob(reader.Version))
			{
				var m_PlatformBlob = reader.ReadByteArray();
			}

			ImageData = reader.ReadByteArray();
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

			if (HasUnsignedCompleteImageSize(container.ExportVersion))
			{
				node.Add(CompleteImageSizeName, (uint)CompleteImageSize);
			}
			else
			{
				node.Add(CompleteImageSizeName, (int)CompleteImageSize);
			}

			if (HasMipsStripped(container.ExportVersion))
			{
				node.Add(MipsStrippedName, MipsStripped);
			}
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
			node.Add(AlphaIsTransparencyName, AlphaIsTransparency);
			node.Add(ImageCountName, ImageCount);
			node.Add(TextureDimensionName, (int)TextureDimension);
			node.Add(TextureSettingsName, TextureSettings.ExportYAML(container));
			node.Add(LightmapFormatName, (int)LightmapFormat);
			node.Add(ColorSpaceName, (int)ColorSpace);
			byte[] imageData = GetExportImageData();
			node.Add(ImageDataName, imageData.Length);
			node.Add(Layout.LayoutInfo.TypelessdataName, imageData.ExportYAML());
			StreamingInfo streamData = new StreamingInfo();
			node.Add(StreamDataName, streamData.ExportYAML(container));
			return node;
		}

		private byte[] GetExportImageData()
		{
			if (this.CheckAssetIntegrity())
			{
				return this.GetImageData();
			}

			Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{this.GetValidName()}' because resources file '{StreamData.Path}' wasn't found");
			return Array.Empty<byte>();
		}

		public bool IsValidData
		{
			get
			{
				if (HasStreamData(SerializedFile.Version))
				{
					if (StreamData.IsSet())
					{
						return true;
					}
				}
				return ImageData.Length > 0;
			}
		}

		public int Width { get; set; }
		public int Height { get; set; }
		public long CompleteImageSize { get; set; }
		public int MipsStripped { get; set; }
		public TextureFormat TextureFormat { get; set; }
		public int MipCount { get; set; }
		public bool IsReadable { get; set; }
		public bool IgnoreMasterTextureLimit { get; set; }
		public bool IsPreProcessed { get; set; }
		public bool ReadAllowed { get; set; }
		public bool StreamingMipmaps { get; set; }
		public int StreamingMipmapsPriority { get; set; }
		public bool AlphaIsTransparency { get; set; } = true;
		public int ImageCount { get; set; }
		public TextureDimension TextureDimension { get; set; }
		public TextureUsageMode LightmapFormat { get; set; }
		public ColorSpace ColorSpace { get; set; }
		public byte[] PlatformBlob { get; set; }
		public byte[] ImageData { get; set; }
		public IStreamingInfo StreamData { get; } = new StreamingInfo();
		public IGLTextureSettings TextureSettings { get; } = new GLTextureSettings();

		public const string Texture2DName = "Texture2D";
		public const string WidthName = "m_Width";
		public const string HeightName = "m_Height";
		public const string CompleteImageSizeName = "m_CompleteImageSize";
		public const string MipsStrippedName = "m_MipsStripped";
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
		public const string PlatformBlobName = "m_PlatformBlob";
		public const string ImageDataName = "image data";
		public const string StreamDataName = "m_StreamData";
	}
}
