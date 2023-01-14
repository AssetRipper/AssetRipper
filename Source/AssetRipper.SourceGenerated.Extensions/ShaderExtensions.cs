using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Extensions.Enums.Shader;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class ShaderExtensions
	{
		public static IEnumerable<GPUPlatform>? GetPlatforms(this IShader shader)
		{
			return shader.Platforms_C48?.Select(p => unchecked((GPUPlatform)p));
		}
	}
}
