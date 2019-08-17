using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uTinyRipper.Classes.Shaders;

namespace DXShaderExporter
{
	internal class Variable
	{
		public ShaderType ShaderType;
		public Variable(MatrixParameter param, ShaderGpuProgramType programType)
		{
			ShaderType = new ShaderType(param, programType);
			Name = param.Name;
			NameIndex = param.NameIndex;
			Index = param.Index;
			ArraySize = param.ArraySize;
			if (Name == null) throw new Exception("Variable name cannot be null");
		}
		public Variable(VectorParameter param, ShaderGpuProgramType programType)
		{
			ShaderType = new ShaderType(param, programType);
			Name = param.Name;
			NameIndex = param.NameIndex;
			Index = param.Index;
			ArraySize = param.ArraySize;
			if (Name == null) throw new Exception("Variable name cannot be null");
		}
		public Variable(StructParameter param, ShaderGpuProgramType programType)
		{
			ShaderType = new ShaderType(param, programType);
			Name = param.Name;
			NameIndex = param.NameIndex;
			Index = param.Index;
			ArraySize = param.ArraySize;
			if (Name == null) throw new Exception("Variable name cannot be null");
		}
		public Variable(string name, int index, int sizeToAdd, ShaderGpuProgramType prgramType)
		{
			if (sizeToAdd % 4 != 0 || sizeToAdd <= 0) throw new Exception($"Invalid dummy variable size {sizeToAdd}");
			var param = new VectorParameter(name, ShaderParamType.Int, index, sizeToAdd / 4, 0);
			ShaderType = new ShaderType(param, prgramType);
			Name = name;
			NameIndex = -1;
			Index = index;
			ArraySize = param.ArraySize;
			Type = ShaderParamType.Int;
			if (Name == null) throw new Exception("Variable name cannot be null");
		}
		public string Name;
		public int NameIndex;
		public int Index;
		public int ArraySize;
		public ShaderParamType Type;
		public uint Length;
	}
}
