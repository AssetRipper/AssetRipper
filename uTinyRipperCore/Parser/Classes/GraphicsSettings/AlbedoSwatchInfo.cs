using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.GraphicsSettingss
{
	public struct AlbedoSwatchInfo : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Name = reader.ReadString();
			Color.Read(reader);
			MinLuminance = reader.ReadSingle();
			MaxLuminance = reader.ReadSingle();
			reader.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("name", Name);
			node.Add("color", Color.ExportYAML(container));
			node.Add("minLuminance", MinLuminance);
			node.Add("maxLuminance", MaxLuminance);
			return node;
		}

		public string Name { get; private set; }
		public float MinLuminance { get; private set; }
		public float MaxLuminance { get; private set; }

		public ColorRGBAf Color;
	}
}
