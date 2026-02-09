using AssetRipper.SourceGenerated.Extensions.Enums.Shader;
using AssetRipper.SourceGenerated.Subclasses.PlatformShaderDefines;

namespace AssetRipper.SourceGenerated.Extensions;

public static class PlatformShaderDefinesExtensions
{
	public static GPUPlatform GetSerializationMode(this IPlatformShaderDefines settings)
	{
		return (GPUPlatform)settings.ShaderPlatform;
	}
}
