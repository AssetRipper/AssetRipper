namespace AssetRipper.AssemblyDumper.Types;

public static class TypeDefinitionExtensions
{
	public static MethodDefinition GetMethodByName(this TypeDefinition type, string name, bool checkBaseTypes = false)
	{
		return type.TryGetMethodByName(name, checkBaseTypes)
			?? throw new Exception($"{type.FullName} doesn't have a {name} method.");
	}

	public static bool TryGetMethodByName(this TypeDefinition type, string name, [NotNullWhen(true)] out MethodDefinition? method, bool checkBaseTypes = false)
	{
		method = type.TryGetMethodByName(name, checkBaseTypes);
		return method != null;
	}

	public static MethodDefinition? TryGetMethodByName(this TypeDefinition type, string name, bool checkBaseTypes = false)
	{
		var result = type.Methods.SingleOrDefault(field => field.Name == name);
		if (result == null && checkBaseTypes)
		{
			result = type.TryGetBaseTypeDefinition()?.TryGetMethodByName(name, checkBaseTypes);
		}
		return result;
	}

	public static List<FieldDefinition> GetAllFieldsInTypeAndBase(this TypeDefinition? type)
	{
		if (type == null)
		{
			return new();
		}

		List<FieldDefinition>? ret = type.Fields.ToList();

		ret.AddRange((type.BaseType?.Resolve()).GetAllFieldsInTypeAndBase());

		return ret;
	}

	public static FieldDefinition GetFieldByName(this TypeDefinition type, string name, bool checkBaseTypes = false)
	{
		return type.TryGetFieldByName(name, checkBaseTypes)
			?? throw new Exception($"{type.FullName} doesn't have a {name} field.");
	}

	public static bool TryGetFieldByName(this TypeDefinition type, string name, [NotNullWhen(true)] out FieldDefinition? field, bool checkBaseTypes = false)
	{
		field = type.TryGetFieldByName(name, checkBaseTypes);
		return field != null;
	}

	public static FieldDefinition? TryGetFieldByName(this TypeDefinition type, string name, bool checkBaseTypes = false)
	{
		var result = type.Fields.SingleOrDefault(field => field.Name == name);
		if (result == null && checkBaseTypes)
		{
			result = type.TryGetBaseTypeDefinition()?.TryGetFieldByName(name, checkBaseTypes);
		}
		return result;
	}

	private static TypeDefinition? TryGetBaseTypeDefinition(this TypeDefinition type)
	{
		return type.BaseType as TypeDefinition;
	}
}
