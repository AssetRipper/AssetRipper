using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Meshes
{
	/// <summary>
	/// BoneInfluence in old versions
	/// </summary>
	public struct BoneWeights4 : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			Weight0 = stream.ReadSingle();
			Weight1 = stream.ReadSingle();
			Weight2 = stream.ReadSingle();
			Weight3 = stream.ReadSingle();
			BoneIndex0 = stream.ReadInt32();
			BoneIndex1 = stream.ReadInt32();
			BoneIndex2 = stream.ReadInt32();
			BoneIndex3 = stream.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("weight[0]", Weight0);
			node.Add("weight[1]", Weight1);
			node.Add("weight[2]", Weight2);
			node.Add("weight[3]", Weight3);
			node.Add("boneIndex[0]", BoneIndex0);
			node.Add("boneIndex[1]", BoneIndex1);
			node.Add("boneIndex[2]", BoneIndex2);
			node.Add("boneIndex[3]", BoneIndex3);
			return node;
		}
		
		public float Weight0 { get; private set; }
		public float Weight1 { get; private set; }
		public float Weight2 { get; private set; }
		public float Weight3 { get; private set; }
		public int BoneIndex0 { get; private set; }
		public int BoneIndex1 { get; private set; }
		public int BoneIndex2 { get; private set; }
		public int BoneIndex3 { get; private set; }
	}
}
