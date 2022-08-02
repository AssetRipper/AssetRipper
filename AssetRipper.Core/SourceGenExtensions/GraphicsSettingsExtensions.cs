using AssetRipper.Core.Classes.Camera;
using AssetRipper.Core.Classes.GraphicsSettings;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.SourceGenerated.Classes.ClassID_30;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class GraphicsSettingsExtensions
	{
		public static void ConvertToEditorFormat(this IGraphicsSettings settings)
		{
			settings.DefaultMobileRenderingPath_C30 = (int)RenderingPath.Forward;
			settings.DefaultRenderingPath_C30 = (int)RenderingPath.Forward;
			settings.FogKeepExp_C30 = true;
			settings.FogKeepExp2_C30 = true;
			settings.FogKeepLinear_C30 = true;
			settings.FogStripping_C30 = (int)LightmapStrippingMode.Automatic;
			settings.InstancingStripping_C30 = (int)InstancingStrippingVariant.StripUnused;
			settings.LightmapKeepDirCombined_C30 = true;
			settings.LightmapKeepDirSeparate_C30 = true;
			settings.LightmapKeepDynamicDirCombined_C30 = true;
			settings.LightmapKeepDynamicDirSeparate_C30 = true;
			settings.LightmapKeepDynamicPlain_C30 = true;
			settings.LightmapKeepDynamic_C30 = true;
			settings.LightmapKeepPlain_C30 = true;
			settings.LightmapKeepShadowMask_C30 = true;
			settings.LightmapKeepSubtractive_C30 = true;
			settings.LightmapStripping_C30 = (int)LightmapStrippingMode.Automatic;

			if (settings.Has_TierSettings_C30())
			{
				settings.TierSettings_C30.Clear();//protection against converting GraphicsSettings multiple times
				if (settings.Has_TierSettings_Tier1_C30() && settings.Has_TierSettings_Tier2_C30() && settings.Has_TierSettings_Tier3_C30())
				{
					settings.TierSettings_C30.Capacity = 3;
					settings.TierSettings_C30.AddNew().ConvertToEditorFormat(settings.TierSettings_Tier1_C30, settings.GetBuildTargetGroup(), GraphicsTier.Tier1);
					settings.TierSettings_C30.AddNew().ConvertToEditorFormat(settings.TierSettings_Tier2_C30, settings.GetBuildTargetGroup(), GraphicsTier.Tier2);
					settings.TierSettings_C30.AddNew().ConvertToEditorFormat(settings.TierSettings_Tier3_C30, settings.GetBuildTargetGroup(), GraphicsTier.Tier3);
				}
				else if (settings.Has_ShaderSettings_Tier1_C30() && settings.Has_ShaderSettings_Tier2_C30() && settings.Has_ShaderSettings_Tier3_C30())
				{
					settings.TierSettings_C30.Capacity = 3;
					settings.ShaderSettings_Tier1_C30.ConvertToEditorFormat();
					settings.ShaderSettings_Tier2_C30.ConvertToEditorFormat();
					settings.ShaderSettings_Tier2_C30.ConvertToEditorFormat();
					settings.TierSettings_C30.AddNew().ConvertToEditorFormat(settings.ShaderSettings_Tier1_C30, settings.GetBuildTargetGroup(), GraphicsTier.Tier1);
					settings.TierSettings_C30.AddNew().ConvertToEditorFormat(settings.ShaderSettings_Tier2_C30, settings.GetBuildTargetGroup(), GraphicsTier.Tier2);
					settings.TierSettings_C30.AddNew().ConvertToEditorFormat(settings.ShaderSettings_Tier3_C30, settings.GetBuildTargetGroup(), GraphicsTier.Tier3);
				}
				else if (settings.Has_ShaderSettings_C30())
				{
					settings.TierSettings_C30.Capacity = 1;
					settings.ShaderSettings_C30.ConvertToEditorFormat();
					settings.TierSettings_C30.AddNew().ConvertToEditorFormat(settings.ShaderSettings_C30, settings.GetBuildTargetGroup(), GraphicsTier.Tier1);
				}
			}
		}

		private static BuildTargetGroup GetBuildTargetGroup(this IGraphicsSettings settings)
		{
			return settings.AssetInfo is not null ? settings.AssetInfo.File.Platform.PlatformToBuildGroup() : BuildTargetGroup.Standalone;
		}

		public static TransparencySortMode GetTransparencySortMode(this IGraphicsSettings settings)
		{
			return (TransparencySortMode)settings.TransparencySortMode_C30;
		}

		/// <summary>
		/// Default: <see cref="RenderingPath.Forward"/>
		/// </summary>
		public static RenderingPath GetDefaultRenderingPath(this IGraphicsSettings settings)
		{
			return settings.Has_DefaultRenderingPath_C30()
				? (RenderingPath)settings.DefaultRenderingPath_C30
				: RenderingPath.Forward;
		}

		/// <summary>
		/// Default: <see cref="RenderingPath.Forward"/>
		/// </summary>
		public static RenderingPath GetDefaultMobileRenderingPath(this IGraphicsSettings settings)
		{
			return settings.Has_DefaultMobileRenderingPath_C30()
				? (RenderingPath)settings.DefaultMobileRenderingPath_C30
				: RenderingPath.Forward;
		}

		/// <summary>
		/// Default: <see cref="LightmapStrippingMode.Automatic"/>
		/// </summary>
		public static LightmapStrippingMode GetLightmapStripping(this IGraphicsSettings settings)
		{
			return (LightmapStrippingMode)settings.LightmapStripping_C30; //default is 0, so no need to check if present
		}

		/// <summary>
		/// Default: <see cref="LightmapStrippingMode.Automatic"/>
		/// </summary>
		public static LightmapStrippingMode GetFogStripping(this IGraphicsSettings settings)
		{
			return (LightmapStrippingMode)settings.FogStripping_C30; //default is 0, so no need to check if present
		}

		/// <summary>
		/// Default: <see cref="InstancingStrippingVariant.StripUnused"/>
		/// </summary>
		public static InstancingStrippingVariant GetInstancingStripping(this IGraphicsSettings settings)
		{
			return (InstancingStrippingVariant)settings.InstancingStripping_C30; //default is 0, so no need to check if present
		}
	}
}
