using System;

namespace ShaderTextRestorer.ShaderBlob.Parameters
{
	public sealed class StructParameter
	{
		public StructParameter() { }

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

		public string Name { get; set; } = string.Empty;
		public int NameIndex { get; set; }
		public int Index { get; set; }
		public int ArraySize { get; set; }
		public int StructSize { get; set; }
		public VectorParameter[] VectorMembers { get; set; } = Array.Empty<VectorParameter>();
		public MatrixParameter[] MatrixMembers { get; set; } = Array.Empty<MatrixParameter>();
	}
}
