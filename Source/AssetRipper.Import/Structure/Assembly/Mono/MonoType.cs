using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using AssetRipper.Import.Structure.Assembly.Serializable;
using AssetRipper.SerializationLogic;
using AssetRipper.SerializationLogic.Extensions;

namespace AssetRipper.Import.Structure.Assembly.Mono
{
	internal sealed class MonoType : SerializableType
	{
		//From the deleted MonoFieldContext: structs are only serializable on 4.5.0 and greater.

		private MonoType(ITypeDefOrRef type) : base(type.Namespace ?? "", PrimitiveType.Complex, type.Name ?? "")
		{
		}

		public MonoType(TypeDefinition typeDefinition, Dictionary<TypeDefinition, MonoType> typeCache) : this(typeDefinition)
		{
			typeCache.Add(typeDefinition, this);
			List<Field> fields = new(typeDefinition.Fields.Count); //Ensure we allocate some initial space so that we have less chance of needing to resize the list.
			foreach ((FieldDefinition fieldDefinition, TypeSignature fieldType) in FieldQuery.GetFieldsInTypeAndBase(typeDefinition))
			{
				if (FieldSerializationLogic.WillUnitySerialize(fieldDefinition, fieldType))
				{
					fields.Add(MakeSerializableField(fieldDefinition, fieldType, typeCache));
				}
			}
			Fields = fields;
		}

		public MonoType(GenericInstanceTypeSignature genericInst, Dictionary<TypeDefinition, MonoType> typeCache) : this(genericInst.GenericType)
		{
			List<Field> fields = new();
			foreach ((FieldDefinition fieldDefinition, TypeSignature fieldType) in FieldQuery.GetFieldsInTypeAndBase(genericInst))
			{
				if (FieldSerializationLogic.WillUnitySerialize(fieldDefinition, fieldType))
				{
					fields.Add(MakeSerializableField(fieldDefinition, fieldType, typeCache));
				}
			}
			Fields = fields;
		}

		private static Field MakeSerializableField(FieldDefinition fieldDefinition, TypeSignature fieldType, Dictionary<TypeDefinition, MonoType> typeCache)
		{
			return MakeSerializableField(
				fieldDefinition.Name ?? throw new NullReferenceException(),
				fieldType,
				0,
				typeCache);
		}

		private static Field MakeSerializableField(string name, TypeSignature typeSignature, int arrayDepth, Dictionary<TypeDefinition, MonoType> typeCache)
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
					else
					{
						fieldType = new MonoType(typeDefinition, typeCache);
					}

					return new Field(fieldType, arrayDepth, name);

				case CorLibTypeSignature corLibTypeSignature:
					return new Field(SerializablePrimitiveType.GetOrCreate(corLibTypeSignature.ToPrimitiveType()), arrayDepth, name);

				case SzArrayTypeSignature szArrayTypeSignature:
					return MakeSerializableField(name, szArrayTypeSignature.BaseType, arrayDepth + 1, typeCache);

				case GenericInstanceTypeSignature genericInstanceTypeSignature:
					return MakeSerializableField(name, genericInstanceTypeSignature, arrayDepth, typeCache);

				default:
					throw new NotSupportedException(typeSignature.FullName);
			};
		}

		private static Field MakeSerializableField(string name, GenericInstanceTypeSignature typeSignature, int arrayDepth, Dictionary<TypeDefinition, MonoType> typeCache)
		{
			if (typeSignature.GenericType.FullName is "System.Collections.Generic.List`1")
			{
				return MakeSerializableField(name, typeSignature.TypeArguments[0], arrayDepth + 1, typeCache);
			}

			return new(new MonoType(typeSignature, typeCache), arrayDepth, name);
		}
	}
}
