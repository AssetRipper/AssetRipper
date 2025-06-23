using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.TierGraphicsSettings;
using AssetRipper.SourceGenerated.Subclasses.TierGraphicsSettingsEditor;
using AssetRipper.SourceGenerated.Subclasses.TierSettings;

namespace AssetRipper.SourceGenerated.Extensions;

public static class TierSettingsExtensions
{
	public static void ConvertToEditorFormat(this ITierSettings settings, ITierGraphicsSettingsEditor tierGraphicsSettingsEditor, BuildTargetGroup buildTarget, GraphicsTier tier)
	{
		settings.Automatic = false;
		settings.SetBuildTarget(buildTarget);
		settings.Settings.CopyValues(tierGraphicsSettingsEditor);
		settings.Tier = (int)tier;
	}

	public static void ConvertToEditorFormat(this ITierSettings settings, ITierGraphicsSettings tierGraphicsSettings, BuildTargetGroup buildTarget, GraphicsTier tier)
	{
		settings.Automatic = false;
		settings.SetBuildTarget(buildTarget);
		settings.Settings.ConvertToEditorFormat(tierGraphicsSettings);
		settings.Tier = (int)tier;
	}

	public static BuildTargetGroup GetBuildTargetAsEnum(this ITierSettings settings)
	{
		if (settings.Has_BuildTarget_Utf8String())
		{
			return StringToBuildGroup(settings.BuildTarget_Utf8String.String);
		}
		else
		{
			return (BuildTargetGroup)settings.BuildTarget_Int32;
		}
	}

	public static string GetBuildTargetAsString(this ITierSettings settings)
	{
		if (settings.Has_BuildTarget_Utf8String())
		{
			return settings.BuildTarget_Utf8String.String;
		}
		else
		{
			return BuildGroupToString((BuildTargetGroup)settings.BuildTarget_Int32);
		}
	}

	public static void SetBuildTarget(this ITierSettings settings, BuildTargetGroup buildTarget)
	{
		if (settings.Has_BuildTarget_Utf8String())
		{
			settings.BuildTarget_Utf8String = BuildGroupToString(buildTarget);
		}
		else
		{
			settings.BuildTarget_Int32 = (int)buildTarget;
		}
	}

	private static BuildTargetGroup StringToBuildGroup(string group)
	{
		return group switch
		{
			"Standalone" => BuildTargetGroup.Standalone,
			"Web" => BuildTargetGroup.WebPlayer,
			"iPhone" => BuildTargetGroup.iPhone,
			"Android" => BuildTargetGroup.Android,
			"WebGL" => BuildTargetGroup.WebGL,
			"Windows Store Apps" => BuildTargetGroup.WSA,
			"Tizen" => BuildTargetGroup.Tizen,
			"PSP2" => BuildTargetGroup.PSP2,
			"PS4" => BuildTargetGroup.PS4,
			"PSM" => BuildTargetGroup.PSM,
			"XboxOne" => BuildTargetGroup.XboxOne,
			"Samsung TV" => BuildTargetGroup.SamsungTV,
			"Nintendo 3DS" => BuildTargetGroup.N3DS,
			"WiiU" => BuildTargetGroup.WiiU,
			"tvOS" => BuildTargetGroup.tvOS,
			_ => BuildTargetGroup.Standalone,
		};
	}

	private static string BuildGroupToString(BuildTargetGroup group)
	{
		return group switch
		{
			BuildTargetGroup.Standalone => "Standalone",
			BuildTargetGroup.WebPlayer => "Web",
			BuildTargetGroup.iPhone => "iPhone",
			BuildTargetGroup.Android => "Android",
			BuildTargetGroup.WebGL => "WebGL",
			BuildTargetGroup.WSA => "Windows Store Apps",
			BuildTargetGroup.Tizen => "Tizen",
			BuildTargetGroup.PSP2 => "PSP2",
			BuildTargetGroup.PS4 => "PS4",
			BuildTargetGroup.PSM => "PSM",
			BuildTargetGroup.XboxOne => "XboxOne",
			BuildTargetGroup.SamsungTV => "Samsung TV",
			BuildTargetGroup.N3DS => "Nintendo 3DS",
			BuildTargetGroup.WiiU => "WiiU",
			BuildTargetGroup.tvOS => "tvOS",
			_ => "Standalone",
		};
	}
}
