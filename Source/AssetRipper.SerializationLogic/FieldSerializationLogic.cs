using AsmResolver.PE.DotNet.Metadata.Tables;
using AssetRipper.SerializationLogic.Extensions;

namespace AssetRipper.SerializationLogic;

public static class FieldSerializationLogic
{
	private static readonly SignatureComparer signatureComparer = new();

	public static bool WillUnitySerialize(FieldDefinition fieldDefinition)
	{
		return WillUnitySerialize(fieldDefinition, fieldDefinition.Signature!.FieldType);
	}

	public static bool WillUnitySerialize(FieldDefinition fieldDefinition, TypeSignature fieldType)
	{
		if (fieldDefinition == null)
		{
			return false;
		}

		//skip static, const and NotSerialized fields before even checking the type
		if (fieldDefinition.IsStatic || IsConst(fieldDefinition) || fieldDefinition.IsNotSerialized || fieldDefinition.IsInitOnly)
		{
			return false;
		}

		// The field must have correct visibility/decoration to be serialized.
		if (!fieldDefinition.IsPublic &&
			!ShouldHaveHadAllFieldsPublic(fieldDefinition) &&
			!HasSerializeFieldAttribute(fieldDefinition) &&
			!HasSerializeReferenceAttribute(fieldDefinition))
		{
			return false;
		}

		// Don't try to resolve types that come from Windows assembly,
		// as serialization weaver will fail to resolve that (due to it being in platform specific SDKs)
		if (ShouldNotTryToResolve(fieldDefinition.Signature!.FieldType))
		{
			return false;
		}

		if (IsFixedBuffer(fieldDefinition))
		{
			return true;
		}

		// Resolving types is more complex and slower than checking their names or attributes,
		// thus keep those checks below

		//the type of the field must be serializable in the first place.

		if (fieldType is CorLibTypeSignature corLibTypeSignature && corLibTypeSignature.ElementType == ElementType.String)
		{
			return true;
		}

		if (fieldType.IsValueType)
		{
			return IsValueTypeSerializable(fieldType);
		}

		if (fieldType is SzArrayTypeSignature || AsmUtils.IsGenericList(fieldType))
		{
			if (!HasSerializeReferenceAttribute(fieldDefinition))
			{
				return IsSupportedCollection(fieldType);
			}
		}


		if (!IsReferenceTypeSerializable(fieldType) && !HasSerializeReferenceAttribute(fieldDefinition))
		{
			return false;
		}

		if (IsDelegate(fieldType))
		{
			return false;
		}

		return true;
	}

	private static bool IsDelegate(ITypeDescriptor typeReference)
	{
		return typeReference.IsAssignableTo("System", "Delegate");
	}

	public static bool ShouldFieldBePPtrRemapped(FieldDefinition fieldDefinition)
	{
		if (!WillUnitySerialize(fieldDefinition))
		{
			return false;
		}

		return CanTypeContainUnityEngineObjectReference(fieldDefinition.Signature!.FieldType);
	}

	private static bool CanTypeContainUnityEngineObjectReference(ITypeDescriptor typeReference)
	{
		if (IsUnityEngineObject(typeReference))
		{
			return true;
		}

		if (typeReference.IsEnum())
		{
			return false;
		}

		if (typeReference.ToTypeSignature() is CorLibTypeSignature corLibTypeSignature && IsSerializablePrimitive(corLibTypeSignature))
		{
			return false;
		}

		if (IsSupportedCollection(typeReference.ToTypeSignature()))
		{
			return CanTypeContainUnityEngineObjectReference(AsmUtils.ElementTypeOfCollection(typeReference.ToTypeSignature()));
		}

		TypeDefinition? definition = typeReference.Resolve();
		return definition switch
		{
			null => false,
			_ => HasFieldsThatCanContainUnityEngineObjectReferences(definition)
		};
	}

	private static bool HasFieldsThatCanContainUnityEngineObjectReferences(TypeDefinition definition)
	{
		return AllFieldsFor(definition).Where(kv => !signatureComparer.Equals(kv.Signature?.FieldType.Resolve(), definition)).Any(kv => CanFieldContainUnityEngineObjectReference(definition, kv));
	}

	private static IEnumerable<FieldDefinition> AllFieldsFor(TypeDefinition definition)
	{
		TypeDefinition? baseType = definition.BaseType?.Resolve();

		if (baseType != null)
		{
			foreach (FieldDefinition baseField in AllFieldsFor(baseType))
			{
				yield return baseField;
			}
		}

		foreach (FieldDefinition field in definition.Fields)
		{
			yield return field;
		}
	}

