using AssetRipper.Assets.Generics;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Subclasses.FastPropertyName;
using AssetRipper.SourceGenerated.Subclasses.UnityPropertySheet;
using AssetRipper.SourceGenerated.Subclasses.UnityTexEnv;
using System.Collections;

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

	public static IReadOnlyList<KeyValuePair<Utf8String, IUnityTexEnv>> GetTextureProperties(this IMaterial material)
	{
		IUnityPropertySheet savedProperties = material.SavedProperties_C21;
		if (savedProperties.Has_TexEnvs_AssetDictionary_Utf8String_UnityTexEnv_5())
		{
			return new TexEnvsList<Utf8String, UnityTexEnv_5>(savedProperties.TexEnvs_AssetDictionary_Utf8String_UnityTexEnv_5);
		}
		else if (savedProperties.Has_TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_5())
		{
			return new TexEnvsList<FastPropertyName, UnityTexEnv_5>(savedProperties.TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_5);
		}
		else if (savedProperties.Has_TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_3_5())
		{
			return new TexEnvsList<FastPropertyName, UnityTexEnv_3_5>(savedProperties.TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_3_5);
		}
		else
		{
			return [];
		}
	}

	private sealed class TexEnvsList<TString, TTexture>(AssetDictionary<TString, TTexture> dictionary) : IReadOnlyList<KeyValuePair<Utf8String, IUnityTexEnv>>
		where TString : notnull, new()
		where TTexture : IUnityTexEnv, new()
	{
		public KeyValuePair<Utf8String, IUnityTexEnv> this[int index] => ConvertPair(dictionary.GetPair(index));

		public int Count => dictionary.Count;

		private static Utf8String ConvertKey(TString key)
		{
			if (typeof(TString) == typeof(Utf8String))
			{
				return (Utf8String)(object)key;
			}
			else if (typeof(TString) == typeof(FastPropertyName))
			{
				return ((FastPropertyName)(object)key).Name;
			}
			else
			{
				return Utf8String.Empty; // Unreachable
			}
		}

		private static KeyValuePair<Utf8String, IUnityTexEnv> ConvertPair(AccessPairBase<TString, TTexture> pair)
		{
			return new(ConvertKey(pair.Key), pair.Value);
		}

		public IEnumerator<KeyValuePair<Utf8String, IUnityTexEnv>> GetEnumerator()
		{
			foreach (AccessPairBase<TString, TTexture> pair in dictionary)
			{
				yield return ConvertPair(pair);
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
