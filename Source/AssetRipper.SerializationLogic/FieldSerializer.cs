using AssetRipper.SerializationLogic.Extensions;
using System.Diagnostics;
using static AssetRipper.SerializationLogic.SerializableType;

namespace AssetRipper.SerializationLogic;

public readonly partial struct FieldSerializer
{
	public bool TryCreateSerializableType(TypeDefinition typeDefinition,
		[NotNullWhen(true)] out SerializableType? result,
		[NotNullWhen(false)] out string? failureReason)
	{
		return TryCreateSerializableType(typeDefinition, new(runtimeContext?.SignatureComparer), out result, out failureReason);
	}

	public bool TryCreateSerializableType(
		TypeDefinition typeDefinition,
		Dictionary<ITypeDefOrRef, SerializableType> typeCache,
		[NotNullWhen(true)] out SerializableType? result,
		[NotNullWhen(false)] out string? failureReason)
	{
		Stack<MonoType> typeStack = new();
		bool returnValue = TryCreateSerializableType(typeDefinition, typeCache, typeStack, out result, out failureReason);
		Debug.Assert(typeStack.Count == 0, "The type stack should be empty after processing.");
		return returnValue;
	}

	private bool TryCreateSerializableType(
		TypeSignature typeSignature,
		Dictionary<ITypeDefOrRef, SerializableType> typeCache,
		Stack<MonoType> typeStack,
		[NotNullWhen(true)] out SerializableType? result,
		[NotNullWhen(false)] out string? failureReason)
	{
		if (typeSignature is GenericInstanceTypeSignature genericInstanceType)
		{
			return TryCreateSerializableType(genericInstanceType, typeCache, typeStack, out result, out failureReason);
		}
		TypeDefinition? typeDefinition = typeSignature.TryResolve(runtimeContext);
		if (typeDefinition is null)
		{
			result = null;
			failureReason = $"Failed to resolve type signature {typeSignature.FullName}.";
			return false;
		}
		else
		{
			return TryCreateSerializableType(typeDefinition, typeCache, typeStack, out result, out failureReason);
		}
	}

	private bool TryCreateSerializableType(
		TypeDefinition typeDefinition,
		Dictionary<ITypeDefOrRef, SerializableType> typeCache,
		Stack<MonoType> typeStack,
		[NotNullWhen(true)] out SerializableType? result,
		[NotNullWhen(false)] out string? failureReason)
	{
		if (typeCache.TryGetValue(typeDefinition, out SerializableType? cachedType))
		{
			result = cachedType;
			failureReason = null;
			return true;
		}
		if (typeDefinition.GenericParameters.Count > 0)
		{
			result = null;
			failureReason = "Generic types are not serializable.";
			return false;
		}
		if (typeDefinition.TryGetPrimitiveType(out PrimitiveType primitiveType))
		{
			result = SerializablePrimitiveType.GetOrCreate(primitiveType);
			typeCache.Add(typeDefinition, result);
			failureReason = null;
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
			if (!TryCreateSerializableType(typeDefinition.BaseType.ToTypeSignature(runtimeContext), typeCache, typeStack, out SerializableType? baseType, out failureReason))
			{
				typeCache.Remove(typeDefinition);
				typeStack.Pop();
				result = null;
				return false;
			}
			else
			{
				fields.EnsureCapacity(baseType.Fields.Count + typeDefinition.Fields.Count);
				AddInheritedFields(fields, baseType, monoType);
			}
		}
		else
		{
			fields.EnsureCapacity(typeDefinition.Fields.Count);
		}

		if (TryCreateSerializableFields(typeStack, monoType, fields, GetFieldsInType(typeDefinition), typeCache, out failureReason))
		{
			if (monoType.ContainsSerializeReference
				&& (typeDefinition.InheritsFromMonoBehaviour(runtimeContext) || typeDefinition.InheritsFromScriptableObject(runtimeContext)))
			{
				fields.Add(ManagedReferenceTypes.RegistryField);
			}
			monoType.SetDepth();
			typeStack.Pop();
			result = monoType;
			return true;
		}
		else
		{
			typeCache.Remove(typeDefinition);
			typeStack.Pop();
			result = null;
			return false;
		}
	}

	private bool TryCreateSerializableType(
		GenericInstanceTypeSignature genericInst,
		Dictionary<ITypeDefOrRef, SerializableType> typeCache,
		Stack<MonoType> typeStack,
		[NotNullWhen(true)] out SerializableType? result,
		[NotNullWhen(false)] out string? failureReason)
	{
		ITypeDefOrRef typeCacheKey = genericInst.ToTypeDefOrRef();
		if (typeCache.TryGetValue(typeCacheKey, out SerializableType? cachedType))
		{
			result = cachedType;
			failureReason = null;
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
			return false;
		}
		else if (baseType is not null)
		{
			if (!TryCreateSerializableType(baseType, typeCache, typeStack, out SerializableType? baseMonoType, out failureReason))
			{
				typeCache.Remove(typeCacheKey);
				typeStack.Pop();
				result = null;
				return false;
			}
			else
			{
				fields.EnsureCapacity(baseMonoType.Fields.Count + genericInst.GenericType.Resolve(runtimeContext).Fields.Count);
				AddInheritedFields(fields, baseMonoType, monoType);
			}
		}
		else
		{
			fields.EnsureCapacity(genericInst.GenericType.Resolve(runtimeContext).Fields.Count);
		}

		if (TryCreateSerializableFields(typeStack, monoType, fields, GetFieldsInType(genericInst), typeCache, out failureReason))
		{
			monoType.SetDepth();
			typeStack.Pop();
			result = monoType;
			return true;
		}
		else
		{
			typeCache.Remove(typeCacheKey);
			typeStack.Pop();
			result = null;
			return false;
		}
	}

	private bool TryCreateSerializableFields(
		Stack<MonoType> typeStack,
		MonoType monoType,
		List<Field> fields,
		IEnumerable<(FieldDefinition, TypeSignature)> enumerable,
		Dictionary<ITypeDefOrRef, SerializableType> typeCache,
		[NotNullWhen(false)] out string? failureReason)
	{
		foreach ((FieldDefinition, TypeSignature) pair in enumerable)
		{
			(FieldDefinition fieldDefinition, TypeSignature fieldType) = pair;
			if (WillUnitySerialize(fieldDefinition, fieldType))
			{
				if (fieldDefinition.HasSerializeReferenceAttribute())
				{
					//The object itself is stored in the managed reference registry, so the field only holds its identifier.
					SerializableType managedReferenceType = HasStableReferenceIds
						? ManagedReferenceTypes.ManagedReference
						: ManagedReferenceTypes.IndexedManagedReference;
					fields.Add(new Field(managedReferenceType, GetManagedReferenceArrayDepth(fieldType), fieldDefinition.Name ?? "", false));
					monoType.ContainsSerializeReference = true;
					continue;
				}

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

				if (TryCreateSerializableField(typeStack, fieldDefinition.Name ?? "", fieldType, arrayDepth, typeCache, out Field field, out failureReason))
				{
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
						if (field.Type is MonoType { ContainsSerializeReference: true })
						{
							monoType.ContainsSerializeReference = true;
						}
						fields.Add(field);
					}
				}
				else
				{
					return false;
				}
			}
		}
		failureReason = null;
		return true;
	}

	/// <summary>
	/// Add the fields of a base type, excluding its managed reference registry.
	/// </summary>
	/// <remarks>
	/// The registry is always the last field of the most derived type, so a derived type adds its own instead of inheriting one.
	/// </remarks>
	private static void AddInheritedFields(List<Field> fields, SerializableType baseType, MonoType monoType)
	{
		if (baseType is MonoType { ContainsSerializeReference: true })
		{
			monoType.ContainsSerializeReference = true;
		}

		IReadOnlyList<Field> baseFields = baseType.Fields;
		int count = baseFields.Count;
		if (count > 0 && baseFields[count - 1].Type == ManagedReferenceTypes.Registry)
		{
			count--;
		}
		for (int i = 0; i < count; i++)
		{
			fields.Add(baseFields[i]);
		}
	}

	/// <summary>
	/// Get the array depth of a field with the [SerializeReference] attribute.
	/// </summary>
	/// <remarks>
	/// Unity stores an identifier for each referenced object, so the element type is irrelevant.
	/// </remarks>
	private int GetManagedReferenceArrayDepth(TypeSignature typeSignature)
	{
		int arrayDepth = 0;
		while (true)
		{
			if (typeSignature is SzArrayTypeSignature szArrayTypeSignature)
			{
				arrayDepth++;
				typeSignature = szArrayTypeSignature.BaseType;
			}
			else if (typeSignature is GenericInstanceTypeSignature genericInstanceTypeSignature && AsmUtils.IsGenericList(genericInstanceTypeSignature, runtimeContext))
			{
				arrayDepth++;
				typeSignature = genericInstanceTypeSignature.TypeArguments[0];
			}
			else
			{
				return arrayDepth;
			}
		}
	}

	private bool TryCreateSerializableField(
		Stack<MonoType> typeStack,
		string name,
		TypeSignature typeSignature,
		int arrayDepth,
		Dictionary<ITypeDefOrRef, SerializableType> typeCache,
		out Field result,
		[NotNullWhen(false)] out string? failureReason)
	{
		switch (typeSignature)
		{
			case TypeDefOrRefSignature typeDefOrRefSignature:
				TypeDefinition typeDefinition = typeDefOrRefSignature.Type.Resolve(runtimeContext);
				SerializableType fieldType;
				if (typeDefinition.IsEnum)
				{
					CorLibTypeSignature enumValueType = (CorLibTypeSignature?)typeDefinition.GetEnumUnderlyingType() ?? throw new("Failed to resolve enum underlying type.");
					PrimitiveType primitiveType = enumValueType.ToPrimitiveType();
					fieldType = SerializablePrimitiveType.GetOrCreate(primitiveType);
				}
				else if (typeDefinition.InheritsFromObject(runtimeContext))
				{
					fieldType = SerializablePointerType.Shared;
				}
				else if (typeCache.TryGetValue(typeDefinition, out SerializableType? cachedMonoType))
				{
					//This needs to come after the InheritsFromObject check so that those fields get properly converted into PPtr assets.
					fieldType = cachedMonoType;
				}
				else if (TryCreateSerializableType(typeDefinition, typeCache, typeStack, out SerializableType? monoType, out failureReason))
				{
					fieldType = monoType;
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
				return TryCreateSerializableField(typeStack, name, szArrayTypeSignature.BaseType, arrayDepth + 1, typeCache, out result, out failureReason);

			case GenericInstanceTypeSignature genericInstanceTypeSignature:
				if (genericInstanceTypeSignature.InheritsFromObject(runtimeContext))
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
					return TryCreateSerializableField(typeStack, name, genericInstanceTypeSignature.TypeArguments[0], arrayDepth + 1, typeCache, out result, out failureReason);
				}
				else if (TryCreateSerializableType(genericInstanceTypeSignature, typeCache, typeStack, out SerializableType? monoType, out failureReason))
				{
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

	private bool TryGetBaseType(GenericInstanceTypeSignature genericInstanceType, out TypeSignature? baseType)
	{
		TypeDefinition? typeDefinition = genericInstanceType.GenericType.TryResolve(runtimeContext);
		if (typeDefinition is null)
		{
			baseType = null;
			return false;
		}

		baseType = typeDefinition.BaseType?.ToTypeSignature(runtimeContext).InstantiateGenericTypes(new GenericContext(genericInstanceType, null));
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

	private IEnumerable<(FieldDefinition, TypeSignature)> GetFieldsInType(GenericInstanceTypeSignature genericInst)
	{
		TypeDefinition? typeDefinition = genericInst.TryResolve(runtimeContext);
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
