using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AssetRipper.Core.Structure.Assembly.Serializable;
using AssetRipper.SerializationLogic;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Core.Structure.Assembly.Mono
{
	internal class MonoType : SerializableType
	{
		//From the deleted MonoFieldContext: structs are only serializable on 4.5.0 and greater.

		public MonoType(TypeDefinition typeDefinition) : base(typeDefinition.Namespace ?? "", PrimitiveType.Complex, typeDefinition.Name ?? "")
		{
			List<Field> fields = new();
			foreach (FieldDefinition fieldDefinition in GetFieldDefinitionsInTypeAndBase(typeDefinition))
			{
				if (FieldSerializationLogic.WillUnitySerialize(fieldDefinition))
				{
					fields.Add(MakeSerializableField(fieldDefinition));
				}
			}
			Fields = fields;
		}

		private static Field MakeSerializableField(FieldDefinition fieldDefinition)
		{
			return MakeSerializableField(
				fieldDefinition.Name ?? throw new NullReferenceException(),
				fieldDefinition.Signature?.FieldType ?? throw new NullReferenceException(),
				0);
		}

		private static Field MakeSerializableField(string name, TypeSignature typeSignature, int arrayDepth)
		{
			switch(typeSignature)
			{
				case TypeDefOrRefSignature typeDefOrRefSignature:
					TypeDefinition typeDefinition = typeDefOrRefSignature.ToTypeDefinition();
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
					return new Field(new SerializablePrimitiveType(corLibTypeSignature.ElementType.ToPrimitiveType()), arrayDepth, name);
				
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
			else
			{
				throw new NotSupportedException(typeSignature.FullName);
			}
		}

		private static IEnumerable<FieldDefinition> GetFieldDefinitionsInTypeAndBase(TypeDefinition typeDefinition)
		{
			Stack<TypeDefinition> hierarchy = new();
			
			TypeDefinition? current = typeDefinition;
			while (current is not null)
			{
				hierarchy.Push(current);
				current = current.TryGetBaseClass();
			}

			foreach (TypeDefinition type in hierarchy)
			{
				foreach (FieldDefinition fieldDefinition in type.Fields)
				{
					if (!fieldDefinition.IsStatic)
					{
						yield return fieldDefinition;
					}
				}
			}
		}

	}

	internal class SerializablePrimitiveType : SerializableType
	{
		public SerializablePrimitiveType(PrimitiveType primitiveType) : base("System", primitiveType, primitiveType.ToSystemTypeName())
		{
		}
	}

	internal class SerializablePointerType : SerializableType
	{
		public SerializablePointerType() : base("UnityEngine", PrimitiveType.Complex, "Object")
		{
		}
	}

	internal static class TypeDefinitionExtensions
	{
		public static TypeDefinition ToTypeDefinition(this TypeDefOrRefSignature typeDefOrRefSignature)
		{
			return typeDefOrRefSignature.Type.Resolve()
				?? throw new NullReferenceException($"Could not resolve {typeDefOrRefSignature.FullName}");
		}
		public static bool InheritsFromMonoBehaviour(this TypeDefinition type)
		{
			return type.InheritsFrom("UnityEngine.MonoBehaviour");
		}
		public static bool InheritsFromObject(this TypeDefinition type)
		{
			return type.InheritsFrom("UnityEngine.Object");
		}
		public static TypeDefinition? TryGetBaseClass(this TypeDefinition current)
		{
			return current.BaseType?.Resolve();
		}
		public static PrimitiveType ToPrimitiveType(this ElementType elementType)
		{
			return elementType switch
			{
				ElementType.Boolean => PrimitiveType.Bool,
				ElementType.Char => PrimitiveType.Char,
				ElementType.I1 => PrimitiveType.SByte,
				ElementType.U1 => PrimitiveType.Byte,
				ElementType.I2 => PrimitiveType.Short,
				ElementType.U2 => PrimitiveType.UShort,
				ElementType.I4 => PrimitiveType.Int,
				ElementType.U4 => PrimitiveType.UInt,
				ElementType.I8 => PrimitiveType.Long,
				ElementType.U8 => PrimitiveType.ULong,
				ElementType.R4 => PrimitiveType.Single,
				ElementType.R8 => PrimitiveType.Double,
				ElementType.String => PrimitiveType.String,
				_ => throw new ArgumentOutOfRangeException(nameof(elementType)),
			};
		}
	}
}
