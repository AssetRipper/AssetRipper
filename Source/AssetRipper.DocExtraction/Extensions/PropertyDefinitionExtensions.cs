using AsmResolver.DotNet;

namespace AssetRipper.DocExtraction.Extensions;

public static class PropertyDefinitionExtensions
{
	public static bool HasParameters(this PropertyDefinition property)
	{
		return property.Signature?.ParameterTypes.Count > 0;
	}

	public static bool IsPublic(this PropertyDefinition property)
	{
		return (property.GetMethod?.IsPublic ?? false) || (property.SetMethod?.IsPublic ?? false);
	}

	public static bool IsStatic(this PropertyDefinition property)
	{
		return (property.GetMethod?.IsStatic ?? false) || (property.SetMethod?.IsStatic ?? false);
	}

	public static bool IsValueType(this PropertyDefinition property)
	{
		return property.Signature?.ReturnType.IsValueType ?? throw new NullReferenceException("Property signature");
	}
}