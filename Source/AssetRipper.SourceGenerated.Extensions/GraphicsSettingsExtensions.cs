using AssetRipper.SourceGenerated.Classes.ClassID_30;
using AssetRipper.SourceGenerated.NativeEnums.Global;
using GraphicsTier = AssetRipper.SourceGenerated.Enums.GraphicsTier;
using RenderingPath = AssetRipper.SourceGenerated.Enums.RenderingPath;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class GraphicsSettingsExtensions
	{
		public static void ConvertToEditorFormat(this IGraphicsSettings settings)
		{
			settings.BrgStripping_C30 = (int)BrgStrippingMode.KeepIfHybrid;//https://github.com/AssetRipper/TypeTreeDumps/blob/23cd2d4db3bd83a25c57ade3d5126b011422aa3c/FieldValues/2022.3.7f1.json#L761
			settings.DefaultMobileRenderingPath_C30 = (int)RenderingPath.Forward;
			settings.DefaultRenderingPath_C30 = (int)RenderingPath.Forward;
			settings.FogKeepExp_C30 = true;
			settings.FogKeepExp2_C30 = true;
			settings.FogKeepLinear_C30 = true;
			settings.FogStripping_C30 = (int)ShaderStrippingMode.Automatic;
			settings.InstancingStripping_C30 = (int)InstancingStrippingMode.StripUnused;
			settings.LightmapKeepDirCombined_C30 = true;
			settings.LightmapKeepDirSeparate_C30 = true;
			settings.LightmapKeepDynamicDirCombined_C30 = true;
			settings.LightmapKeepDynamicDirSeparate_C30 = true;
			settings.LightmapKeepDynamicPlain_C30 = true;
			settings.LightmapKeepDynamic_C30 = true;
			settings.LightmapKeepPlain_C30 = true;
			settings.LightmapKeepShadowMask_C30 = true;
			settings.LightmapKeepSubtractive_C30 = true;
			settings.LightmapStripping_C30 = (int)ShaderStrippingMode.Automatic;

			if (settings.Has_TierSettings_C30())
			{
				settings.TierSettings_C30.Clear();//protection against converting GraphicsSettings multiple times
				if (settings.Has_TierSettings_Tier1_C30())
				{
					settings.TierSettings_C30.Capacity = 3;
					settings.TierSettings_C30.AddNew().ConvertToEditorFormat(settings.TierSettings_Tier1_C30, settings.GetBuildTargetGroup(), GraphicsTier.Tier1);
					settings.TierSettings_C30.AddNew().ConvertToEditorFormat(settings.TierSettings_Tier2_C30, settings.GetBuildTargetGroup(), GraphicsTier.Tier2);
					settings.TierSettings_C30.AddNew().ConvertToEditorFormat(settings.TierSettings_Tier3_C30, settings.GetBuildTargetGroup(), GraphicsTier.Tier3);
				}
				else if (settings.Has_ShaderSettings_Tier1_C30())
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
			return settings.Collection.Platform.PlatformToBuildGroup();
		}

		/// <summary>
		/// Default: <see cref="RenderingPath.Forward"/>
		/// </summary>
		public static RenderingPath GetDefaultRenderingPath(this IGraphicsSettings settings)
		{
			return settings.Has_DefaultRenderingPath_C30()
				? settings.DefaultRenderingPath_C30E
				: RenderingPath.Forward;
		}

		/// <summary>
		/// Default: <see cref="RenderingPath.Forward"/>
		/// </summary>
		public static RenderingPath GetDefaultMobileRenderingPath(this IGraphicsSettings settings)
		{
			return settings.Has_DefaultMobileRenderingPath_C30()
				? settings.DefaultMobileRenderingPath_C30E
				: RenderingPath.Forward;
		}

		/// <summary>
		/// Default: <see cref="ShaderStrippingMode.Automatic"/>
		/// </summary>
		public static ShaderStrippingMode GetLightmapStripping(this IGraphicsSettings settings)
		{
			return (ShaderStrippingMode)settings.LightmapStripping_C30; //default is 0, so no need to check if present
		}

		/// <summary>
		/// Default: <see cref="ShaderStrippingMode.Automatic"/>
		/// </summary>
		public static ShaderStrippingMode GetFogStripping(this IGraphicsSettings settings)
		{
			return (ShaderStrippingMode)settings.FogStripping_C30; //default is 0, so no need to check if present
		}

		/// <summary>
		/// Default: <see cref="InstancingStrippingMode.StripUnused"/>
		/// </summary>
		public static InstancingStrippingMode GetInstancingStripping(this IGraphicsSettings settings)
		{
			return (InstancingStrippingMode)settings.InstancingStripping_C30; //default is 0, so no need to check if present
		}
	}
}
