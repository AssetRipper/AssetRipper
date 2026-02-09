using AssetRipper.SourceGenerated.Classes.ClassID_30;
using AssetRipper.SourceGenerated.NativeEnums.Global;
using GraphicsTier = AssetRipper.SourceGenerated.Enums.GraphicsTier;
using RenderingPath = AssetRipper.SourceGenerated.Enums.RenderingPath;

namespace AssetRipper.SourceGenerated.Extensions;

public static class GraphicsSettingsExtensions
{
	public static void ConvertToEditorFormat(this IGraphicsSettings settings)
	{
		settings.BrgStripping = (int)BrgStrippingMode.KeepIfHybrid;//https://github.com/AssetRipper/TypeTreeDumps/blob/23cd2d4db3bd83a25c57ade3d5126b011422aa3c/FieldValues/2022.3.7f1.json#L761
		settings.DefaultMobileRenderingPath = (int)RenderingPath.Forward;
		settings.DefaultRenderingPath = (int)RenderingPath.Forward;
		settings.FogKeepExp = true;
		settings.FogKeepExp2 = true;
		settings.FogKeepLinear = true;
		settings.FogStripping = (int)ShaderStrippingMode.Automatic;
		settings.InstancingStripping = (int)InstancingStrippingMode.StripUnused;
		settings.LightmapKeepDirCombined = true;
		settings.LightmapKeepDirSeparate = true;
		settings.LightmapKeepDynamicDirCombined = true;
		settings.LightmapKeepDynamicDirSeparate = true;
		settings.LightmapKeepDynamicPlain = true;
		settings.LightmapKeepDynamic = true;
		settings.LightmapKeepPlain = true;
		settings.LightmapKeepShadowMask = true;
		settings.LightmapKeepSubtractive = true;
		settings.LightmapStripping = (int)ShaderStrippingMode.Automatic;

		if (settings.Has_TierSettings())
		{
			settings.TierSettings.Clear();//protection against converting GraphicsSettings multiple times
			if (settings.Has_TierSettings_Tier1())
			{
				settings.TierSettings.Capacity = 3;
				settings.TierSettings.AddNew().ConvertToEditorFormat(settings.TierSettings_Tier1, settings.GetBuildTargetGroup(), GraphicsTier.Tier1);
				settings.TierSettings.AddNew().ConvertToEditorFormat(settings.TierSettings_Tier2, settings.GetBuildTargetGroup(), GraphicsTier.Tier2);
				settings.TierSettings.AddNew().ConvertToEditorFormat(settings.TierSettings_Tier3, settings.GetBuildTargetGroup(), GraphicsTier.Tier3);
			}
			else if (settings.Has_ShaderSettings_Tier1())
			{
				settings.TierSettings.Capacity = 3;
				settings.ShaderSettings_Tier1.ConvertToEditorFormat();
				settings.ShaderSettings_Tier2.ConvertToEditorFormat();
				settings.ShaderSettings_Tier2.ConvertToEditorFormat();
				settings.TierSettings.AddNew().ConvertToEditorFormat(settings.ShaderSettings_Tier1, settings.GetBuildTargetGroup(), GraphicsTier.Tier1);
				settings.TierSettings.AddNew().ConvertToEditorFormat(settings.ShaderSettings_Tier2, settings.GetBuildTargetGroup(), GraphicsTier.Tier2);
				settings.TierSettings.AddNew().ConvertToEditorFormat(settings.ShaderSettings_Tier3, settings.GetBuildTargetGroup(), GraphicsTier.Tier3);
			}
			else if (settings.Has_ShaderSettings())
			{
				settings.TierSettings.Capacity = 1;
				settings.ShaderSettings.ConvertToEditorFormat();
				settings.TierSettings.AddNew().ConvertToEditorFormat(settings.ShaderSettings, settings.GetBuildTargetGroup(), GraphicsTier.Tier1);
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
		return settings.Has_DefaultRenderingPath()
			? settings.DefaultRenderingPathE
			: RenderingPath.Forward;
	}

	/// <summary>
	/// Default: <see cref="RenderingPath.Forward"/>
	/// </summary>
	public static RenderingPath GetDefaultMobileRenderingPath(this IGraphicsSettings settings)
	{
		return settings.Has_DefaultMobileRenderingPath()
			? settings.DefaultMobileRenderingPathE
			: RenderingPath.Forward;
	}

	/// <summary>
	/// Default: <see cref="ShaderStrippingMode.Automatic"/>
	/// </summary>
	public static ShaderStrippingMode GetLightmapStripping(this IGraphicsSettings settings)
	{
		return (ShaderStrippingMode)settings.LightmapStripping; //default is 0, so no need to check if present
	}

	/// <summary>
	/// Default: <see cref="ShaderStrippingMode.Automatic"/>
	/// </summary>
	public static ShaderStrippingMode GetFogStripping(this IGraphicsSettings settings)
	{
		return (ShaderStrippingMode)settings.FogStripping; //default is 0, so no need to check if present
	}

	/// <summary>
	/// Default: <see cref="InstancingStrippingMode.StripUnused"/>
	/// </summary>
	public static InstancingStrippingMode GetInstancingStripping(this IGraphicsSettings settings)
	{
		return (InstancingStrippingMode)settings.InstancingStripping; //default is 0, so no need to check if present
	}
}
