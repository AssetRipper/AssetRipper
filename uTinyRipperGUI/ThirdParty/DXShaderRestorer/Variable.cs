using System;
using uTinyRipper.Classes.Shaders;

namespace DXShaderRestorer
{
	internal class Variable
	{
		private Variable()
		{
		}

		public Variable(MatrixParameter param, ShaderGpuProgramType programType)
		{
			ShaderType = new ShaderType(param, programType);
			Name = param.Name ?? throw new Exception("Variable name cannot be null");
			NameIndex = param.NameIndex;
			Index = param.Index;
			ArraySize = param.ArraySize;
			Length = (uint)(param.RowCount * param.ColumnCount * 4);
		}

		public Variable(VectorParameter param, ShaderGpuProgramType programType)
		{
			ShaderType = new ShaderType(param, programType);
			Name = param.Name ?? throw new Exception("Variable name cannot be null");
			NameIndex = param.NameIndex;
			Index = param.Index;
			ArraySize = param.ArraySize;
			Length = (uint)(param.Dim * 4);
		}

		public Variable(StructParameter param, ShaderGpuProgramType programType)
		{
			ShaderType = new ShaderType(param, programType);
			Name = param.Name ?? throw new Exception("Variable name cannot be null");
			NameIndex = param.NameIndex;
			Index = param.Index;
			ArraySize = param.ArraySize;
		}

		public static Variable CreateDummyVariable(string name, int index, int sizeToAdd, ShaderGpuProgramType programType)
		{
			if (sizeToAdd % 4 != 0 || sizeToAdd <= 0) throw new Exception($"Invalid dummy variable size {sizeToAdd}");
			Variable variable = new Variable();
			var param = new VectorParameter(name, ShaderParamType.Int, index, sizeToAdd / 4, 0);
			variable.ShaderType = new ShaderType(param, programType);
			variable.Name = name ?? throw new Exception("Variable name cannot be null");
			variable.NameIndex = -1;
			variable.Index = index;
			variable.ArraySize = param.ArraySize;
			variable.Type = ShaderParamType.Int;
			variable.Length = (uint)sizeToAdd;
			return variable;
		}

		public static Variable CreateResourceBindVariable(ShaderGpuProgramType programType)
		{
			Variable variable = new Variable();
			variable.Name = "$Element";
			var param = new VectorParameter(variable.Name, ShaderParamType.UInt, 0, 1);
			variable.ShaderType = new ShaderType(param, programType);
			variable.NameIndex = -1;
			variable.Index = 0;
			variable.ArraySize = param.ArraySize;
			variable.Length = 4;
			variable.Type = ShaderParamType.UInt;
			return variable;
		}

		public ShaderType ShaderType { get; private set;  }
		public string Name { get; private set; }
		public int NameIndex { get; private set; }
		public int Index { get; private set; }
		public int ArraySize { get; private set; }
		public ShaderParamType Type { get; private set; }
		public uint Length { get; set; }

	}
}
