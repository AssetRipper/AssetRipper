using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Misc
{
	/// <summary>
	/// BoneInfluence previously
	/// </summary>
	public sealed class BoneWeights4 : IAsset
	{
		public BoneWeights4() { }

		public BoneWeights4(float w0, float w1, float w2, float w3, int i0, int i1, int i2, int i3)
		{
			m_Weights = new float[4];
			m_BoneIndices = new int[4];
			Weight_0_ = w0;
			Weight_1_ = w1;
			Weight_2_ = w2;
			Weight_3_ = w3;
			BoneIndex_0_ = i0;
			BoneIndex_1_ = i1;
			BoneIndex_2_ = i2;
			BoneIndex_3_ = i3;
		}

		public void Read(AssetReader reader)
		{
			Weight_0_ = reader.ReadSingle();
			Weight_1_ = reader.ReadSingle();
			Weight_2_ = reader.ReadSingle();
			Weight_3_ = reader.ReadSingle();
			BoneIndex_0_ = reader.ReadInt32();
			BoneIndex_1_ = reader.ReadInt32();
			BoneIndex_2_ = reader.ReadInt32();
			BoneIndex_3_ = reader.ReadInt32();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(Weight_0_);
			writer.Write(Weight_1_);
			writer.Write(Weight_2_);
			writer.Write(Weight_3_);
			writer.Write(BoneIndex_0_);
			writer.Write(BoneIndex_1_);
			writer.Write(BoneIndex_2_);
			writer.Write(BoneIndex_3_);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(Weight0Name, Weight_0_);
			node.Add(Weight1Name, Weight_1_);
			node.Add(Weight2Name, Weight_2_);
			node.Add(Weight3Name, Weight_3_);
			node.Add(BoneIndex0Name, BoneIndex_0_);
			node.Add(BoneIndex1Name, BoneIndex_1_);
			node.Add(BoneIndex2Name, BoneIndex_2_);
			node.Add(BoneIndex3Name, BoneIndex_3_);
			return node;
		}

		public float Weight_0_ { get => Weights[0]; set => Weights[0] = value; }
		public float Weight_1_ { get => Weights[1]; set => Weights[1] = value; }
		public float Weight_2_ { get => Weights[2]; set => Weights[2] = value; }
		public float Weight_3_ { get => Weights[3]; set => Weights[3] = value; }
		public int BoneIndex_0_ { get => BoneIndices[0]; set => BoneIndices[0] = value; }
		public int BoneIndex_1_ { get => BoneIndices[1]; set => BoneIndices[1] = value; }
		public int BoneIndex_2_ { get => BoneIndices[2]; set => BoneIndices[2] = value; }
		public int BoneIndex_3_ { get => BoneIndices[3]; set => BoneIndices[3] = value; }
		public float[] Weights
		{
			get
			{
				m_Weights ??= new float[4];
				return m_Weights;
			}
		}
		public int[] BoneIndices
		{
			get
			{
				m_BoneIndices ??= new int[4];
				return m_BoneIndices;
			}
		}
		private float[]? m_Weights;
		private int[]? m_BoneIndices;


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
