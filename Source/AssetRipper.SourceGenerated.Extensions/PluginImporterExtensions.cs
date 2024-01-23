using AssetRipper.Assets.Generics;
using AssetRipper.SourceGenerated.Classes.ClassID_1050;
using AssetRipper.SourceGenerated.Subclasses.PlatformSettingsData_Plugin;

namespace AssetRipper.SourceGenerated.Extensions;

public static class PluginImporterExtensions
{
	public static bool HasPlatformData(this IPluginImporter importer)
	{
		return importer.Has_PlatformData_AssetDictionary_AssetPair_Utf8String_Utf8String_PlatformSettingsData_Plugin()
			|| importer.Has_PlatformData_AssetDictionary_Utf8String_PlatformSettingsData_Plugin();
	}

	public static PlatformSettingsData_Plugin AddPlatformSettings(this IPluginImporter importer, Utf8String platformKey, Utf8String platformValue)
	{
		if (importer.Has_PlatformData_AssetDictionary_AssetPair_Utf8String_Utf8String_PlatformSettingsData_Plugin())
		{
			(AssetPair<Utf8String, Utf8String> pair, PlatformSettingsData_Plugin data)
				= importer.PlatformData_AssetDictionary_AssetPair_Utf8String_Utf8String_PlatformSettingsData_Plugin.AddNew();
			pair.Key = platformKey;
			pair.Value = platformValue;
			return data;
		}
		else if (importer.Has_PlatformData_AssetDictionary_Utf8String_PlatformSettingsData_Plugin())
		{
			AssetPair<Utf8String, PlatformSettingsData_Plugin> pair
				= importer.PlatformData_AssetDictionary_Utf8String_PlatformSettingsData_Plugin.AddNew();

			pair.Key = platformKey;
			return pair.Value;
		}
		else
		{
			throw new InvalidOperationException();
		}
	}
}
