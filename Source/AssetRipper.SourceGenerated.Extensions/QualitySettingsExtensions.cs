using AssetRipper.Assets.Generics;
using AssetRipper.SourceGenerated.Classes.ClassID_47;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions;

public static class QualitySettingsExtensions
{
	public static void ConvertToEditorFormat(this IQualitySettings settings)
	{
		settings.PerPlatformDefaultQuality?.SetDefaultPlatformQuality();
	}

	private static void SetDefaultPlatformQuality(this AssetDictionary<Utf8String, int> dictionary)
	{
		dictionary.Clear();
		dictionary.Capacity = 14;

		dictionary.Add(BuildTargetGroup.Android, QualityLevel.Simple);
		dictionary.Add(BuildTargetGroup.N3DS, QualityLevel.Fantastic);
		dictionary.Add(BuildTargetGroup.Switch, QualityLevel.Fantastic);
		dictionary.Add(BuildTargetGroup.PS4, QualityLevel.Fantastic);
		dictionary.Add(BuildTargetGroup.PSM, QualityLevel.Fantastic);
		dictionary.Add(BuildTargetGroup.PSP2, QualityLevel.Simple);
		dictionary.Add(BuildTargetGroup.Standalone, QualityLevel.Fantastic);
		dictionary.Add(BuildTargetGroup.Tizen, QualityLevel.Simple);
		dictionary.Add(BuildTargetGroup.WebGL, QualityLevel.Good);
		dictionary.Add(BuildTargetGroup.WiiU, QualityLevel.Fantastic);
		dictionary.Add(BuildTargetGroup.Metro, QualityLevel.Fantastic);
		dictionary.Add(BuildTargetGroup.XboxOne, QualityLevel.Fantastic);
		dictionary.Add(BuildTargetGroup.iOS, QualityLevel.Simple);
		dictionary.Add(BuildTargetGroup.tvOS, QualityLevel.Simple);
	}

	private static void Add(this AssetDictionary<Utf8String, int> dictionary, BuildTargetGroup buildTargetGroup, QualityLevel qualityLevel)
	{
		Utf8String str = buildTargetGroup.ToExportString();
		dictionary.Add(str, (int)qualityLevel);
	}
}
