using AssetRipper.Core.Classes.Shader.Enums.GpuProgramType;
using AssetRipper.SourceGenerated.Subclasses.SerializedSubProgram;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class SerializedSubProgramExtensions
	{
		public static ShaderGpuProgramType GetProgramType(this ISerializedSubProgram subProgram, UnityVersion version)
		{
			if (ShaderGpuProgramTypeExtensions.GpuProgramType55Relevant(version))
			{
				return ((ShaderGpuProgramType55)subProgram.GpuProgramType).ToGpuProgramType();
			}
			else
			{
				return ((ShaderGpuProgramType53)subProgram.GpuProgramType).ToGpuProgramType();
			}
		}
	}
}
