using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

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

		public float Weight0 { get => Weights[0]; set => Weights[0] = value; }
		public float Weight1 { get => Weights[1]; set => Weights[1] = value; }
		public float Weight2 { get => Weights[2]; set => Weights[2] = value; }
		public float Weight3 { get => Weights[3]; set => Weights[3] = value; }
		public int BoneIndex0 { get => BoneIndices[0]; set => BoneIndices[0] = value; }
		public int BoneIndex1 { get => BoneIndices[1]; set => BoneIndices[1] = value; }
		public int BoneIndex2 { get => BoneIndices[2]; set => BoneIndices[2] = value; }
		public int BoneIndex3 { get => BoneIndices[3]; set => BoneIndices[3] = value; }
		public float[] Weights
		{
			get
			{
				if (m_Weights == null) m_Weights = new float[4];
				return m_Weights;
			}
		}
		public int[] BoneIndices
		{
			get
			{
				if (m_BoneIndices == null) m_BoneIndices = new int[4];
				return m_BoneIndices;
			}
		}
		private float[] m_Weights;
		private int[] m_BoneIndices;


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
