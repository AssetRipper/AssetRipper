using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Avatars
{
	public struct SkeletonPose : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			m_x = reader.ReadAssetArray<XForm>();
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
