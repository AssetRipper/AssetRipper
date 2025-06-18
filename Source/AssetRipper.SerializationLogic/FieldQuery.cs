namespace AssetRipper.SerializationLogic;

public static class FieldQuery
{
	public static IEnumerable<(FieldDefinition, TypeSignature)> GetFieldsInTypeAndBase(TypeDefinition typeDefinition)
	{
		Stack<(TypeDefinition, GenericContext)> hierarchy = new();

		IList<TypeSignature> currentTypeArguments = Array.Empty<TypeSignature>();
		TypeDefinition? currentType = typeDefinition;
		while (currentType is not null)
		{
			GenericContext genericContext = new GenericContext(new GenericArgumentsProvider(currentTypeArguments), null);
			hierarchy.Push((currentType, genericContext));
			currentTypeArguments = ApplyTypeArgumentsToBaseTypeArguments(currentTypeArguments, GetTypeArgumentsForBaseType(currentType));
			currentType = currentType.BaseType?.Resolve();
		}

		return IterateFields(hierarchy);
	}

	public static IEnumerable<(FieldDefinition, TypeSignature)> GetFieldsInTypeAndBase(GenericInstanceTypeSignature genericInst)
	{
		Stack<(TypeDefinition, GenericContext)> hierarchy = new();

		IList<TypeSignature> currentTypeArguments = genericInst.TypeArguments;
		TypeDefinition? currentType = genericInst.Resolve();
		while (currentType is not null)
		{
			GenericContext genericContext = new GenericContext(new GenericArgumentsProvider(currentTypeArguments), null);
			hierarchy.Push((currentType, genericContext));
			currentTypeArguments = ApplyTypeArgumentsToBaseTypeArguments(currentTypeArguments, GetTypeArgumentsForBaseType(currentType));
			currentType = currentType.BaseType?.Resolve();
		}

		return IterateFields(hierarchy);
	}

	private static IEnumerable<(FieldDefinition, TypeSignature)> IterateFields(Stack<(TypeDefinition, GenericContext)> hierarchy)
	{
		foreach ((TypeDefinition type, GenericContext genericContext) in hierarchy)
		{
			foreach (FieldDefinition field in type.Fields)
			{
				TypeSignature fieldType = field.Signature!.FieldType;
				TypeSignature instanceTypeSignature = fieldType.InstantiateGenericTypes(genericContext);
				yield return (field, instanceTypeSignature);
			}
		}
	}

	private static IList<TypeSignature> GetTypeArgumentsForBaseType(TypeDefinition typeDefinition)
	{
		TypeSpecification? baseType = typeDefinition.BaseType as TypeSpecification;
		if (baseType is not null && baseType.Signature is GenericInstanceTypeSignature genericInstanceTypeSignature)
		{
			return genericInstanceTypeSignature.TypeArguments;
		}
		else
		{
			return Array.Empty<TypeSignature>();
		}
	}

	private static IList<TypeSignature> ApplyTypeArgumentsToBaseTypeArguments(IList<TypeSignature> derivedTypeArguments, IList<TypeSignature> baseTypeArguments)
	{
		if (baseTypeArguments.Count == 0)
		{
			return Array.Empty<TypeSignature>();
		}
		else
		{
			GenericContext genericContext = new GenericContext(new GenericArgumentsProvider(derivedTypeArguments), null);
			TypeSignature[] newBaseTypeArguments = new TypeSignature[baseTypeArguments.Count];
			for (int i = 0; i < newBaseTypeArguments.Length; i++)
			{
				newBaseTypeArguments[i] = baseTypeArguments[i].InstantiateGenericTypes(genericContext);
			}
			return newBaseTypeArguments;
		}
	}

	private sealed record class GenericArgumentsProvider(IList<TypeSignature> TypeArguments) : IGenericArgumentsProvider
	{
	}
}
