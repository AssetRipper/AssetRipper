using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Avatars
{
	public struct Skeleton : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			m_node = reader.ReadAssetArray<Node>();
			m_ID = reader.ReadUInt32Array();
			m_axesArray = reader.ReadAssetArray<Axes>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Node", Node == null ? YAMLSequenceNode.Empty : Node.ExportYAML(container));
			node.Add("m_ID", ID == null ? YAMLSequenceNode.Empty : ID.ExportYAML(true));
			node.Add("m_AxesArray", AxesArray == null ? YAMLSequenceNode.Empty : AxesArray.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<Node> Node => m_node;
		public IReadOnlyList<uint> ID => m_ID;
		public IReadOnlyList<Axes> AxesArray => m_axesArray;
		
		private Node[] m_node;
		private uint[] m_ID;
		private Axes[] m_axesArray;
	}
}
