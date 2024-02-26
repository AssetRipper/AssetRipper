using AsmResolver.DotNet;

internal static class TypeDefinitionExtensions
{
	public static string GetTypeCategory(this TypeDefinition type)
	{
		if (type.IsClass)
		{
			return "class";
		}
		else if (type.IsInterface)
		{
			return "interface";
		}
		else if (type.IsEnum)
		{
			return "enum";
		}
		else if (type.IsValueType)
		{
			return "struct";
		}
		else if (type.IsDelegate)
		{
			return "delegate";
		}
		throw new NotSupportedException($"Unsupported type category for type {type.FullName}");
	}

	public static bool TryGetInheritanceModifier(this TypeDefinition type, [NotNullWhen(true)] out string? modifier)
	{
		if (type.IsSealed)
		{
			modifier = type.IsAbstract ? "static" : "sealed";
			return true;
		}
		else if (type.IsAbstract)
		{
			modifier = "abstract";
			return true;
		}
		modifier = null;
		return false;
	}
}
