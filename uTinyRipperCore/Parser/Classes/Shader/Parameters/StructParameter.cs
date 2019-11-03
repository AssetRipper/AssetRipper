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
			VectorMembers = vectors;
			MatrixMembers = matrices;
		}

		public void Read(AssetReader reader)
		{
			NameIndex = reader.ReadInt32();
			Index = reader.ReadInt32();
			ArraySize = reader.ReadInt32();
			StructSize = reader.ReadInt32();
			VectorMembers = reader.ReadAssetArray<VectorParameter>();
			reader.AlignStream();
			MatrixMembers = reader.ReadAssetArray<MatrixParameter>();
			reader.AlignStream();
		}

		public string Name { get; set; }
		public int NameIndex { get; set; }
		public int Index { get; set; }
		public int ArraySize { get; set; }
		public int StructSize { get; set; }
		public VectorParameter[] VectorMembers { get; set; }
		public MatrixParameter[] MatrixMembers { get; set; }
	}
}
