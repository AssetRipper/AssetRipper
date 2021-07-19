using AssetRipper.Classes.Misc;
using AssetRipper.Converters;
using AssetRipper.YAML;

namespace AssetRipper.Classes.Avatars
{
	public struct SkeletonPose : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			X = reader.ReadAssetArray<XForm>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(XName, X == null ? YAMLSequenceNode.Empty : X.ExportYAML(container));
			return node;
		}

		public XForm[] X { get; set; }

		public const string XName = "m_X";
	}
}
