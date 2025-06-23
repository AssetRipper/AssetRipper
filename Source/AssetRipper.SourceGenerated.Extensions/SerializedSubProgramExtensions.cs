using AssetRipper.SourceGenerated.Extensions.Enums.Shader.GpuProgramType;
using AssetRipper.SourceGenerated.Subclasses.SerializedSubProgram;

namespace AssetRipper.SourceGenerated.Extensions;

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
