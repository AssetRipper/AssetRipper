using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;

namespace AssetRipper.Core.Classes.Avatar
{
	public sealed class Skeleton : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			Node = reader.ReadAssetArray<Node>();
			ID = reader.ReadUInt32Array();
			AxesArray = reader.ReadAssetArray<Axes>();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(NodeName, Node == null ? YamlSequenceNode.Empty : Node.ExportYaml(container));
			node.Add(IDName, ID == null ? YamlSequenceNode.Empty : ID.ExportYaml(true));
			node.Add(AxesArrayName, AxesArray == null ? YamlSequenceNode.Empty : AxesArray.ExportYaml(container));
			return node;
		}

		public Node[] Node { get; set; }
		public uint[] ID { get; set; }
		public Axes[] AxesArray { get; set; }

		public const string NodeName = "m_Node";
		public const string IDName = "m_ID";
		public const string AxesArrayName = "m_AxesArray";
	}
}
