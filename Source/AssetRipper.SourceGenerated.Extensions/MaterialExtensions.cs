using AssetRipper.Assets.Generics;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Subclasses.FastPropertyName;
using AssetRipper.SourceGenerated.Subclasses.UnityPropertySheet;
using AssetRipper.SourceGenerated.Subclasses.UnityTexEnv;

namespace AssetRipper.SourceGenerated.Extensions;

public static class MaterialExtensions
{
	public static string? FindPropertyNameByCRC28(this IMaterial material, uint crc)
	{
		return material.SavedProperties_C21.FindPropertyNameByCRC28(crc);
	}

	public static bool TryGetTextureProperty(this IMaterial material, string propertyName, [NotNullWhen(true)] out IUnityTexEnv? property)
	{
		IUnityPropertySheet savedProperties = material.SavedProperties_C21;
		if (savedProperties.Has_TexEnvs_AssetDictionary_Utf8String_UnityTexEnv_5())
		{
			savedProperties.TexEnvs_AssetDictionary_Utf8String_UnityTexEnv_5.TryGetValue((Utf8String)propertyName, out UnityTexEnv_5? texEnv);
			property = texEnv;
		}
		else if (savedProperties.Has_TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_5())
		{
			savedProperties.TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_5.TryGetValue(new() { Name = propertyName }, out UnityTexEnv_5? texEnv);
			property = texEnv;
		}
		else if (savedProperties.Has_TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_3_5())
		{
			savedProperties.TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_3_5.TryGetValue(new() { Name = propertyName }, out UnityTexEnv_3_5? texEnv);
			property = texEnv;
		}
		else
		{
			property = null;
		}
		return property is not null;
	}

	public static IEnumerable<KeyValuePair<Utf8String, IUnityTexEnv>> GetTextureProperties(this IMaterial material)
	{
		IUnityPropertySheet savedProperties = material.SavedProperties_C21;
		if (savedProperties.Has_TexEnvs_AssetDictionary_Utf8String_UnityTexEnv_5())
		{
			return savedProperties
				.TexEnvs_AssetDictionary_Utf8String_UnityTexEnv_5
				.Select((AccessPairBase<Utf8String, UnityTexEnv_5> pair) => new KeyValuePair<Utf8String, IUnityTexEnv>(pair.Key, pair.Value));
		}
		else if (savedProperties.Has_TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_5())
		{
			return savedProperties
				.TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_5
				.Select((AccessPairBase<FastPropertyName, UnityTexEnv_5> pair) => new KeyValuePair<Utf8String, IUnityTexEnv>(pair.Key.Name, pair.Value));
		}
		else if (savedProperties.Has_TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_3_5())
		{
			return savedProperties
				.TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_3_5
				.Select((AccessPairBase<FastPropertyName, UnityTexEnv_3_5> pair) => new KeyValuePair<Utf8String, IUnityTexEnv>(pair.Key.Name, pair.Value));
		}
		else
		{
			return Enumerable.Empty<KeyValuePair<Utf8String, IUnityTexEnv>>();
		}
	}
}
