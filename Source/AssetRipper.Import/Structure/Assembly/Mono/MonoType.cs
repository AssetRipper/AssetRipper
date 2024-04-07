using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using AssetRipper.Import.Structure.Assembly.Serializable;
using AssetRipper.SerializationLogic;
using AssetRipper.SerializationLogic.Extensions;
using System.Numerics;

namespace AssetRipper.Import.Structure.Assembly.Mono
{
	internal sealed class MonoType : SerializableType
	{
		//From the deleted MonoFieldContext: structs are only serializable on 4.5.0 and greater.

		private MonoType(ITypeDefOrRef type) : base(type.Namespace ?? "", PrimitiveType.Complex, type.Name ?? "")
		{
		}

		private MonoType(ITypeDefOrRef type, IReadOnlyList<Field> fields) : this(type)
		{
			Fields = fields;
		}

		public static bool TryCreate(
			TypeDefinition typeDefinition,
			Dictionary<TypeDefinition, MonoType> typeCache,
			[NotNullWhen(true)] out MonoType? result,
			[NotNullWhen(false)] out string? failureReason)
		{
			//Ensure we allocate some initial space so that we have less chance of needing to resize the list.
			List<Field> fields = new(RoundUpToPowerOf2(typeDefinition.Fields.Count));

			//Caching before completion prevents infinite loops.
			result = new(typeDefinition, fields);
			typeCache.Add(typeDefinition, result);

			foreach ((FieldDefinition fieldDefinition, TypeSignature fieldType) in FieldQuery.GetFieldsInTypeAndBase(typeDefinition))
			{
				if (FieldSerializationLogic.WillUnitySerialize(fieldDefinition, fieldType))
				{
					if (FieldSerializationLogic.HasSerializeReferenceAttribute(fieldDefinition))
					{
						typeCache.Remove(typeDefinition);
						return FailBecauseOfSerializeReference(fieldDefinition, out result, out failureReason);
					}
					else if (TryCreateSerializableField(fieldDefinition, fieldType, typeCache, out Field field, out failureReason))
					{
						fields.Add(field);
					}
					else
					{
						typeCache.Remove(typeDefinition);
						result = null;
						return false;
					}
				}
			}

			failureReason = null;
			return true;
		}

		public static bool TryCreate(
			GenericInstanceTypeSignature genericInst,
			Dictionary<TypeDefinition, MonoType> typeCache,
			[NotNullWhen(true)] out MonoType? result,
			[NotNullWhen(false)] out string? failureReason)
		{
			List<Field> fields = new();
			foreach ((FieldDefinition fieldDefinition, TypeSignature fieldType) in FieldQuery.GetFieldsInTypeAndBase(genericInst))
			{
				if (FieldSerializationLogic.WillUnitySerialize(fieldDefinition, fieldType))
				{
					if (FieldSerializationLogic.HasSerializeReferenceAttribute(fieldDefinition))
					{
						return FailBecauseOfSerializeReference(fieldDefinition, out result, out failureReason);
					}
					else if (TryCreateSerializableField(fieldDefinition, fieldType, typeCache, out Field field, out failureReason))
					{
						fields.Add(field);
					}
					else
					{
						result = null;
						return false;
					}
				}
			}

			result = new(genericInst.GenericType, fields);
			failureReason = null;
			return true;
		}

		private static bool FailBecauseOfSerializeReference(FieldDefinition fieldDefinition, out MonoType? result, out string? failureReason)
		{
			result = null;
			failureReason = $"{fieldDefinition.DeclaringType?.FullName}.{fieldDefinition.Name} uses the [SerializeReference] attribute, which is currently not supported.";
			return false;
		}

		private static int RoundUpToPowerOf2(int value)
		{
			unchecked
			{
				return (int)BitOperations.RoundUpToPowerOf2((uint)value);
			}
		}

