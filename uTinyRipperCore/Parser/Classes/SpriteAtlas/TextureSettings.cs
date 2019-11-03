using uTinyRipper.Classes.Textures;
using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper.Classes.TextureImporters;

namespace uTinyRipper.Classes.SpriteAtlases
{
	public struct TextureSettings : IAssetReadable, IYAMLExportable
	{
		public TextureSettings(bool _)
		{
			AnisoLevel = 1;
			CompressionQuality = 50;
			MaxTextureSize = 2048;
			TextureCompression = TextureImporterCompression.Uncompressed;
			FilterMode = FilterMode.Bilinear;
			GenerateMipMaps = false;
			Readable = false;
			CrunchedCompression = false;
			SRGB = true;
		}

		public static int ToSerializedVersion(Version version)
		{
			// colorSpace was renamed to sRGB
			if (version.IsGreaterEqual(2017, 1, 2))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 2017.1.0b3
		/// </summary>
		public static bool HasCrunchedCompression(Version version) => version.IsGreaterEqual(2017, 1, 0, VersionType.Beta, 3);

		/// <summary>
		/// Less than 2017.1.2
		/// </summary>
		private static bool HasColorSpace(Version version) => version.IsLess(2017, 1, 2);

		public void Read(AssetReader reader)
		{
			AnisoLevel = reader.ReadInt32();
			CompressionQuality = reader.ReadInt32();
			MaxTextureSize = reader.ReadInt32();
			TextureCompression = (TextureImporterCompression)reader.ReadInt32();
			if (HasColorSpace(reader.Version))
			{
				ColorSpace colorSpace = (ColorSpace)reader.ReadInt32();
				SRGB = colorSpace == ColorSpace.Gamma;
			}
			FilterMode = (FilterMode)reader.ReadInt32();
			GenerateMipMaps = reader.ReadBoolean();
			Readable = reader.ReadBoolean();
			if (HasCrunchedCompression(reader.Version))
			{
				CrunchedCompression = reader.ReadBoolean();
			}
			reader.AlignStream();
			
			if (!HasColorSpace(reader.Version))
			{
				SRGB = reader.ReadBoolean();
				reader.AlignStream();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(AnisoLevelName, AnisoLevel);
			node.Add(CompressionQualityName, CompressionQuality);
			node.Add(MaxTextureSizeName, MaxTextureSize);
			node.Add(TextureCompressionName, (int)TextureCompression);
			node.Add(FilterModeName, (int)FilterMode);
			node.Add(GenerateMipMapsName, GenerateMipMaps);
			node.Add(ReadableName, Readable);
			node.Add(CrunchedCompressionName, CrunchedCompression);
			node.Add(SRGBName, SRGB);
			return node;
		}

		public int AnisoLevel { get; set; }
		public int CompressionQuality { get; set; }
		public int MaxTextureSize { get; set; }
		public TextureImporterCompression TextureCompression { get; set; }
		public FilterMode FilterMode { get; set; }
		public bool GenerateMipMaps { get; set; }
		public bool Readable { get; set; }
		public bool CrunchedCompression { get; set; }
		/// <summary>
		/// ColorSpace previously
		/// </summary>
		public bool SRGB { get; set; }

		public const string AnisoLevelName = "anisoLevel";
		public const string CompressionQualityName = "compressionQuality";
		public const string MaxTextureSizeName = "maxTextureSize";
		public const string TextureCompressionName = "textureCompression";
		public const string FilterModeName = "filterMode";
		public const string GenerateMipMapsName = "generateMipMaps";
		public const string ReadableName = "readable";
		public const string CrunchedCompressionName = "crunchedCompression";
		public const string SRGBName = "sRGB";
	}
}
