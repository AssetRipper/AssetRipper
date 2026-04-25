using AssetRipper.SerializationLogic.Extensions;
using System.Diagnostics;
using static AssetRipper.SerializationLogic.SerializableType;

namespace AssetRipper.SerializationLogic;

public readonly partial struct FieldSerializer
{
	private static SerializableType ManagedReferencesRegistryFieldType { get; } = new SyntheticSerializableType(null, PrimitiveType.Complex, "ManagedReferencesRegistry", []);

	public bool TryCreateSerializableType(TypeDefinition typeDefinition,
		[NotNullWhen(true)] out SerializableType? result,
		[NotNullWhen(false)] out string? failureReason)
	{
		return TryCreateSerializableType(typeDefinition, new(SignatureComparer.Default), out result, out failureReason);
	}

	public bool TryCreateSerializableTypeForMonoBehaviour(
		TypeDefinition typeDefinition,
		Dictionary<ITypeDefOrRef, SerializableType> typeCache,
		[NotNullWhen(true)] out SerializableType? result,
		[NotNullWhen(false)] out string? failureReason)
	{
		Stack<MonoType> typeStack = new();
		bool returnValue = TryCreateSerializableType(typeDefinition, typeCache, typeStack, out SerializableType? baseType, out failureReason, out bool usesManagedReference);
		Debug.Assert(typeStack.Count == 0, "The type stack should be empty after processing.");
		if (!returnValue || baseType is null)
		{
			failureReason ??= "Failed to create serializable type.";
			result = null;
			return false;
		}

		result = usesManagedReference
			? AppendRegistryField(baseType)
			: baseType;
		typeCache[typeDefinition] = result;
		return true;
	}

	public bool TryCreateSerializableType(
		TypeDefinition typeDefinition,
		Dictionary<ITypeDefOrRef, SerializableType> typeCache,
		[NotNullWhen(true)] out SerializableType? result,
		[NotNullWhen(false)] out string? failureReason)
	{
		Stack<MonoType> typeStack = new();
		bool returnValue = TryCreateSerializableType(typeDefinition, typeCache, typeStack, out result, out failureReason, out _);
		Debug.Assert(typeStack.Count == 0, "The type stack should be empty after processing.");
		return returnValue;
	}

	private bool TryCreateSerializableType(
		TypeSignature typeSignature,
		Dictionary<ITypeDefOrRef, SerializableType> typeCache,
		Stack<MonoType> typeStack,
		[NotNullWhen(true)] out SerializableType? result,
		[NotNullWhen(false)] out string? failureReason,
		out bool usesManagedReference)
	{
		if (typeSignature is GenericInstanceTypeSignature genericInstanceType)
		{
			return TryCreateSerializableType(genericInstanceType, typeCache, typeStack, out result, out failureReason, out usesManagedReference);
		}
		TypeDefinition? typeDefinition = typeSignature.Resolve();
		if (typeDefinition is null)
		{
			result = null;
			failureReason = $"Failed to resolve type signature {typeSignature.FullName}.";
			usesManagedReference = false;
			return false;
		}
		else
		{
			return TryCreateSerializableType(typeDefinition, typeCache, typeStack, out result, out failureReason, out usesManagedReference);
		}
	}

	private bool TryCreateSerializableType(
		TypeDefinition typeDefinition,
		Dictionary<ITypeDefOrRef, SerializableType> typeCache,
		Stack<MonoType> typeStack,
		[NotNullWhen(true)] out SerializableType? result,
		[NotNullWhen(false)] out string? failureReason,
		out bool usesManagedReference)
	{
		if (typeCache.TryGetValue(typeDefinition, out SerializableType? cachedType))
		{
			result = cachedType;
			failureReason = null;
			usesManagedReference = false;
			return true;
		}
		if (typeDefinition.GenericParameters.Count > 0)
		{
			result = null;
			failureReason = "Generic types are not serializable.";
			usesManagedReference = false;
			return false;
		}
		if (typeDefinition.TryGetPrimitiveType(out PrimitiveType primitiveType))
		{
			result = SerializablePrimitiveType.GetOrCreate(primitiveType);
			typeCache.Add(typeDefinition, result);
			failureReason = null;
			usesManagedReference = false;
			return true;
		}

		//Ensure we allocate some initial space so that we have less chance of needing to resize the list.
		List<Field> fields = [];

		//Caching before completion prevents infinite loops.
		MonoType monoType = new(typeDefinition, fields);
		typeCache.Add(typeDefinition, monoType);
		typeStack.Push(monoType);

		if (typeDefinition.BaseType is not null)
		{
			if (!TryCreateSerializableType(typeDefinition.BaseType.ToTypeSignature(), typeCache, typeStack, out SerializableType? baseType, out failureReason, out bool baseUsesManagedReference))
			{
				typeCache.Remove(typeDefinition);
				typeStack.Pop();
				result = null;
				usesManagedReference = false;
				return false;
			}
			else
			{
				fields.EnsureCapacity(baseType.Fields.Count + typeDefinition.Fields.Count);
				fields.AddRange(baseType.Fields);
				usesManagedReference = baseUsesManagedReference;
			}
		}
		else
		{
			fields.EnsureCapacity(typeDefinition.Fields.Count);
			usesManagedReference = false;
		}

		if (TryCreateSerializableFields(typeStack, monoType, fields, GetFieldsInType(typeDefinition), typeCache, out failureReason, out bool localUsesManagedReference))
		{
			monoType.SetDepth();
			typeStack.Pop();
			result = monoType;
			usesManagedReference |= localUsesManagedReference;
			return true;
		}
		else
		{
			typeCache.Remove(typeDefinition);
			typeStack.Pop();
			result = null;
			usesManagedReference = false;
			return false;
		}
	}

	private bool TryCreateSerializableType(
		GenericInstanceTypeSignature genericInst,
		Dictionary<ITypeDefOrRef, SerializableType> typeCache,
		Stack<MonoType> typeStack,
		[NotNullWhen(true)] out SerializableType? result,
		[NotNullWhen(false)] out string? failureReason,
		out bool usesManagedReference)
	{
		ITypeDefOrRef typeCacheKey = genericInst.ToTypeDefOrRef();
		if (typeCache.TryGetValue(typeCacheKey, out SerializableType? cachedType))
		{
			result = cachedType;
			failureReason = null;
			usesManagedReference = false;
			return true;
		}

		List<Field> fields = [];

		MonoType monoType = new(genericInst.GenericType, fields);
		typeCache.Add(typeCacheKey, monoType);
		typeStack.Push(monoType);

		if (!TryGetBaseType(genericInst, out TypeSignature? baseType))
		{
			typeCache.Remove(typeCacheKey);
			typeStack.Pop();
			result = null;
			failureReason = $"Failed to resolve base type of {genericInst.FullName}.";
			usesManagedReference = false;
			return false;
		}
		else if (baseType is not null)
		{
			if (!TryCreateSerializableType(baseType, typeCache, typeStack, out SerializableType? baseMonoType, out failureReason, out bool baseUsesManagedReference))
			{
				typeCache.Remove(typeCacheKey);
				typeStack.Pop();
				result = null;
				usesManagedReference = false;
				return false;
			}
			else
			{
				fields.EnsureCapacity(baseMonoType.Fields.Count + genericInst.GenericType.Resolve()!.Fields.Count);
				fields.AddRange(baseMonoType.Fields);
				usesManagedReference = baseUsesManagedReference;
			}
		}
		else
		{
			fields.EnsureCapacity(genericInst.GenericType.Resolve()!.Fields.Count);
			usesManagedReference = false;
		}

		if (TryCreateSerializableFields(typeStack, monoType, fields, GetFieldsInType(genericInst), typeCache, out failureReason, out bool localUsesManagedReference))
		{
			monoType.SetDepth();
			typeStack.Pop();
			result = monoType;
			usesManagedReference |= localUsesManagedReference;
			return true;
		}
		else
		{
			typeCache.Remove(typeCacheKey);
			typeStack.Pop();
			result = null;
			usesManagedReference = false;
			return false;
		}
	}

	private bool TryCreateSerializableFields(
		Stack<MonoType> typeStack,
		MonoType monoType,
		List<Field> fields,
		IEnumerable<(FieldDefinition, TypeSignature)> enumerable,
		Dictionary<ITypeDefOrRef, SerializableType> typeCache,
		[NotNullWhen(false)] out string? failureReason,
		out bool usesManagedReference)
	{
		usesManagedReference = false;
		foreach ((FieldDefinition, TypeSignature) pair in enumerable)
		{
			(FieldDefinition fieldDefinition, TypeSignature fieldType) = pair;
			if (WillUnitySerialize(fieldDefinition, fieldType))
			{
				int arrayDepth = 0;
				if (fieldDefinition.HasFixedBufferAttribute())
				{
					fieldType = fieldDefinition.GetFixedBufferElementType();
					arrayDepth = 1;
				}

				if (fieldType is CustomModifierTypeSignature customModifierType)
				{
					fieldType = customModifierType.BaseType;
				}

				bool useManagedReferenceLayout = fieldDefinition.HasSerializeReferenceAttribute() && ShouldUseManagedReferenceLayout(fieldType);
				if (useManagedReferenceLayout)
				{
					usesManagedReference = true;
					fields.Add(new Field(CreateManagedReferenceFieldType(), GetManagedReferenceArrayDepth(fieldType, arrayDepth), fieldDefinition.Name ?? "", true));
				}
				else if (TryCreateSerializableField(typeStack, fieldDefinition.Name ?? "", fieldType, arrayDepth, typeCache, out Field field, out failureReason, out bool fieldUsesManagedReference))
				{
					usesManagedReference |= fieldUsesManagedReference;
					if (monoType.IsCyclicReference(field.Type))
					{
						// Infinite recursion disqualifies a field from serialization.
					}
					else if (!field.Type.IsMaxDepthKnown)
					{
						// New cycle reference detected.
						List<MonoType> cycleList = new(typeStack.Count);
						foreach (MonoType monoTypeInStack in typeStack)
						{
							cycleList.Add(monoTypeInStack);
							if (monoTypeInStack == field.Type)
							{
								break;
							}
						}

						for (int i = 0; i < cycleList.Count; i++)
						{
							for (int j = 0; j <= i; j++)
							{
								SerializableType type1 = cycleList[i];
								SerializableType type2 = cycleList[j];
								type1.AddCyclicReference(type2);
								type2.AddCyclicReference(type1);
							}
						}
					}
					else
					{
						fields.Add(field);
					}
				}
				else
				{
					usesManagedReference = false;
					return false;
				}
			}
		}
		failureReason = null;
		return true;
	}

	private bool TryCreateSerializableField(
		Stack<MonoType> typeStack,
		string name,
		TypeSignature typeSignature,
		int arrayDepth,
		Dictionary<ITypeDefOrRef, SerializableType> typeCache,
		out Field result,
		[NotNullWhen(false)] out string? failureReason,
		out bool usesManagedReference)
	{
		usesManagedReference = false;
		switch (typeSignature)
		{
			case TypeDefOrRefSignature typeDefOrRefSignature:
				TypeDefinition typeDefinition = typeDefOrRefSignature.Type.CheckedResolve();
				SerializableType fieldType;
				if (typeDefinition.IsEnum)
				{
					CorLibTypeSignature enumValueType = (CorLibTypeSignature?)typeDefinition.GetEnumUnderlyingType() ?? throw new("Failed to resolve enum underlying type.");
					PrimitiveType primitiveType = enumValueType.ToPrimitiveType();
					fieldType = SerializablePrimitiveType.GetOrCreate(primitiveType);
				}
				else if (typeDefinition.InheritsFromObject())
				{
					fieldType = SerializablePointerType.Shared;
				}
				else if (typeCache.TryGetValue(typeDefinition, out SerializableType? cachedMonoType))
				{
					//This needs to come after the InheritsFromObject check so that those fields get properly converted into PPtr assets.
					fieldType = cachedMonoType;
				}
				else if (TryCreateSerializableType(typeDefinition, typeCache, typeStack, out SerializableType? monoType, out failureReason, out bool nestedUsesManagedReference))
				{
					fieldType = monoType;
					usesManagedReference = nestedUsesManagedReference;
				}
				else
				{
					result = default;
					return false;
				}

				result = new Field(fieldType, arrayDepth, name, true);
				failureReason = null;
				return true;

			case CorLibTypeSignature corLibTypeSignature:
				result = new Field(SerializablePrimitiveType.GetOrCreate(corLibTypeSignature.ToPrimitiveType()), arrayDepth, name, true);
				failureReason = null;
				return true;

			case SzArrayTypeSignature szArrayTypeSignature:
				return TryCreateSerializableField(typeStack, name, szArrayTypeSignature.BaseType, arrayDepth + 1, typeCache, out result, out failureReason, out usesManagedReference);

			case GenericInstanceTypeSignature genericInstanceTypeSignature:
				if (genericInstanceTypeSignature.InheritsFromObject())
				{
					result = new Field(SerializablePointerType.Shared, arrayDepth, name, true);
					failureReason = null;
					return true;
				}
				else if (typeCache.TryGetValue(genericInstanceTypeSignature.ToTypeDefOrRef(), out SerializableType? cachedGenericMonoType))
				{
					result = new Field(cachedGenericMonoType, arrayDepth, name, true);
					failureReason = null;
					return true;
				}
				else if (genericInstanceTypeSignature.GenericType is { Namespace.Value: "System.Collections.Generic", Name.Value: "List`1" })
				{
					return TryCreateSerializableField(typeStack, name, genericInstanceTypeSignature.TypeArguments[0], arrayDepth + 1, typeCache, out result, out failureReason, out usesManagedReference);
				}
				else if (TryCreateSerializableType(genericInstanceTypeSignature, typeCache, typeStack, out SerializableType? monoType, out failureReason, out bool nestedUsesManagedReference))
				{
					usesManagedReference = nestedUsesManagedReference;
					result = new(monoType, arrayDepth, name, true);
					return true;
				}
				else
				{
					result = default;
					return false;
				}

			default:
				result = default;
				failureReason = $"{typeSignature.FullName} not supported.";
				return false;
		}
	}

	private SerializableType CreateManagedReferenceFieldType()
	{
		SerializableType idType = SerializablePrimitiveType.GetOrCreate(IsInt64Serializable ? PrimitiveType.Long : PrimitiveType.Int);
		return new SyntheticSerializableType(
			null,
			PrimitiveType.Complex,
			"managedReference",
			[new Field(idType, 0, IsInt64Serializable ? "rid" : "id", false)]);
	}

	private static SerializableType AppendRegistryField(SerializableType original)
	{
		List<Field> fields = new(original.Fields.Count + 1);
		fields.AddRange(original.Fields);
		fields.Add(new Field(ManagedReferencesRegistryFieldType, 0, "references", false));
		return new SyntheticSerializableType(original.Namespace, original.Type, original.Name, fields, original.Version, original.FlowMappedInYaml);
	}

	private static bool TryGetBaseType(GenericInstanceTypeSignature genericInstanceType, out TypeSignature? baseType)
	{
		TypeDefinition? typeDefinition = genericInstanceType.GenericType.Resolve();
		if (typeDefinition is null)
		{
			baseType = null;
			return false;
		}

		baseType = typeDefinition.BaseType?.ToTypeSignature().InstantiateGenericTypes(new GenericContext(genericInstanceType, null));
		return true;
	}

	private static bool ShouldUseManagedReferenceLayout(TypeSignature fieldType)
	{
		if (fieldType is CustomModifierTypeSignature customModifierType)
		{
			fieldType = customModifierType.BaseType;
		}

		switch (fieldType)
		{
			case SzArrayTypeSignature arrayType:
				return ShouldUseManagedReferenceLayout(arrayType.BaseType);

			case GenericInstanceTypeSignature genericInstanceType:
				if (genericInstanceType.GenericType is { Namespace.Value: "System.Collections.Generic", Name.Value: "List`1" })
				{
					return ShouldUseManagedReferenceLayout(genericInstanceType.TypeArguments[0]);
				}

				if (genericInstanceType.InheritsFromObject())
				{
					return false;
				}

				TypeDefinition? genericTypeDefinition = genericInstanceType.Resolve();
				return genericTypeDefinition is null || IsManagedReferenceCandidate(genericTypeDefinition);

			case TypeDefOrRefSignature typeDefOrRefSignature:
				TypeDefinition? typeDefinition = typeDefOrRefSignature.Resolve();
				return typeDefinition is null || IsManagedReferenceCandidate(typeDefinition);

			case CorLibTypeSignature:
				return false;

			default:
				return false;
		}
	}

	private static int GetManagedReferenceArrayDepth(TypeSignature fieldType, int arrayDepth)
	{
		if (fieldType is CustomModifierTypeSignature customModifierType)
		{
			fieldType = customModifierType.BaseType;
		}

		while (true)
		{
			switch (fieldType)
			{
				case SzArrayTypeSignature arrayType:
					arrayDepth++;
					fieldType = arrayType.BaseType;
					continue;

				case GenericInstanceTypeSignature genericInstanceType when genericInstanceType.GenericType is { Namespace.Value: "System.Collections.Generic", Name.Value: "List`1" }:
					arrayDepth++;
					fieldType = genericInstanceType.TypeArguments[0];
					continue;

				default:
					return arrayDepth;
			}
		}
	}

	private static bool IsManagedReferenceCandidate(TypeDefinition typeDefinition)
	{
		if (typeDefinition.InheritsFromObject())
		{
			return false;
		}

		if (typeDefinition.IsEnum || typeDefinition.IsValueType)
		{
			return false;
		}

		if (typeDefinition.Namespace == "System" && typeDefinition.Name == "String")
		{
			return false;
		}

		return true;
	}

	private static IEnumerable<(FieldDefinition, TypeSignature)> GetFieldsInType(TypeDefinition typeDefinition)
	{
		return typeDefinition.Fields.Select(field =>
		{
			TypeSignature fieldType = field.Signature!.FieldType;
			return (field, fieldType);
		});
	}

	private static IEnumerable<(FieldDefinition, TypeSignature)> GetFieldsInType(GenericInstanceTypeSignature genericInst)
	{
		TypeDefinition? typeDefinition = genericInst.Resolve();
		if (typeDefinition is null)
		{
			return [];
		}
		return typeDefinition.Fields.Select(field =>
		{
			TypeSignature fieldType = field.Signature!.FieldType;
			GenericContext genericContext = new GenericContext(genericInst, null);
			TypeSignature instanceTypeSignature = fieldType.InstantiateGenericTypes(genericContext);
			return (field, instanceTypeSignature);
		});
	}
}
