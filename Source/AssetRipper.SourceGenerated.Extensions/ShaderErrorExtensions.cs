using AssetRipper.IO.Files;
using AssetRipper.SourceGenerated.Subclasses.ShaderError;

namespace AssetRipper.SourceGenerated.Extensions;

public static class ShaderErrorExtensions
{
	public static BuildTarget GetCompilerPlatform(this IShaderError error)
	{
		return (BuildTarget)(uint)error.CompilerPlatform;
	}
}
