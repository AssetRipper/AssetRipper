using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class ShaderExtensions
	{
		public static IEnumerable<GPUPlatform>? GetPlatforms(this IShader shader)
		{
			return shader.Platforms_C48?.Select(p => unchecked((GPUPlatform)p));
		}
	}
}
