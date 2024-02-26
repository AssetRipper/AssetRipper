using AsmResolver.DotNet;

internal static class AccessModifier
{
	public static string GetAccessModifier(this TypeDefinition type)
	{
		if (type.IsNested)
		{
			if (type.IsNestedPublic)
			{
				return "public";
			}
			else if (type.IsNestedAssembly)
			{
				return "internal";
			}
			else if (type.IsNestedFamily)
			{
				return "protected";
			}
			else if (type.IsNestedPrivate)
			{
				return "private";
			}
			else if (type.IsNestedFamilyOrAssembly)
			{
				return "protected internal";
			}
			else if (type.IsNestedFamilyAndAssembly)
			{
				return "private protected";
			}
		}
		else
		{
			if (type.IsPublic)
			{
				return "public";
			}
			else if (type.IsNotPublic)
			{
				return "internal";
			}
		}
		throw new NotSupportedException($"Unsupported access modifier for type {type.FullName}");
	}

	public static string GetAccessModifier(this FieldDefinition field)
	{
		if (field.IsPublic)
		{
			return "public";
		}
		else if (field.IsAssembly)
		{
			return "internal";
		}
		else if (field.IsFamily)
		{
			return "protected";
		}
		else if (field.IsPrivate)
		{
			return "private";
		}
		else if (field.IsFamilyOrAssembly)
		{
			return "internal protected";
		}
		else if (field.IsFamilyAndAssembly)
		{
			return "private protected";
		}
		throw new NotSupportedException($"Unsupported access modifier for type {field.FullName}");
	}

	public static string GetAccessModifier(this MethodDefinition method)
	{
		if (method.IsPublic)
		{
			return "public";
		}
		else if (method.IsAssembly)
		{
			return "internal";
		}
		else if (method.IsFamily)
		{
			return "protected";
		}
		else if (method.IsPrivate)
		{
			return "private";
		}
		else if (method.IsFamilyOrAssembly)
		{
			return "internal protected";
		}
		else if (method.IsFamilyAndAssembly)
		{
			return "private protected";
		}
		throw new NotSupportedException($"Unsupported access modifier for type {method.FullName}");
	}

	public static string GetAccessModifier(this PropertyDefinition property)
	{
		MethodDefinition? getMethod = property.GetMethod;
		MethodDefinition? setMethod = property.SetMethod;
		if (getMethod is null)
		{
			if (setMethod is null)
			{
				return "/* Property has no accessors */";
			}
			return setMethod.GetAccessModifier();
		}
		else if (setMethod is null)
		{
			return getMethod.GetAccessModifier();
		}
		else
		{
			if (getMethod.IsPublic || setMethod.IsPublic)
			{
				return "public";
			}
			else if (getMethod.IsFamilyOrAssembly || setMethod.IsFamilyOrAssembly)
			{
				return "internal protected";
			}
			else if (getMethod.IsAssembly)
			{
				if (setMethod.IsFamily)
				{
					return "/* Property has incompatible accessors: 'internal' and 'protected' */";
				}
				return "internal";
			}
			else if (setMethod.IsAssembly)
			{
				if (getMethod.IsFamily)
				{
					return "/* Property has incompatible accessors: 'protected' and 'internal' */";
				}
				return "internal";
			}
			else if (getMethod.IsFamily || setMethod.IsFamily)
			{
				return "protected";
			}
			else if (getMethod.IsFamilyAndAssembly || setMethod.IsFamilyAndAssembly)
			{
				return "private protected";
			}
			else if (getMethod.IsPrivate || setMethod.IsPrivate)
			{
				return "private";
			}
			throw new NotSupportedException($"Unsupported access modifier for type {property.FullName}");
		}
	}
}
