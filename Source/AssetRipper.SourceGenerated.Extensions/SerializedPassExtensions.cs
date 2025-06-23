using AssetRipper.SourceGenerated.Extensions.Enums.Shader;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader.SerializedShader;
using AssetRipper.SourceGenerated.Subclasses.SerializedPass;
using AssetRipper.SourceGenerated.Subclasses.SerializedProgram;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SerializedPassExtensions
{
	public static SerializedPassType GetType_(this ISerializedPass pass)
	{
		return (SerializedPassType)pass.Type;
	}

	public static bool HasProgram(this ISerializedPass pass, ShaderType type)
	{
		return (pass.ProgramMask & type.ToProgramMask()) != 0;
	}

	public static IEnumerable<ISerializedProgram> GetPrograms(this ISerializedPass pass)
	{
		if (pass.HasProgram(ShaderType.Vertex))
		{
			yield return pass.ProgVertex;
		}
		if (pass.HasProgram(ShaderType.Fragment))
		{
			yield return pass.ProgFragment;
		}
		if (pass.HasProgram(ShaderType.Geometry))
		{
			yield return pass.ProgGeometry;
		}
		if (pass.HasProgram(ShaderType.Hull))
		{
			yield return pass.ProgHull;
		}
		if (pass.HasProgram(ShaderType.Domain))
		{
			yield return pass.ProgDomain;
		}
		if (pass.HasProgram(ShaderType.RayTracing) && pass.Has_ProgRayTracing())
		{
			yield return pass.ProgRayTracing;
		}
	}
}
