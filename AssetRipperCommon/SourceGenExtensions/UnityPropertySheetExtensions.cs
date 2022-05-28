using AssetRipper.Core.Utils;
using AssetRipper.SourceGenerated.Subclasses.UnityPropertySheet;
using AssetRipper.SourceGenerated.Subclasses.Utf8String;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class UnityPropertySheetExtensions
	{
		private const string HDRPostfixName = "_HDR";
		private const string STPostfixName = "_ST";
		private const string TexelSizePostfixName = "_TexelSize";

		public static string? FindPropertyNameByCRC28(this IUnityPropertySheet sheet, uint crc)
		{
			foreach (Utf8String property in sheet.GetTexEnvNames())
			{
				string propertyString = property.String;
				string hdrName = propertyString + HDRPostfixName;
				if (CrcUtils.Verify28DigestUTF8(hdrName, crc))
				{
					return hdrName;
				}
				string stName = propertyString + STPostfixName;
				if (CrcUtils.Verify28DigestUTF8(stName, crc))
				{
					return stName;
				}
				string texelName = propertyString + TexelSizePostfixName;
				if (CrcUtils.Verify28DigestUTF8(texelName, crc))
				{
					return texelName;
				}
			}
			foreach (Utf8String property in sheet.GetFloatNames())
			{
				string propertyString = property.String;
				if (CrcUtils.Verify28DigestUTF8(propertyString, crc))
				{
					return propertyString;
				}
			}
			foreach (Utf8String property in sheet.GetIntNames())
			{
				string propertyString = property.String;
				if (CrcUtils.Verify28DigestUTF8(propertyString, crc))
				{
					return propertyString;
				}
			}
			foreach (Utf8String property in sheet.GetColorNames())
			{
				string propertyString = property.String;
				if (CrcUtils.Verify28DigestUTF8(propertyString, crc))
				{
					return propertyString;
				}
			}
			return null;
		}

		private static IEnumerable<Utf8String> GetTexEnvNames(this IUnityPropertySheet sheet)
		{
			if (sheet.Has_TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_3_0_0_f5())
			{
				return sheet.TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_3_0_0_f5.Keys.Select(n => n.Name);
			}
			else if (sheet.Has_TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_3_5_0_f5())
			{
				return sheet.TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_3_5_0_f5.Keys.Select(n => n.Name);
			}
			else if (sheet.Has_TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_5_0_0_f4())
			{
				return sheet.TexEnvs_AssetDictionary_FastPropertyName_UnityTexEnv_5_0_0_f4.Keys.Select(n => n.Name);
			}
			else if (sheet.Has_TexEnvs_AssetDictionary_Utf8String_UnityTexEnv_5_0_0_f4())
			{
				return sheet.TexEnvs_AssetDictionary_Utf8String_UnityTexEnv_5_0_0_f4.Keys;
			}
			else
			{
				throw new NotSupportedException(sheet.GetType().FullName);
			}
		}

		private static IEnumerable<Utf8String> GetColorNames(this IUnityPropertySheet sheet)
		{
			if (sheet.Has_Colors_AssetDictionary_FastPropertyName_ColorRGBAf_3_0_0_f5())
			{
				return sheet.Colors_AssetDictionary_FastPropertyName_ColorRGBAf_3_0_0_f5.Keys.Select(n => n.Name);
			}
			else if (sheet.Has_Colors_AssetDictionary_FastPropertyName_ColorRGBAf_3_5_0_f5())
			{
				return sheet.Colors_AssetDictionary_FastPropertyName_ColorRGBAf_3_5_0_f5.Keys.Select(n => n.Name);
			}
			else if (sheet.Has_Colors_AssetDictionary_Utf8String_ColorRGBAf_3_5_0_f5())
			{
				return sheet.Colors_AssetDictionary_Utf8String_ColorRGBAf_3_5_0_f5.Keys;
			}
			else
			{
				throw new NotSupportedException(sheet.GetType().FullName);
			}
		}

		private static IEnumerable<Utf8String> GetFloatNames(this IUnityPropertySheet sheet)
		{
			if (sheet.Has_Floats_AssetDictionary_FastPropertyName_Single())
			{
				return sheet.Floats_AssetDictionary_FastPropertyName_Single.Keys.Select(n => n.Name);
			}
			else if (sheet.Has_Floats_AssetDictionary_Utf8String_Single())
			{
				return sheet.Floats_AssetDictionary_Utf8String_Single.Keys;
			}
			else
			{
				throw new NotSupportedException(sheet.GetType().FullName);
			}
		}

		private static IEnumerable<Utf8String> GetIntNames(this IUnityPropertySheet sheet)
		{
			if (sheet.Has_Ints())
			{
				return sheet.Ints.Keys;
			}
			else
			{
				return Enumerable.Empty<Utf8String>();
			}
		}
	}
}
