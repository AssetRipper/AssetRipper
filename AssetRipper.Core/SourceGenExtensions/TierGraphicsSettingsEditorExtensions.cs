using AssetRipper.Core.Classes.Camera;
using AssetRipper.Core.Classes.GraphicsSettings;
using AssetRipper.SourceGenerated.Subclasses.TierGraphicsSettings;
using AssetRipper.SourceGenerated.Subclasses.TierGraphicsSettingsEditor;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class TierGraphicsSettingsEditorExtensions
	{
		public static void ConvertToEditorFormat(this ITierGraphicsSettingsEditor settings)
		{
			settings.StandardShaderQuality = (int)ShaderQuality.High;
			settings.RenderingPath = (int)RenderingPath.Forward;
			settings.HdrMode = (int)CameraHDRMode.FP16;
			settings.RealtimeGICPUUsage = (int)RealtimeGICPUUsage.Low;
			settings.UseReflectionProbeBoxProjection = true;
			settings.UseReflectionProbeBlending = true;
			settings.UseHDR = true;
			settings.UseDetailNormalMap = true;
			settings.UseCascadedShadowMaps = true;
			settings.Prefer32BitShadowMaps = false;
			settings.EnableLPPV = true;
			settings.UseDitherMaskForAlphaBlendedShadows = true;
		}

		public static void ConvertToEditorFormat(this ITierGraphicsSettingsEditor settings, ITierGraphicsSettings tierGraphicsSettings)
		{
			settings.StandardShaderQuality = (int)ShaderQuality.High;
			settings.RenderingPath = tierGraphicsSettings.RenderingPath;
			settings.HdrMode = tierGraphicsSettings.Has_HdrMode()
				? tierGraphicsSettings.HdrMode
				: (int)CameraHDRMode.FP16;
			settings.RealtimeGICPUUsage = tierGraphicsSettings.Has_RealtimeGICPUUsage()
				? tierGraphicsSettings.RealtimeGICPUUsage
				: (int)RealtimeGICPUUsage.Low;
			settings.UseReflectionProbeBoxProjection = true;
			settings.UseReflectionProbeBlending = true;
			settings.UseHDR = !tierGraphicsSettings.Has_UseHDR() || tierGraphicsSettings.UseHDR;
			settings.UseDetailNormalMap = true;
			settings.UseCascadedShadowMaps = tierGraphicsSettings.UseCascadedShadowMaps;
			settings.Prefer32BitShadowMaps = tierGraphicsSettings.Prefer32BitShadowMaps;
			settings.EnableLPPV = !tierGraphicsSettings.Has_EnableLPPV() || tierGraphicsSettings.EnableLPPV;
			settings.UseDitherMaskForAlphaBlendedShadows = true;
		}

		public static ShaderQuality GetStandardShaderQuality(this ITierGraphicsSettingsEditor settings)
		{
			return (ShaderQuality)settings.StandardShaderQuality;
		}

		public static RenderingPath GetRenderingPath(this ITierGraphicsSettingsEditor settings)
		{
			return (RenderingPath)settings.RenderingPath;
		}

		public static CameraHDRMode GetHdrMode(this ITierGraphicsSettingsEditor settings)
		{
			return (CameraHDRMode)settings.HdrMode;
		}

		public static RealtimeGICPUUsage GetRealtimeGICPUUsage(this ITierGraphicsSettingsEditor settings)
		{
			return (RealtimeGICPUUsage)settings.RealtimeGICPUUsage;
		}
	}
}
