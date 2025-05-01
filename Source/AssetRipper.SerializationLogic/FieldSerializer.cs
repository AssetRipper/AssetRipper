using AssetRipper.Primitives;
using AssetRipper.SerializationLogic.Extensions;
using System.Diagnostics;
using System.Numerics;
using static AssetRipper.SerializationLogic.SerializableType;

namespace AssetRipper.SerializationLogic;

public readonly struct FieldSerializer(UnityVersion version)
{
	public UnityVersion Version => version;
	/// <summary>
	/// Not sure about the exact version boundary, structs are supposedly only serializable on 4.5.0 and greater.
	/// </summary>
	private bool IsStructSerializable => version.GreaterThanOrEquals(4, 5);
	/// <summary>
	/// Not sure about the exact version boundary, but online references suggest that 2017 was the first version to support this.
	/// </summary>
	/// <remarks>
	/// <see href="https://github.com/AssetRipper/AssetRipper/issues/647"/>
	/// </remarks>
	private bool IsInt64Serializable => version.GreaterThanOrEquals(2017);
	/// <summary>
	/// Prior to some unknown version, System.Collections.Generic.List`1 and UnityEngine.ExposedReference`1 were the only supported generic types.
	/// </summary>
	private bool IsGenericInstanceSerializable => true;

	private bool WillUnitySerialize(FieldDefinition field, TypeSignature type)
	{
		return FieldSerializationLogic.WillUnitySerialize(field, type);
	}

	public bool TryCreateSerializableType(TypeDefinition typeDefinition,
		[NotNullWhen(true)] out SerializableType? result,
		[NotNullWhen(false)] out string? failureReason)
	{
		return TryCreateSerializableType(typeDefinition, new(SignatureComparer.Default), out result, out failureReason);
	}

	public bool TryCreateSerializableType(TypeDefinition typeDefinition,
		Dictionary<ITypeDefOrRef, SerializableType> typeCache,
		[NotNullWhen(true)] out SerializableType? result,
		[NotNullWhen(false)] out string? failureReason)
	{
		Stack<MonoType> typeStack = new();
		bool returnValue = TryCreateSerializableType(typeDefinition, typeCache, typeStack, out result, out failureReason);
		Debug.Assert(typeStack.Count == 0, "The type stack should be empty after processing.");
		return returnValue;
	}

	private bool TryCreateSerializableType(TypeDefinition typeDefinition,
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
		List<Field> fields = new(RoundUpToPowerOf2(typeDefinition.Fields.Count));

		//Caching before completion prevents infinite loops.
		MonoType monoType = new(typeDefinition, fields);
		typeCache.Add(typeDefinition, monoType);
		typeStack.Push(monoType);

		if (TryCreateSerializableFields(typeStack, monoType, fields, FieldQuery.GetFieldsInTypeAndBase(typeDefinition), typeCache, out failureReason))
		{
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
		List<Field> fields = new();

		MonoType monoType = new(genericInst.GenericType, fields);
		typeCache.Add(genericInst.ToTypeDefOrRef(), monoType);
		typeStack.Push(monoType);

		if (TryCreateSerializableFields(typeStack, monoType, fields, FieldQuery.GetFieldsInTypeAndBase(genericInst), typeCache, out failureReason))
		{
			monoType.SetDepth();
			typeStack.Pop();
			result = monoType;
			return true;
		}
		else
		{
			typeCache.Remove(genericInst.ToTypeDefOrRef());
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
		foreach ((FieldDefinition fieldDefinition, TypeSignature fieldType) in enumerable)
		{
			if (WillUnitySerialize(fieldDefinition, fieldType))
			{
				if (FieldSerializationLogic.HasSerializeReferenceAttribute(fieldDefinition))
				{
					failureReason = $"{fieldDefinition.DeclaringType?.FullName}.{fieldDefinition.Name} uses the [SerializeReference] attribute, which is currently not supported.";
					return false;
				}
				else if (TryCreateSerializableField(typeStack, fieldDefinition.Name ?? "", fieldType, 0, typeCache, out Field field, out failureReason))
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
				if (typeCache.TryGetValue(genericInstanceTypeSignature.ToTypeDefOrRef(), out SerializableType? cachedGenericMonoType))
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

	private static int RoundUpToPowerOf2(int value)
	{
		unchecked
		{
			return (int)BitOperations.RoundUpToPowerOf2((uint)value);
		}
	}
}
