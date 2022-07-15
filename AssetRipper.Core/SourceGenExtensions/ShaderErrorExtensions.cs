using AssetRipper.Core.Parser.Files;
using AssetRipper.SourceGenerated.Subclasses.ShaderError;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class ShaderErrorExtensions
	{
		public static BuildTarget GetCompilerPlatform(this IShaderError error)
		{
			return (BuildTarget)error.CompilerPlatform;
		}
	}
}
