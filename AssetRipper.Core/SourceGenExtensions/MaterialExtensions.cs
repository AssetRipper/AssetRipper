using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Subclasses.UnityPropertySheet;
using AssetRipper.SourceGenerated.Subclasses.UnityTexEnv;
using AssetRipper.SourceGenerated.Subclasses.Utf8String;
using System.Diagnostics.CodeAnalysis;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class MaterialExtensions
	{
		public static string? FindPropertyNameByCRC28(this IMaterial material, uint crc)
		{
			return material.SavedProperties_C21.FindPropertyNameByCRC28(crc);
		}

		public static bool TryGetTextureProperty(this IMaterial material, string propertyName, [NotNullWhen(true)] out IUnityTexEnv? property)
		{
			IUnityPropertySheet savedProperties = material.SavedProperties_C21;
			if (savedProperties.Has_TexEnvs_AssetDictionary_Utf8String_UnityTexEnv_5_0_0_f4())
			{
				savedProperties.TexEnvs_AssetDictionary_Utf8String_UnityTexEnv_5_0_0_f4.TryGetValue((Utf8String)propertyName, out UnityTexEnv_5_0_0_f4? texEnv);
				property = texEnv;
			}
			else if (savedProperties.Has_TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_5_0_0_f4())
			{
				savedProperties.TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_5_0_0_f4.TryGetValue(new() { NameString = propertyName }, out UnityTexEnv_5_0_0_f4? texEnv);
				property = texEnv;
			}
			else if (savedProperties.Has_TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_3_5_0_f5())
			{
				savedProperties.TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_3_5_0_f5.TryGetValue(new() { NameString = propertyName }, out UnityTexEnv_3_5_0_f5? texEnv);
				property = texEnv;
			}
			else if (savedProperties.Has_TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_3_0_0_f5())
			{
				savedProperties.TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_3_0_0_f5.TryGetValue(new() { NameString = propertyName }, out UnityTexEnv_3_0_0_f5? texEnv);
				property = texEnv;
			}
			else
			{
				property = null;
			}
			return property is not null;
		}
	}
}