		private static bool TryCreateSerializableField(
			FieldDefinition fieldDefinition,
			TypeSignature fieldType,
			Dictionary<TypeDefinition, MonoType> typeCache,
			out Field result,
			[NotNullWhen(false)] out string? failureReason)
		{
			return TryCreateSerializableField(
				fieldDefinition.Name ?? throw new NullReferenceException(),
				fieldType,
				0,
				typeCache,
				out result,
				out failureReason);
		}

		private static bool TryCreateSerializableField(
			string name,
			TypeSignature typeSignature,
			int arrayDepth,
			Dictionary<TypeDefinition, MonoType> typeCache,
			out Field result,
			[NotNullWhen(false)] out string? failureReason)
		{
			switch (typeSignature)
			{
				case TypeDefOrRefSignature typeDefOrRefSignature:
					TypeDefinition typeDefinition = typeDefOrRefSignature.Type.Resolve()
						?? throw new NullReferenceException($"Could not resolve {typeDefOrRefSignature.FullName}");
					SerializableType fieldType;
					if (typeDefinition.IsEnum)
					{
						TypeSignature enumValueType = typeDefinition.Fields.Single(f => !f.IsStatic).Signature!.FieldType;
						PrimitiveType primitiveType = ((CorLibTypeSignature)enumValueType).ToPrimitiveType();
						fieldType = SerializablePrimitiveType.GetOrCreate(primitiveType);
					}
					else if (typeDefinition.InheritsFromObject())
					{
						fieldType = SerializablePointerType.Shared;
					}
					else if (MonoUtils.IsPropertyName(typeDefinition))
					{
						//In the managed editor code, PropertyName is only backed by an int ID field.
						//However, in yaml and release binaries, it appears identical to Utf8String.
						//Presumably, editor binaries are the same, but this was not verified.
						fieldType = SerializablePrimitiveType.GetOrCreate(PrimitiveType.String);
					}
					else if (typeCache.TryGetValue(typeDefinition, out MonoType? cachedMonoType))
					{
						//This needs to come after the InheritsFromObject check so that those fields get properly converted into PPtr assets.
						fieldType = cachedMonoType;
					}
					else if (TryCreate(typeDefinition, typeCache, out MonoType? monoType, out failureReason))
					{
						fieldType = monoType;
					}
					else
					{
						result = default;
						return false;
					}

					result = new Field(fieldType, arrayDepth, name);
					failureReason = null;
					return true;

				case CorLibTypeSignature corLibTypeSignature:
					result = new Field(SerializablePrimitiveType.GetOrCreate(corLibTypeSignature.ToPrimitiveType()), arrayDepth, name);
					failureReason = null;
					return true;

				case SzArrayTypeSignature szArrayTypeSignature:
					return TryCreateSerializableField(name, szArrayTypeSignature.BaseType, arrayDepth + 1, typeCache, out result, out failureReason);

				case GenericInstanceTypeSignature genericInstanceTypeSignature:
					return TryCreateSerializableField(name, genericInstanceTypeSignature, arrayDepth, typeCache, out result, out failureReason);

				default:
					result = default;
					failureReason = $"{typeSignature.FullName} not supported.";
					return false;
			};
		}

		private static bool TryCreateSerializableField(
			string name,
			GenericInstanceTypeSignature typeSignature,
			int arrayDepth,
			Dictionary<TypeDefinition, MonoType> typeCache,
			out Field result,
			[NotNullWhen(false)] out string? failureReason)
		{
			if (typeSignature.GenericType.FullName is "System.Collections.Generic.List`1")
			{
				return TryCreateSerializableField(name, typeSignature.TypeArguments[0], arrayDepth + 1, typeCache, out result, out failureReason);
			}
			else if (TryCreate(typeSignature, typeCache, out MonoType? monoType, out failureReason))
			{
				result = new(monoType, arrayDepth, name);
				return true;
			}
			else
			{
				result = default;
				return false;
			}
		}
	}
}
