using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper;

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
			node.Add(NodeName, Node == null ? YAMLSequenceNode.Empty : Node.ExportYAML(container));
			node.Add(IDName, ID == null ? YAMLSequenceNode.Empty : ID.ExportYAML(true));
			node.Add(AxesArrayName, AxesArray == null ? YAMLSequenceNode.Empty : AxesArray.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<Node> Node => m_node;
		public IReadOnlyList<uint> ID => m_ID;
		public IReadOnlyList<Axes> AxesArray => m_axesArray;

		public const string NodeName = "m_Node";
		public const string IDName = "m_ID";
		public const string AxesArrayName = "m_AxesArray";

		private Node[] m_node;
		private uint[] m_ID;
		private Axes[] m_axesArray;
	}
}
