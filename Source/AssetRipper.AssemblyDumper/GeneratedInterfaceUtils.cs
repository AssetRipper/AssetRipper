using AssetRipper.Assets;
using System.Reflection;
using MethodAttributes = System.Reflection.MethodAttributes;

namespace AssetRipper.AssemblyDumper;

internal static class GeneratedInterfaceUtils
{
	private static HashSet<string> BlackListedPropertyNamesForSubclassGroup { get; }
		= GetPublicOrProtectedPropertyNames(typeof(UnityAssetBase)).Append(nameof(INamed.Name)).ToHashSet();

	private static HashSet<string> BlackListedPropertyNamesForClassGroup { get; }
		= GetPublicOrProtectedPropertyNames(typeof(UnityObjectBase)).Union(BlackListedPropertyNamesForSubclassGroup).ToHashSet();

	public static string GetPropertyNameFromFieldName(string fieldName, ClassGroupBase group)
	{
		if (IsBlackListed(fieldName, group))
		{
			throw new Exception($"Field uses a blacklisted name");
		}

		string result = fieldName;
		if (result.StartsWith("m_", StringComparison.Ordinal))
		{
			result = result.Substring(2);
		}

		if (result.StartsWith('_'))
		{
			result = $"P{result}";
		}
		else if (char.IsDigit(result[0]))
		{
			result = $"P_{result}";
		}
		else if (char.IsLower(result[0]))
		{
			result = $"{char.ToUpperInvariant(result[0])}{result.Substring(1)}";
		}

		if (!group.IsSealed)
		{
			result = $"{result}_C{group.ID}";
		}

		if (IsBlackListed(result, group))
		{
			result = $"{result}_R";
		}
		else if (result == fieldName)
		{
			result = $"P_{result}";
		}

		return result;
	}

	public static string GetHasMethodName(string propertyNameWithTypeSuffix)
	{
		return $"Has_{propertyNameWithTypeSuffix}";
	}

	public static string GetReleaseOnlyMethodName(string propertyNameWithTypeSuffix)
	{
		return $"IsReleaseOnly_{propertyNameWithTypeSuffix}";
	}

	public static string GetEditorOnlyMethodName(string propertyNameWithTypeSuffix)
	{
		return $"IsEditorOnly_{propertyNameWithTypeSuffix}";
	}

	public static void FillWithSimpleBooleanReturn(this CilInstructionCollection instructions, bool returnTrue)
	{
		if (returnTrue)
		{
			instructions.Add(CilOpCodes.Ldc_I4_1);
		}
		else
		{
			instructions.Add(CilOpCodes.Ldc_I4_0);
		}

		instructions.Add(CilOpCodes.Ret);
	}

	private static bool IsBlackListed(string name, ClassGroupBase group)
	{
		return (group is SubclassGroup
			? BlackListedPropertyNamesForSubclassGroup.Contains(name)
			: BlackListedPropertyNamesForClassGroup.Contains(name))
			|| name == group.Interface.Name
			|| group.Types.Any(t => t.Name == name);
	}

	private static IEnumerable<string> GetPublicOrProtectedPropertyNames(Type type)
	{
		foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
		{
			if (property.GetIndexParameters().Length == 0 && IsPublicOrProtected(property))
			{
				yield return property.Name;
			}
		}

		static bool IsPublicOrProtected(PropertyInfo property)
		{
			foreach (MethodInfo accessor in property.GetAccessors())
			{
				if (IsPublicOrProtected(accessor))
				{
					return true;
				}
			}
			return false;

			static bool IsPublicOrProtected(MethodInfo method)
			{
				return (method.Attributes & MethodAttributes.MemberAccessMask) is MethodAttributes.Public or MethodAttributes.Family or MethodAttributes.FamORAssem;
			}
		}
	}
}
