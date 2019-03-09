using uTinyRipper.Classes;
using uTinyRipper.Classes.Textures;
using uTinyRipper.Exporter.YAML;

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
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 1;
			}
			return ToSerializedVersion(version);
		}

		private static int ToSerializedVersion(Version version)
		{
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
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("buildTarget", BuildTarget);
			node.Add("maxTextureSize", MaxTextureSize);
			node.Add("resizeAlgorithm", (int)ResizeAlgorithm);
			node.Add("textureFormat", (int)GetTextureFormat(container.Version));
			node.Add("textureCompression", (int)TextureCompression);
			node.Add("compressionQuality", CompressionQuality);
			node.Add("crunchedCompression", CrunchedCompression);
			node.Add("allowsAlphaSplitting", AllowsAlphaSplitting);
			node.Add("overridden", Overridden);
			node.Add("androidETC2FallbackOverride", (int)AndroidETC2FallbackOverride);
			return node;
		}

		public TextureFormat GetTextureFormat(Version version)
		{
			if(ToSerializedVersion(version) > 1)
			{
				if(TextureFormat == TextureFormat.ATC_RGB4)
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

		public const string DefaultTexturePlatformName = "DefaultTexturePlatform";
	}
}
