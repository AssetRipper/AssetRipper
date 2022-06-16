using AssetRipper.Core.Classes.GraphicsSettings;
using AssetRipper.SourceGenerated.Subclasses.TierGraphicsSettingsEditor;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class TierGraphicsSettingsEditorExtensions
	{
		public static void ConvertToEditorFormat(this ITierGraphicsSettingsEditor settings)
		{
			settings.StandardShaderQuality = (int)ShaderQuality.High;
			settings.UseReflectionProbeBoxProjection = true;
			settings.UseReflectionProbeBlending = true;
		}

		public static ShaderQuality GetStandardShaderQuality(this ITierGraphicsSettingsEditor settings)
		{
			return (ShaderQuality)settings.StandardShaderQuality;
		}
	}
}
