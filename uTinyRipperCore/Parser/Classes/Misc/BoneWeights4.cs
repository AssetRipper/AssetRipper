using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Misc
{
	/// <summary>
	/// BoneInfluence previously
	/// </summary>
	public struct BoneWeights4 : IAsset
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

		public void Write(AssetWriter writer)
		{
			writer.Write(Weight0);
			writer.Write(Weight1);
			writer.Write(Weight2);
			writer.Write(Weight3);
			writer.Write(BoneIndex0);
			writer.Write(BoneIndex1);
			writer.Write(BoneIndex2);
			writer.Write(BoneIndex3);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(Weight0Name, Weight0);
			node.Add(Weight1Name, Weight1);
			node.Add(Weight2Name, Weight2);
			node.Add(Weight3Name, Weight3);
			node.Add(BoneIndex0Name, BoneIndex0);
			node.Add(BoneIndex1Name, BoneIndex1);
			node.Add(BoneIndex2Name, BoneIndex2);
			node.Add(BoneIndex3Name, BoneIndex3);
			return node;
		}

		public float Weight0 { get; set; }
		public float Weight1 { get; set; }
		public float Weight2 { get; set; }
		public float Weight3 { get; set; }
		public int BoneIndex0 { get; set; }
		public int BoneIndex1 { get; set; }
		public int BoneIndex2 { get; set; }
		public int BoneIndex3 { get; set; }

		public const string Weight0Name = "weight[0]";
		public const string Weight1Name = "weight[1]";
		public const string Weight2Name = "weight[2]";
		public const string Weight3Name = "weight[3]";
		public const string BoneIndex0Name = "boneIndex[0]";
		public const string BoneIndex1Name = "boneIndex[1]";
		public const string BoneIndex2Name = "boneIndex[2]";
		public const string BoneIndex3Name = "boneIndex[3]";

		public const int Dimention = 4;
	}
}
