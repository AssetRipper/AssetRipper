using AsmResolver.PE.DotNet.Metadata.Tables;
using AssetRipper.Primitives;
using AssetRipper.SerializationLogic.Extensions;

namespace AssetRipper.SerializationLogic;

public readonly partial struct FieldSerializer(UnityVersion version)
{
	private static readonly SignatureComparer signatureComparer = new(SignatureComparisonFlags.VersionAgnostic);

	/// <summary>
	/// Not sure about the exact version boundary, structs are supposedly only serializable on 4.5.0 and greater.
	/// </summary>
	private bool IsStructSerializable { get; } = version.GreaterThanOrEquals(4, 5);
	private bool IsInt8Serializable => IsInt16Serializable;
	/// <summary>
	/// Not sure about the exact version boundary, but int8, int16, uint16, and uint32 were added around 5.0.0.
	/// </summary>
	/// <remarks>
	/// <see href="https://github.com/AssetRipper/AssetRipper/issues/1851"/>
	/// </remarks>
	private bool IsInt16Serializable { get; } = version.GreaterThanOrEquals(5);
	private bool IsUInt32Serializable => IsInt16Serializable;
	private bool IsCharSerializable => IsInt64Serializable;
	/// <summary>
	/// Not sure about the exact version boundary, but online references suggest that 2017 was the first version to support this.
	/// </summary>
	/// <remarks>
	/// <see href="https://github.com/AssetRipper/AssetRipper/issues/647"/>
	/// </remarks>
	private bool IsInt64Serializable { get; } = version.GreaterThanOrEquals(2017);
	/// <summary>
	/// Prior to some unknown version, System.Collections.Generic.List`1 and UnityEngine.ExposedReference`1 were the only supported generic types.
	/// </summary>
	private bool IsGenericInstanceSerializable => true;

	public bool WillUnitySerialize(FieldDefinition fieldDefinition)
	{
		return WillUnitySerialize(fieldDefinition, fieldDefinition.Signature!.FieldType);
	}

	public bool WillUnitySerialize(FieldDefinition fieldDefinition, TypeSignature fieldType)
	{
		if (fieldDefinition == null)
		{
			return false;
		}

		//skip static, const and NotSerialized fields before even checking the type
		if (fieldDefinition.IsStatic || fieldDefinition.IsConst() || fieldDefinition.IsNotSerialized || fieldDefinition.IsInitOnly)
		{
			return false;
		}

		// The field must have correct visibility/decoration to be serialized.
		if (!fieldDefinition.IsPublic &&
			!ShouldHaveHadAllFieldsPublic(fieldDefinition) &&
			!fieldDefinition.HasSerializeFieldAttribute() &&
			!fieldDefinition.HasSerializeReferenceAttribute())
		{
			return false;
		}

		// Don't try to resolve types that come from Windows assembly,
		// as serialization weaver will fail to resolve that (due to it being in platform specific SDKs)
		if (ShouldNotTryToResolve(fieldDefinition.Signature!.FieldType))
		{
			return false;
		}

		if (fieldDefinition.HasFixedBufferAttribute())
		{
			return true;
		}

		// Resolving types is more complex and slower than checking their names or attributes,
		// thus keep those checks below

		//the type of the field must be serializable in the first place.

		if (fieldType is CustomModifierTypeSignature customModifierType)
		{
			fieldType = customModifierType.BaseType;
		}

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
			if (!fieldDefinition.HasSerializeReferenceAttribute())
			{
				return IsSupportedCollection(fieldType);
			}
		}


		if (!IsReferenceTypeSerializable(fieldType) && !fieldDefinition.HasSerializeReferenceAttribute())
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

	public bool ShouldFieldBePPtrRemapped(FieldDefinition fieldDefinition)
	{
		if (!WillUnitySerialize(fieldDefinition))
		{
			return false;
		}

		return CanTypeContainUnityEngineObjectReference(fieldDefinition.Signature!.FieldType);
	}

	private bool CanTypeContainUnityEngineObjectReference(ITypeDescriptor typeReference)
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

	private bool HasFieldsThatCanContainUnityEngineObjectReferences(TypeDefinition definition)
	{
		foreach (FieldDefinition field in AllFieldsFor(definition))
		{
			if (signatureComparer.Equals(field.Signature?.FieldType.Resolve(), definition))
			{
				continue;
			}
			if (CanFieldContainUnityEngineObjectReference(definition, field))
			{
				return true;
			}
		}
		return false;
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

	private bool CanFieldContainUnityEngineObjectReference(ITypeDescriptor typeReference, FieldDefinition t)
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

	private bool IsValueTypeSerializable(TypeSignature typeReference)
	{
		if (typeReference.IsPrimitive())
		{
			return IsSerializablePrimitive((CorLibTypeSignature)typeReference);
		}

		if (typeReference.IsEnum())
		{
			// Enums are serializable as long as their underlying type is serializable
			TypeDefinition typeDefinition = typeReference.CheckedResolve();
			CorLibTypeSignature underlyingType = (CorLibTypeSignature)typeDefinition.GetEnumUnderlyingType()!;
			return IsSerializablePrimitive(underlyingType);
		}
		else
		{
			return EngineTypePredicates.IsSerializableUnityStruct(typeReference) || ShouldImplementIDeserializable(typeReference);
		}
	}

	private bool IsReferenceTypeSerializable(ITypeDescriptor typeReference)
	{
		if (typeReference.ToTypeSignature() is CorLibTypeSignature { ElementType: ElementType.String } corLibTypeSignature)
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

	public bool IsTypeSerializable(ITypeDescriptor typeReference)
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

	public static int PrimitiveTypeSize(CorLibTypeSignature type)
	{
		return type.ElementType switch
		{
			ElementType.Boolean or ElementType.U1 or ElementType.I1 => 1,
			ElementType.Char or ElementType.I2 or ElementType.U2 => 2,
			ElementType.I4 or ElementType.U4 or ElementType.R4 => 4,
			ElementType.I8 or ElementType.U8 or ElementType.R8 => 8,
			_ => throw new ArgumentException($"Unsupported {type.ElementType}"),
		};
	}

	private bool IsSerializablePrimitive(CorLibTypeSignature typeReference)
	{
		return typeReference.ElementType switch
		{
			ElementType.I1 => IsInt8Serializable,
			ElementType.I2 or ElementType.U2 => IsInt16Serializable,
			ElementType.U4 => IsUInt32Serializable,
			ElementType.I8 or ElementType.U8 => IsInt64Serializable,
			ElementType.Char => IsCharSerializable,
			ElementType.Boolean or ElementType.U1 or ElementType.I4 or ElementType.R4 or ElementType.R8 or ElementType.String => true,
			_ => false,
		};
	}

	public bool IsSupportedCollection(TypeSignature typeReference)
	{
		if (typeReference is SzArrayTypeSignature || AsmUtils.IsGenericList(typeReference))
		{
			return IsTypeSerializable(AsmUtils.ElementTypeOfCollection(typeReference));
		}

		return false;
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

		//Check namespace
		return typeDeclaration.Namespace == "System" || (typeDeclaration.Namespace?.StartsWith("System.", StringComparison.Ordinal) ?? false);
	}

	public bool ShouldImplementIDeserializable([NotNullWhen(true)] ITypeDescriptor? typeDeclaration)
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
		isSerializable &= !resolvedTypeDeclaration.IsCompilerGenerated();

		if (typeDeclaration.IsValueType)
		{
			return isSerializable && IsStructSerializable;
		}

		//Reference types can be serializable, or they can be MB/SO.
		return isSerializable || resolvedTypeDeclaration.InheritsFromMonoBehaviour() || resolvedTypeDeclaration.InheritsFromScriptableObject();
	}
}
