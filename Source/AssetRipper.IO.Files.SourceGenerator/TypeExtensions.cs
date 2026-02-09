namespace AssetRipper.IO.Files.SourceGenerator;

internal static class TypeExtensions
{
	public static string GetGlobalQualifiedName(this Type type)
	{
		if (type == typeof(void))
		{
			return "void";
		}
		else if (type.IsGenericType)
		{
			// Handle generic types by appending generic arguments
			string genericTypeDefinition = type.GetGenericTypeDefinition().FullName!;
			string genericArguments = string.Join(", ", type.GetGenericArguments().Select(GetGlobalQualifiedName));
			return $"global::{genericTypeDefinition[..genericTypeDefinition.IndexOf('`')]}<{genericArguments}>";
		}
		else if (type.IsArray)
		{
			// Handle arrays
			return $"{type.GetElementType()!.GetGlobalQualifiedName()}[{new string(',', type.GetArrayRank() - 1)}]";
		}
		else
		{
			return $"global::{type.FullName}";
		}
	}
}
