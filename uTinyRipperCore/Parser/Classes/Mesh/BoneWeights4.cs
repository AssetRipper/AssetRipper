using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Meshes
{
	/// <summary>
	/// BoneInfluence in old versions
	/// </summary>
	public struct BoneWeights4 : IAssetReadable, IYAMLExportable
	{
		public BoneWeights4(float w0, float w1, float w2, float w3, int i0, int i1, int i2, int i3)
		{
			Weight0 = w0;
			Weight1 = w1;
			Weight2 = w2;
			Weight3 = w3;
			BoneIndex0 = i0;
			BoneIndex1 = i1;
			BoneIndex2 = i2;
			BoneIndex3 = i3;
		}

		public void Read(AssetReader reader)
		{
			Weight0 = reader.ReadSingle();
			Weight1 = reader.ReadSingle();
			Weight2 = reader.ReadSingle();
			Weight3 = reader.ReadSingle();
			BoneIndex0 = reader.ReadInt32();
			BoneIndex1 = reader.ReadInt32();
			BoneIndex2 = reader.ReadInt32();
			BoneIndex3 = reader.ReadInt32();
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
