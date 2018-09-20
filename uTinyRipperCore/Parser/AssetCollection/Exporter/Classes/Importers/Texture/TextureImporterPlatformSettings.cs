using uTinyRipper.Classes.Textures;
using uTinyRipper.Exporter.YAML;

namespace uTinyRipper.AssetExporters.Classes
{
	public class TextureImporterPlatformSettings : IYAMLExportable
	{
		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("buildTarget", BuildTarget);
			node.Add("maxTextureSize", MaxTextureSize);
			node.Add("resizeAlgorithm", (int)ResizeAlgorithm);
			node.Add("textureFormat", (int)TextureFormat);
			node.Add("textureCompression", (int)TextureCompression);
			node.Add("compressionQuality", CompressionQuality);
			node.Add("crunchedCompression", CrunchedCompression);
			node.Add("allowsAlphaSplitting", AllowsAlphaSplitting);
			node.Add("overridden", Overridden);
			node.Add("androidETC2FallbackOverride", (int)AndroidETC2FallbackOverride);
			return node;
		}

		public string BuildTarget { get; private set; } = "DefaultTexturePlatform";
		public int MaxTextureSize { get; private set; } = 2048;
		public TextureResizeAlgorithm ResizeAlgorithm { get; private set; } = TextureResizeAlgorithm.Mitchell;
		public TextureFormat TextureFormat { get; private set; } = TextureFormat.Automatic;
		public TextureImporterCompression TextureCompression { get; private set; } = TextureImporterCompression.Compressed;
		public int CompressionQuality { get; private set; } = 50;
		public bool CrunchedCompression { get; private set; } = false;
		public bool AllowsAlphaSplitting { get; private set; } = false;
		public bool Overridden { get; private set; } = false;
		public AndroidETC2FallbackOverride AndroidETC2FallbackOverride { get; private set; } = AndroidETC2FallbackOverride.UseBuildSettings;
	}
}
