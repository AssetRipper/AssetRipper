using uTinyRipper.Classes.Textures;
using uTinyRipper.Converters;
using uTinyRipper.Layout;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.TextureImporters
{
	/// <summary>
	/// PlatformSettings in version < 2017.3
	/// BuildTargetSettings in version < 5.5.0
	/// </summary>
	public struct TextureImporterPlatformSettings : IAsset
	{
		public TextureImporterPlatformSettings(AssetLayout layout)
		{
			BuildTarget = DefaultTexturePlatformName;
			MaxTextureSize = 2048;
			ResizeAlgorithm = TextureResizeAlgorithm.Mitchell;
			TextureFormat = TextureFormat.Automatic;
			TextureCompression = TextureImporterCompression.Uncompressed;
			CompressionQuality = 50;
			CrunchedCompression = false;
			AllowsAlphaSplitting = false;
			Overridden = false;
			AndroidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings;
			ForceMaximumCompressionQuality_BC6H_BC7 = false;
		}

		public static int ToSerializedVersion(Version version)
		{
			// ForceMaximumCompressionQuality_BC6H_BC7 default value has been changed from 1 to 0
			if (version.IsGreaterEqual(2019, 2))
			{
				return 3;
			}
			// TextureFormat.ATC_RGB4/ATC_RGBA8 has been replaced by ETC_RGB4/ETC2_RGBA8
			if (version.IsGreaterEqual(2018))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool HasResizeAlgorithm(Version version) => version.IsGreaterEqual(2017, 2);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasTextureCompression(Version version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasCrunchedCompression(Version version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasCompressionQuality(Version version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 5.2.0 and greater
		/// </summary>
		public static bool HasAllowsAlphaSplitting(Version version) => version.IsGreaterEqual(5, 2);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasOverridden(Version version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasAndroidETC2FallbackOverride(Version version) => version.IsGreaterEqual(2017, 3);
		/// <summary>
		/// 2019.2 and greater
		/// </summary>
		public static bool HasForceMaximumCompressionQuality_BC6H_BC7(Version version) => version.IsGreaterEqual(2019, 2);

		/// <summary>
		/// 5.2.0 and greater
		/// </summary>
		private static bool IsAlign(Version version) => version.IsGreaterEqual(5, 2);

		public void Read(AssetReader reader)
		{
			BuildTarget = reader.ReadString();
			MaxTextureSize = reader.ReadInt32();
			if (HasResizeAlgorithm(reader.Version))
			{
				ResizeAlgorithm = (TextureResizeAlgorithm)reader.ReadInt32();
			}
			TextureFormat = (TextureFormat)reader.ReadInt32();
			if (HasTextureCompression(reader.Version))
			{
				TextureCompression = (TextureImporterCompression)reader.ReadInt32();
			}
			if (HasCompressionQuality(reader.Version))
			{
				CompressionQuality = reader.ReadInt32();
			}
			if (HasCrunchedCompression(reader.Version))
			{
				CrunchedCompression = reader.ReadBoolean();
			}
			if (HasAllowsAlphaSplitting(reader.Version))
			{
				AllowsAlphaSplitting = reader.ReadBoolean();
			}
			if (HasOverridden(reader.Version))
			{
				Overridden = reader.ReadBoolean();
			}
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasAndroidETC2FallbackOverride(reader.Version))
			{
				AndroidETC2FallbackOverride = (AndroidETC2FallbackOverride)reader.ReadInt32();
			}
			if (HasForceMaximumCompressionQuality_BC6H_BC7(reader.Version))
			{
				ForceMaximumCompressionQuality_BC6H_BC7 = reader.ReadBoolean();
			}
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(BuildTarget);
			writer.Write(MaxTextureSize);
			if (HasResizeAlgorithm(writer.Version))
			{
				writer.Write((int)ResizeAlgorithm);
			}
			writer.Write((int)TextureFormat);
			if (HasTextureCompression(writer.Version))
			{
				writer.Write((int)TextureCompression);
			}
			if (HasCompressionQuality(writer.Version))
			{
				writer.Write(CompressionQuality);
			}
			if (HasCrunchedCompression(writer.Version))
			{
				writer.Write(CrunchedCompression);
			}
			if (HasAllowsAlphaSplitting(writer.Version))
			{
				writer.Write(AllowsAlphaSplitting);
			}
			if (HasOverridden(writer.Version))
			{
				writer.Write(Overridden);
			}
			if (IsAlign(writer.Version))
			{
				writer.AlignStream();
			}

			if (HasAndroidETC2FallbackOverride(writer.Version))
			{
				writer.Write((int)AndroidETC2FallbackOverride);
			}
			if (HasForceMaximumCompressionQuality_BC6H_BC7(writer.Version))
			{
				writer.Write(ForceMaximumCompressionQuality_BC6H_BC7);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(BuildTargetName, BuildTarget);
			node.Add(MaxTextureSizeName, MaxTextureSize);
			if (HasResizeAlgorithm(container.ExportVersion))
			{
				node.Add(ResizeAlgorithmName, (int)ResizeAlgorithm);
			}
			node.Add(TextureFormatName, (int)GetTextureFormat(container.Version));
			if (HasTextureCompression(container.ExportVersion))
			{
				node.Add(TextureCompressionName, (int)TextureCompression);
			}
			if (HasCompressionQuality(container.ExportVersion))
			{
				node.Add(CompressionQualityName, GetCompressionQuality(container));
			}
			if (HasCrunchedCompression(container.ExportVersion))
			{
				node.Add(CrunchedCompressionName, CrunchedCompression);
			}
			if (HasAllowsAlphaSplitting(container.ExportVersion))
			{
				node.Add(AllowsAlphaSplittingName, AllowsAlphaSplitting);
			}
			if (HasOverridden(container.ExportVersion))
			{
				node.Add(OverriddenName, Overridden);
			}
			if (HasAndroidETC2FallbackOverride(container.ExportVersion))
			{
				node.Add(AndroidETC2FallbackOverrideName, (int)AndroidETC2FallbackOverride);
			}
			if (HasForceMaximumCompressionQuality_BC6H_BC7(container.ExportVersion))
			{
				node.Add(ForceMaximumCompressionQuality_BC6H_BC7Name, GetForceMaximumCompressionQuality_BC6H_BC7(container.Version));
			}
			return node;
		}

		private TextureFormat GetTextureFormat(Version version)
		{
			if (ToSerializedVersion(version) > 1)
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
		private int GetCompressionQuality(IExportContainer container)
		{
			if (ToSerializedVersion(container.Version) < 3)
			{
				if (ToSerializedVersion(container.ExportVersion) >= 3)
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
			return HasForceMaximumCompressionQuality_BC6H_BC7(version) ? ForceMaximumCompressionQuality_BC6H_BC7 : true;
		}

		public string BuildTarget { get; set; }
		public int MaxTextureSize { get; set; }
		public TextureResizeAlgorithm ResizeAlgorithm { get; set; }
		public TextureFormat TextureFormat { get; set; }
		public TextureImporterCompression TextureCompression { get; set; }
		public int CompressionQuality { get; set; }
		public bool CrunchedCompression { get; set; }
		public bool AllowsAlphaSplitting { get; set; }
		public bool Overridden { get; set; }
		public AndroidETC2FallbackOverride AndroidETC2FallbackOverride { get; set; }
		public bool ForceMaximumCompressionQuality_BC6H_BC7 { get; set; }

		public const string DefaultTexturePlatformName = "DefaultTexturePlatform";
		public const string StandaloneTexturePlatformName = "StandalonePlatform";

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
	}
}
