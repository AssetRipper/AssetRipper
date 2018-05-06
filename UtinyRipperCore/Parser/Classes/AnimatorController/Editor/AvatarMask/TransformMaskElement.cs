using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AvatarMasks
{
	public sealed class TransformMaskElement : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			Path = stream.ReadStringAligned();
			Weight = stream.ReadSingle();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Path", Path);
			node.Add("m_Weight", Weight);
			return node;
		}

		public string Path { get; private set; }
		public float Weight { get; private set; }
	}
}
