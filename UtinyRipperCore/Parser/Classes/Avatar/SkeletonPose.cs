using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Avatars
{
	public struct SkeletonPose : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			m_x = stream.ReadArray<XForm>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_X", X == null ? YAMLSequenceNode.Empty : X.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<XForm> X => m_x;

		private XForm[] m_x;
	}
}
