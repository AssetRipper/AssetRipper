using uTinyRipper.Classes.Shaders;

namespace DXShaderRestorer
{
	class ShaderTypeMember
	{
		public ShaderTypeMember(MatrixParameter param, ShaderGpuProgramType programType)
		{
			ProgramType = programType;
			Name = param.Name;
			ShaderType = new ShaderType(param, programType);
			Index = (uint)param.Index;
		}

		public ShaderTypeMember(VectorParameter param, ShaderGpuProgramType programType)
		{
			ProgramType = programType;
			Name = param.Name;
			ShaderType = new ShaderType(param, programType);
			Index = (uint)param.Index;
		}

		public string Name { get; }
		public ShaderType ShaderType { get; }
		public ShaderGpuProgramType ProgramType { get; }
		public uint Index { get; }
	}
}
