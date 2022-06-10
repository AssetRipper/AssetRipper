using AssetRipper.Core.Classes.Shader.Enums;
using AssetRipper.SourceGenerated.Subclasses.PlatformShaderDefines;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class PlatformShaderDefinesExtensions
	{
		public static GPUPlatform GetSerializationMode(this IPlatformShaderDefines settings)
		{
			return (GPUPlatform)settings.ShaderPlatform;
		}
	}
}
