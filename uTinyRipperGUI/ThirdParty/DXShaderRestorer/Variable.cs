using System;
using uTinyRipper.Classes.Shaders;

namespace DXShaderRestorer
{
	internal class Variable
	{
		public Variable(MatrixParameter param, ShaderGpuProgramType programType)
		{
			ShaderType = new ShaderType(param, programType);
			Name = param.Name ?? throw new Exception("Variable name cannot be null");
			NameIndex = param.NameIndex;
			Index = param.Index;
			ArraySize = param.ArraySize;
		}

		public Variable(VectorParameter param, ShaderGpuProgramType programType)
		{
			ShaderType = new ShaderType(param, programType);
			Name = param.Name ?? throw new Exception("Variable name cannot be null");
			NameIndex = param.NameIndex;
			Index = param.Index;
			ArraySize = param.ArraySize;
		}

		public Variable(StructParameter param, ShaderGpuProgramType programType)
		{
			ShaderType = new ShaderType(param, programType);
			Name = param.Name ?? throw new Exception("Variable name cannot be null");
			NameIndex = param.NameIndex;
			Index = param.Index;
			ArraySize = param.ArraySize;
		}

		public Variable(string name, int index, int sizeToAdd, ShaderGpuProgramType prgramType)
		{
			if (sizeToAdd % 4 != 0 || sizeToAdd <= 0) throw new Exception($"Invalid dummy variable size {sizeToAdd}");
			var param = new VectorParameter(name, ShaderParamType.Int, index, sizeToAdd / 4, 0);
			ShaderType = new ShaderType(param, prgramType);
			Name = name ?? throw new Exception("Variable name cannot be null");
			NameIndex = -1;
			Index = index;
			ArraySize = param.ArraySize;
			Type = ShaderParamType.Int;
		}

		public ShaderType ShaderType { get; }
		public string Name { get; }
		public int NameIndex { get; }
		public int Index { get; }
		public int ArraySize { get; }
		public ShaderParamType Type { get; }
		public uint Length { get; set; }
	}
}
