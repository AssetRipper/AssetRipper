using System.Collections.Generic;

namespace uTinyRipper.Classes.Shaders
{
	public struct StructParameter : IAssetReadable
	{
		public StructParameter(string name, int index, int arraySize, int structSize, VectorParameter[] vectors, MatrixParameter[] matrices)
		{
			Name = name;
			NameIndex = -1;
			Index = index;
			ArraySize = arraySize;
			StructSize = structSize;
			m_vectorMembers = vectors;
			m_matrixMembers = matrices;
		}

		public void Read(AssetReader reader)
		{
			NameIndex = reader.ReadInt32();
			Index = reader.ReadInt32();
			ArraySize = reader.ReadInt32();
			StructSize = reader.ReadInt32();
			m_vectorMembers = reader.ReadAssetArray<VectorParameter>();
			reader.AlignStream(AlignType.Align4);
			m_matrixMembers = reader.ReadAssetArray<MatrixParameter>();
			reader.AlignStream(AlignType.Align4);
		}

		public string Name { get; private set; }
		public int NameIndex { get; private set; }
		public int Index { get; private set; }
		public int ArraySize { get; private set; }
		public int StructSize { get; private set; }
		public IReadOnlyList<VectorParameter> VectorMembers => m_vectorMembers;
		public IReadOnlyList<MatrixParameter> MatrixMembers => m_matrixMembers;

		private VectorParameter[] m_vectorMembers;
		private MatrixParameter[] m_matrixMembers;
	}
}
