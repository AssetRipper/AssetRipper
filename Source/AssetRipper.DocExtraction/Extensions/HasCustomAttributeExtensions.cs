using AsmResolver.DotNet;

namespace AssetRipper.DocExtraction.Extensions;

internal static class HasCustomAttributeExtensions
{

	public static CustomAttribute? GetCustomAttribute(this IHasCustomAttribute hasCustomAttributeObject, string @namespace, string name)
	{
		return hasCustomAttributeObject.CustomAttributes.Count > 0
			? hasCustomAttributeObject.CustomAttributes
				.FirstOrDefault(attr => MatchesNameAndNamespace(attr.Constructor?.DeclaringType, @namespace, name))
			: null;
	}

	public static bool HasAttribute(this IHasCustomAttribute hasCustomAttributeObject, string @namespace, string name)
	{
		return hasCustomAttributeObject.CustomAttributes.Count > 0
			&& hasCustomAttributeObject.CustomAttributes.Any(attr => MatchesNameAndNamespace(attr.Constructor?.DeclaringType, @namespace, name));
	}

	public static bool TryGetCustomAttribute(this IHasCustomAttribute hasCustomAttributeObject, string @namespace, string name, [NotNullWhen(true)] out CustomAttribute? attribute)
	{
		attribute = hasCustomAttributeObject.GetCustomAttribute(@namespace, name);
		return attribute is not null;
	}

	private static bool MatchesNameAndNamespace(ITypeDefOrRef? type, string @namespace, string name)
	{
		return type is not null && type.Namespace == @namespace && type.Name == name;
	}
}