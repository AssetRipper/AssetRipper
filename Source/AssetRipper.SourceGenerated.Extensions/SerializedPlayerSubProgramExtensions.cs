using AssetRipper.SourceGenerated.Extensions.Enums.Shader.GpuProgramType;
using AssetRipper.SourceGenerated.Subclasses.SerializedPlayerSubProgram;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SerializedPlayerSubProgramExtensions
{
	public static ShaderGpuProgramType GetProgramType(this ISerializedPlayerSubProgram subProgram, UnityVersion version)
	{
		return ((ShaderGpuProgramType55)subProgram.GpuProgramType).ToGpuProgramType();
	}
}
