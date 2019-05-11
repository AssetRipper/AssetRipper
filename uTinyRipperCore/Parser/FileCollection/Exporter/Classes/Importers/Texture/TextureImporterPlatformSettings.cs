using uTinyRipper.Classes;
using uTinyRipper.Classes.Textures;
using uTinyRipper.YAML;

namespace uTinyRipper.AssetExporters.Classes
{
	public class TextureImporterPlatformSettings : IAssetReadable, IYAMLExportable
	{
		public TextureImporterPlatformSettings()
		{
		}

		public TextureImporterPlatformSettings(TextureFormat textureFormat)
		{
			BuildTarget = DefaultTexturePlatformName;
			MaxTextureSize = 2048;
			ResizeAlgorithm = TextureResizeAlgorithm.Mitchell;
			TextureFormat = textureFormat;
			TextureCompression = TextureImporterCompression.Uncompressed;
			CompressionQuality = 50;
			CrunchedCompression = false;
			AllowsAlphaSplitting = false;
			Overridden = false;
			AndroidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings;
			ForceMaximumCompressionQuality_BC6H_BC7 = true;
		}

		/// <summary>
		/// 2019.2 and greater
		/// </summary>
		public static bool IsReadForceMaximumCompressionQuality_BC6H_BC7(Version version)
		{
			return version.IsGreaterEqual(2019, 2);
		}

		private static int GetSerializedVersion(Version version)
		{
			// ForceMaximumCompressionQuality_BC6H_BC7 default value has been changed from 1 to 0
			if (version.IsGreaterEqual(2019, 2))
			{
				return 3;
			}
			// TextureFormat.ATC_RGB4/ATC_RGBA8 was replaced to ETC_RGB4/ETC2_RGBA8
			if (version.IsGreaterEqual(2018))
			{
				return 2;
			}
			return 1;
		}

		public void Read(AssetReader reader)
		{
			BuildTarget = reader.ReadString();
			MaxTextureSize = reader.ReadInt32();
			ResizeAlgorithm = (TextureResizeAlgorithm)reader.ReadInt32();
			TextureFormat = (TextureFormat)reader.ReadInt32();
			TextureCompression = (TextureImporterCompression)reader.ReadInt32();
			CompressionQuality = reader.ReadInt32();
			CrunchedCompression = reader.ReadBoolean();
			AllowsAlphaSplitting = reader.ReadBoolean();
			Overridden = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);

			AndroidETC2FallbackOverride = (AndroidETC2FallbackOverride)reader.ReadInt32();
			if (IsReadForceMaximumCompressionQuality_BC6H_BC7(reader.Version))
			{
				ForceMaximumCompressionQuality_BC6H_BC7 = reader.ReadBoolean();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(BuildTargetName, BuildTarget);
			node.Add(MaxTextureSizeName, MaxTextureSize);
			node.Add(ResizeAlgorithmName, (int)ResizeAlgorithm);
			node.Add(TextureFormatName, (int)GetTextureFormat(container.Version));
			node.Add(TextureCompressionName, (int)TextureCompression);
			node.Add(CompressionQualityName, GetCompressionQuality(container.Version, container.ExportVersion));
			node.Add(CrunchedCompressionName, CrunchedCompression);
			node.Add(AllowsAlphaSplittingName, AllowsAlphaSplitting);
			node.Add(OverriddenName, Overridden);
			node.Add(AndroidETC2FallbackOverrideName, (int)AndroidETC2FallbackOverride);
			if (IsReadForceMaximumCompressionQuality_BC6H_BC7(container.ExportVersion))
			{
				node.Add(ForceMaximumCompressionQuality_BC6H_BC7Name, GetForceMaximumCompressionQuality_BC6H_BC7(container.Version));
			}
			return node;
		}

		private TextureFormat GetTextureFormat(Version version)
		{
			if (GetSerializedVersion(version) > 1)
			{
				if (TextureFormat == TextureFormat.ATC_RGB4)
				{
					return TextureFormat.ETC_RGB4;
				}
				if (TextureFormat == TextureFormat.ATC_RGBA8)
				{
					return TextureFormat.ETC2_RGBA8;
				}
			}
			return TextureFormat;
		}
		private int GetCompressionQuality(Version dataVersion, Version exportVersion)
		{
			if (GetSerializedVersion(dataVersion) < 3)
			{
				if (GetSerializedVersion(exportVersion) >= 3)
				{
					if (TextureFormat == TextureFormat.BC6H || TextureFormat == TextureFormat.BC7)
					{
						return 100;
					}
				}
			}
			return CompressionQuality;
		}
		private bool GetForceMaximumCompressionQuality_BC6H_BC7(Version version)
		{
			return IsReadForceMaximumCompressionQuality_BC6H_BC7(version) ? ForceMaximumCompressionQuality_BC6H_BC7 : true;
		}

		public string BuildTarget { get; private set; }
		public int MaxTextureSize { get; private set; }
		public TextureResizeAlgorithm ResizeAlgorithm { get; private set; }
		public TextureFormat TextureFormat { get; private set; }
		public TextureImporterCompression TextureCompression { get; private set; }
		public int CompressionQuality { get; private set; }
		public bool CrunchedCompression { get; private set; }
		public bool AllowsAlphaSplitting { get; private set; }
		public bool Overridden { get; private set; }
		public AndroidETC2FallbackOverride AndroidETC2FallbackOverride { get; private set; }
		public bool ForceMaximumCompressionQuality_BC6H_BC7 { get; private set; }

		public const string BuildTargetName = "m_BuildTarget";
		public const string MaxTextureSizeName = "m_MaxTextureSize";
		public const string ResizeAlgorithmName = "m_ResizeAlgorithm";
		public const string TextureFormatName = "m_TextureFormat";
		public const string TextureCompressionName = "m_TextureCompression";
		public const string CompressionQualityName = "m_CompressionQuality";
		public const string CrunchedCompressionName = "m_CrunchedCompression";
		public const string AllowsAlphaSplittingName = "m_AllowsAlphaSplitting";
		public const string OverriddenName = "m_Overridden";
		public const string AndroidETC2FallbackOverrideName = "m_AndroidETC2FallbackOverride";
		public const string ForceMaximumCompressionQuality_BC6H_BC7Name = "m_ForceMaximumCompressionQuality_BC6H_BC7";

		public const string DefaultTexturePlatformName = "DefaultTexturePlatform";
	}
}
