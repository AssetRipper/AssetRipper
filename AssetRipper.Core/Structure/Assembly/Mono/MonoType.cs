using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using AssetRipper.Core.Structure.Assembly.Serializable;
using AssetRipper.SerializationLogic;
using AssetRipper.SerializationLogic.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Core.Structure.Assembly.Mono
{
	internal class MonoType : SerializableType
	{
		//From the deleted MonoFieldContext: structs are only serializable on 4.5.0 and greater.

		private MonoType(ITypeDefOrRef type) : base(type.Namespace ?? "", PrimitiveType.Complex, type.Name ?? "")
		{
			
		}

		public MonoType(TypeDefinition typeDefinition) : this((ITypeDefOrRef) typeDefinition)
		{
			List<Field> fields = new();
			foreach ((FieldDefinition fieldDefinition, TypeSignature fieldType) in FieldQuery.GetFieldsInTypeAndBase(typeDefinition))
			{
				if (FieldSerializationLogic.WillUnitySerialize(fieldDefinition, fieldType))
				{
					fields.Add(MakeSerializableField(fieldDefinition, fieldType));
				}
			}
			Fields = fields;
		}

		public MonoType(GenericInstanceTypeSignature genericInst) : this(genericInst.GenericType)
		{
			List<Field> fields = new();
			foreach ((FieldDefinition fieldDefinition, TypeSignature fieldType) in FieldQuery.GetFieldsInTypeAndBase(genericInst))
			{
				if (FieldSerializationLogic.WillUnitySerialize(fieldDefinition, fieldType))
				{
					fields.Add(MakeSerializableField(fieldDefinition, fieldType));
				}
			}
			Fields = fields;
		}

		private static Field MakeSerializableField(FieldDefinition fieldDefinition, TypeSignature fieldType)
		{
			return MakeSerializableField(
				fieldDefinition.Name ?? throw new NullReferenceException(),
				fieldType,
				0);
		}

		private static Field MakeSerializableField(string name, TypeSignature typeSignature, int arrayDepth)
		{
			switch(typeSignature)
			{
				case TypeDefOrRefSignature typeDefOrRefSignature:
					TypeDefinition typeDefinition = typeDefOrRefSignature.Type.Resolve()
						?? throw new NullReferenceException($"Could not resolve {typeDefOrRefSignature.FullName}");
					SerializableType fieldType;
					if (typeDefinition.IsEnum)
					{
						TypeSignature enumValueType = typeDefinition.Fields.Single(f => !f.IsStatic).Signature!.FieldType;
						PrimitiveType primitiveType = ((CorLibTypeSignature)enumValueType).ToPrimitiveType();
						fieldType = new SerializablePrimitiveType(primitiveType);
					}
					else if (typeDefinition.InheritsFromObject())
					{
						fieldType = new SerializablePointerType();
					}
					else
					{
						fieldType = new MonoType(typeDefinition);
					}

					return new Field(fieldType, arrayDepth, name);

				case CorLibTypeSignature corLibTypeSignature:
					return new Field(new SerializablePrimitiveType(corLibTypeSignature.ToPrimitiveType()), arrayDepth, name);
				
				case SzArrayTypeSignature szArrayTypeSignature:
					return MakeSerializableField(name, szArrayTypeSignature.BaseType, arrayDepth + 1);
				
				case GenericInstanceTypeSignature genericInstanceTypeSignature:
					return MakeSerializableField(name, genericInstanceTypeSignature, arrayDepth);
				
				default:
					throw new NotSupportedException(typeSignature.FullName);
			};
		}

		private static Field MakeSerializableField(string name, GenericInstanceTypeSignature typeSignature, int arrayDepth)
		{
			if (typeSignature.GenericType.FullName is "System.Collections.Generic.List`1")
			{
				return MakeSerializableField(name, typeSignature.TypeArguments[0], arrayDepth + 1);
			}

			return new(new MonoType(typeSignature), arrayDepth, name);
		}
	}
}