	private static bool CanFieldContainUnityEngineObjectReference(ITypeDescriptor typeReference, FieldDefinition t)
	{
		if (signatureComparer.Equals(t.Signature!.FieldType, typeReference.ToTypeSignature()))
		{
			return false;
		}

		if (!WillUnitySerialize(t))
		{
			return false;
		}

		if (EngineTypePredicates.IsUnityEngineValueType(typeReference))
		{
			return false;
		}

		return true;
	}

	private static bool IsConst(FieldDefinition fieldDefinition)
	{
		return fieldDefinition.IsLiteral && !fieldDefinition.IsInitOnly;
	}

	public static bool HasSerializeFieldAttribute(FieldDefinition field)
	{
		foreach (CustomAttribute ca in field.CustomAttributes)
		{
			ITypeDefOrRef type = ca.Constructor!.DeclaringType!;
			if (EngineTypePredicates.IsSerializeFieldAttribute(type))
			{
				return true;
			}
		}

		return false;
	}

	public static bool HasSerializeReferenceAttribute(FieldDefinition field)
	{
		foreach (CustomAttribute ca in field.CustomAttributes)
		{
			ITypeDefOrRef type = ca.Constructor!.DeclaringType!;
			if (EngineTypePredicates.IsSerializeReferenceAttribute(type))
			{
				return true;
			}
		}

		return false;
	}

	public static bool ShouldNotTryToResolve(ITypeDescriptor typeReference)
	{
		if (typeReference is TypeDefinition)
		{
			//Early-out if we're already resolved.
			return false;
		}

		string? typeReferenceScopeName = typeReference.Scope?.Name;
		if (typeReferenceScopeName == "Windows")
		{
			return true;
		}

		if (typeReferenceScopeName == "mscorlib")
		{
			TypeDefinition? resolved = typeReference.Resolve();
			return resolved == null;
		}

		try
		{   // This will throw an exception if typereference thinks it's referencing a .dll,
			// but actually there's .winmd file in the current directory. RRW will fix this
			// at a later step, so we will not try to resolve this type. This is OK, as any
			// type defined in a winmd cannot be serialized.
			typeReference.Resolve();
		}
		catch
		{
			return true;
		}

		return false;
	}

	private static bool IsFieldTypeSerializable(ITypeDescriptor typeReference, FieldDefinition fieldDefinition)
	{
		return IsTypeSerializable(typeReference) || IsSupportedCollection(typeReference.ToTypeSignature()) || IsFixedBuffer(fieldDefinition);
	}

	private static bool IsValueTypeSerializable(TypeSignature typeReference)
	{
		if (typeReference.IsPrimitive())
		{
			return IsSerializablePrimitive((CorLibTypeSignature)typeReference);
		}

		return typeReference.IsEnum() ||
			ShouldImplementIDeserializable(typeReference) ||
			EngineTypePredicates.IsSerializableUnityStruct(typeReference);
	}

	private static bool IsReferenceTypeSerializable(ITypeDescriptor typeReference)
	{
		if (typeReference.ToTypeSignature() is CorLibTypeSignature corLibTypeSignature && corLibTypeSignature.ElementType == ElementType.String)
		{
			return IsSerializablePrimitive(corLibTypeSignature);
		}

		if (IsGenericDictionary(typeReference))
		{
			return false;
		}

		if (IsUnityEngineObject(typeReference) ||
			EngineTypePredicates.IsSerializableUnityClass(typeReference) ||
			ShouldImplementIDeserializable(typeReference))
		{
			return true;
		}

		return false;
	}

	public static bool IsTypeSerializable(ITypeDescriptor typeReference)
	{
		if (typeReference.ToTypeSignature() is CorLibTypeSignature corLibTypeSignature && corLibTypeSignature.ElementType == ElementType.String)
		{
			return true;
		}

		if (typeReference.IsValueType)
		{
			return IsValueTypeSerializable(typeReference.ToTypeSignature());
		}

		return IsReferenceTypeSerializable(typeReference);
	}

	private static bool IsGenericDictionary(ITypeDescriptor typeReference) => AsmUtils.IsGenericDictionary(typeReference);

	public static bool IsFixedBuffer(FieldDefinition fieldDefinition)
	{
		return GetFixedBufferAttribute(fieldDefinition) != null;
	}

	public static CustomAttribute? GetFixedBufferAttribute(FieldDefinition fieldDefinition)
	{
		IList<CustomAttribute> attrs = fieldDefinition.CustomAttributes;
		foreach (CustomAttribute ca in attrs)
		{
			ITypeDefOrRef type = ca.Constructor!.DeclaringType!;
			if (type is { Namespace.Value: "System.Runtime.CompilerServices", Name.Value: "FixedBufferAttribute" })
			{
				return ca;
			}
		}

		return null;
	}

