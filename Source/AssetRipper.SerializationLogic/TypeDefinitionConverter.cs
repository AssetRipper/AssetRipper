using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AssetRipper.SerializationLogic;

public sealed class TypeDefinitionConverter
{
	private readonly TypeDefinition TypeDef;

	public TypeDefinitionConverter(TypeDefinition typeDef)
	{
		TypeDef = typeDef;
	}

	private bool WillUnitySerialize(FieldDefinition fieldDefinition)
	{
		try
		{
			TypeSignature? resolvedFieldType = fieldDefinition.Signature?.FieldType;
			if (resolvedFieldType is null || FieldSerializationLogic.ShouldNotTryToResolve(resolvedFieldType))
			{
				return false;
			}
			if (!EngineTypePredicates.IsUnityEngineObject(resolvedFieldType))
			{
				if (resolvedFieldType.FullName == fieldDefinition.DeclaringType?.FullName)
				{
					return false;
				}
			}
			return FieldSerializationLogic.WillUnitySerialize(fieldDefinition);
		}
		catch (Exception ex)
		{
			throw new Exception(string.Format("Exception while processing {0} {1}, error {2}", fieldDefinition.Signature?.FieldType.FullName, fieldDefinition.FullName, ex.Message));
		}
	}

	private static bool IsHiddenByParentClass(IEnumerable<TypeReference> parentTypes, FieldDefinition fieldDefinition, TypeDefinition processingType)
	{
		return processingType.Fields.Any(f => f.Name == fieldDefinition.Name) || parentTypes.Any(t => t.Resolve()?.Fields.Any(f => f.Name == fieldDefinition.Name) ?? false);
	}

	private IEnumerable<FieldDefinition> FilteredFields()
	{
		return TypeDef.Fields.Where(WillUnitySerialize).Where(f =>
			FieldSerializationLogic.IsSupportedCollection(f.Signature!.FieldType) ||
			f.Signature.FieldType is not GenericInstanceTypeSignature ||
			FieldSerializationLogic.ShouldImplementIDeserializable(f.Signature.FieldType.Resolve()));
	}

	private static bool RequiresAlignment(ITypeDescriptor typeRef) => RequiresAlignment(typeRef.ToTypeSignature());

	private static bool RequiresAlignment(TypeSignature typeRef)
	{
		if (typeRef is CorLibTypeSignature corLibTypeSignature)
		{
			return RequiresAlignment(corLibTypeSignature.ElementType);
		}
		else
		{
			return FieldSerializationLogic.IsSupportedCollection(typeRef) && RequiresAlignment(AsmUtils.ElementTypeOfCollection(typeRef));
		}
	}

	private static bool RequiresAlignment(ElementType elementType)
	{
		return elementType is ElementType.Boolean or ElementType.Char or ElementType.I1 or ElementType.U1 or ElementType.I2 or ElementType.U2;
	}

	private static bool IsSystemString(ITypeDescriptor typeRef) => IsSystemString(typeRef.ToTypeSignature());

	private static bool IsSystemString(TypeSignature typeRef)
	{
		return typeRef is CorLibTypeSignature corLibTypeSignature && corLibTypeSignature.ElementType == ElementType.String;
	}
}
