using AssetRipper.Core.Classes.Shader.Enums;

namespace ShaderTextRestorer.ShaderBlob.Parameters
{
	public sealed class VectorParameter
	{
		public VectorParameter() { }

		public VectorParameter(string name, ShaderParamType type, int index, int columns)
		{
			Name = name;
			NameIndex = -1;
			Index = index;
			ArraySize = 0;
			Type = type;
			Dim = (byte)columns;
		}

		public VectorParameter(string name, ShaderParamType type, int index, int arraySize, int columns) : this(name, type, index, columns)
		{
			ArraySize = arraySize;
		}

		public string Name { get; set; } = string.Empty;
		public int NameIndex { get; set; }
		public int Index { get; set; }
		public int ArraySize { get; set; }
		public ShaderParamType Type { get; set; }
		public byte Dim { get; set; }
	}
}
