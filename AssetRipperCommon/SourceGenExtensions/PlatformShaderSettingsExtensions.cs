using AssetRipper.Core.Classes.GraphicsSettings;
using AssetRipper.SourceGenerated.Subclasses.PlatformShaderSettings;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class PlatformShaderSettingsExtensions
	{
		public static void ConvertToEditorFormat(this IPlatformShaderSettings settings)
		{
			settings.StandardShaderQuality = (int)ShaderQuality.High;
			settings.UseReflectionProbeBoxProjection = true;
			settings.UseReflectionProbeBlending = true;
		}

		public static ShaderQuality GetStandardShaderQuality(this IPlatformShaderSettings settings)
		{
			return (ShaderQuality)settings.StandardShaderQuality;
		}
	}
}