	public static int GetFixedBufferLength(FieldDefinition fieldDefinition)
	{
		CustomAttribute fixedBufferAttribute = GetFixedBufferAttribute(fieldDefinition)
			?? throw new ArgumentException($"Field '{fieldDefinition.FullName}' is not a fixed buffer field.");

		int size = (int)(fixedBufferAttribute.Signature?.FixedArguments[1].Element ?? 0);

		return size;
	}

	public static int PrimitiveTypeSize(CorLibTypeSignature type)
	{
		return type.ElementType switch
		{
			ElementType.Boolean or ElementType.U1 or ElementType.I1 => 1,
			ElementType.Char or ElementType.I2 or ElementType.U2 => 2,
			ElementType.I4 or ElementType.U4 or ElementType.R4 => 4,
			ElementType.I8 or ElementType.U8 or ElementType.R8 => 8,
			_ => throw new ArgumentException(string.Format("Unsupported {0}", type.ElementType)),
		};
	}

	private static bool IsSerializablePrimitive(CorLibTypeSignature typeReference)
	{
		switch (typeReference.ElementType)
		{
			case ElementType.Boolean:
			case ElementType.I1:
			case ElementType.U1:
			case ElementType.Char:
			case ElementType.I2:
			case ElementType.U2:
			case ElementType.I4:
			case ElementType.U4:
			case ElementType.I8:
			case ElementType.U8:
			case ElementType.R4:
			case ElementType.R8:
			case ElementType.String:
				return true;
			default:
				return false;
		}
	}

	public static bool IsSupportedCollection(TypeSignature typeReference)
	{
		// We don't support arrays like byte[,] etc
		//if (typeReference is ArrayTypeSignature arrayType && arrayType.Dimensions.Count != 1)
		//{
		//	return false;
		//}
		//Redundant

		if (typeReference is not SzArrayTypeSignature && !AsmUtils.IsGenericList(typeReference))
		{
			return false;
		}

		return IsTypeSerializable(AsmUtils.ElementTypeOfCollection(typeReference));
	}

	private static bool ShouldHaveHadAllFieldsPublic(FieldDefinition field)
	{
		return field.DeclaringType is not null && EngineTypePredicates.IsUnityEngineValueType(field.DeclaringType);
	}

	private static bool IsUnityEngineObject(ITypeDescriptor typeReference)
	{
		return EngineTypePredicates.IsUnityEngineObject(typeReference);
	}

	public static bool IsNonSerialized([NotNullWhen(false)] ITypeDescriptor? typeDeclaration)
	{
		if (typeDeclaration == null)
		{
			return true;
		}

		if (typeDeclaration.ToTypeSignature() is GenericInstanceTypeSignature genericInstanceTypeSignature
			&& genericInstanceTypeSignature.TypeArguments.Any(t => t is GenericParameterSignature))
		{
			return true;
		}

		if (typeDeclaration.ToTypeSignature() is CorLibTypeSignature { ElementType: ElementType.Object })
		{
			return true;
		}

		if (typeDeclaration.IsArray())
		{
			return true;
		}

		if (typeDeclaration.IsEnum())
		{
			return true;
		}

		//MB and SO are not serializable
		if (typeDeclaration is { Namespace: EngineTypePredicates.UnityEngineNamespace, Name: EngineTypePredicates.MonoBehaviour or EngineTypePredicates.ScriptableObject })
		{
			return true;
		}

		//Fullname is slow, do it last
		return typeDeclaration.FullName.StartsWith("System."); //can this be done better?
	}

	public static bool ShouldImplementIDeserializable([NotNullWhen(true)] ITypeDescriptor? typeDeclaration)
	{
		if (typeDeclaration is { Namespace: EngineTypePredicates.UnityEngineNamespace, Name: "ExposedReference`1" })
		{
			return true;
		}

		if (IsNonSerialized(typeDeclaration))
		{
			return false;
		}

		if (EngineTypePredicates.ShouldHaveHadSerializableAttribute(typeDeclaration))
		{
			return true;
		}

		TypeDefinition resolvedTypeDeclaration = typeDeclaration.CheckedResolve();

		bool isSerializable = resolvedTypeDeclaration.IsSerializable;

		//If serializable, also check we're not compiler generated
		isSerializable &= resolvedTypeDeclaration.CustomAttributes.All(a => a.Constructor?.DeclaringType is not { } type || type.Namespace != "System.Runtime.CompilerServices" || !type.Name!.Value.StartsWith("CompilerGenerated"));

		if (typeDeclaration.IsValueType)
		{
			return isSerializable;
		}

		//Reference types can be serializable, or they can be MB/SO.
		return isSerializable || resolvedTypeDeclaration.IsSubclassOfAny(EngineTypePredicates.MonoBehaviourFullName, EngineTypePredicates.ScriptableObjectFullName);
	}
}
