using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using AssetRipper.SerializationLogic.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.SerializationLogic
{
	public static class FieldSerializationLogic
	{
		private static readonly SignatureComparer signatureComparer = new();

		public static bool WillUnitySerialize(FieldDefinition fieldDefinition)
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
			TypeSignature? typeReference = fieldDefinition.Signature!.FieldType;

			//the type of the field must be serializable in the first place.

			if (typeReference is CorLibTypeSignature corLibTypeSignature && corLibTypeSignature.ElementType == ElementType.String)
			{
				return true;
			}

			if (typeReference.IsValueType)
			{
				return IsValueTypeSerializable(typeReference);
			}

			if (typeReference is SzArrayTypeSignature || AsmUtils.IsGenericList(typeReference))
			{
				if (!HasSerializeReferenceAttribute(fieldDefinition))
				{
					return IsSupportedCollection(typeReference);
				}
			}


			if (!IsReferenceTypeSerializable(typeReference) && !HasSerializeReferenceAttribute(fieldDefinition))
			{
				return false;
			}

			if (IsDelegate(typeReference))
			{
				return false;
			}

			return true;
		}

		private static bool IsDelegate(ITypeDescriptor typeReference)
		{
			return typeReference.IsAssignableTo("System.Delegate");
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
			return FieldAttributes(field).Any(EngineTypePredicates.IsSerializeFieldAttribute);
		}

		public static bool HasSerializeReferenceAttribute(FieldDefinition field)
		{
			foreach (ITypeDefOrRef attribute in FieldAttributes(field))
			{
				if (EngineTypePredicates.IsSerializeReferenceAttribute(attribute))
				{
					return true;
				}
			}

			return false;
		}

		private static IEnumerable<ITypeDefOrRef> FieldAttributes(FieldDefinition field)
		{
			return field.CustomAttributes.Select(c => c.Constructor!.DeclaringType!);
		}

		public static bool ShouldNotTryToResolve(ITypeDescriptor typeReference)
		{
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

			return EngineTypePredicates.IsSerializableUnityStruct(typeReference) ||
				typeReference.IsEnum() ||
				ShouldImplementIDeserializable(typeReference);
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
				ShouldImplementIDeserializable(typeReference) ||
				EngineTypePredicates.IsSerializableUnityClass(typeReference))
			{
				return true;
			}

			return false;
		}

		private static bool IsTypeSerializable(ITypeDescriptor typeReference)
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
			return fieldDefinition.CustomAttributes.Count switch
			{
				0 => null,
				_ => fieldDefinition.CustomAttributes.SingleOrDefault(a => a.Constructor?.DeclaringType?.FullName == "System.Runtime.CompilerServices.FixedBufferAttribute")
			};
		}

		public static int GetFixedBufferLength(FieldDefinition fieldDefinition)
		{
			CustomAttribute? fixedBufferAttribute = GetFixedBufferAttribute(fieldDefinition);

			if (fixedBufferAttribute == null)
			{
				throw new ArgumentException(string.Format("Field '{0}' is not a fixed buffer field.", fieldDefinition.FullName));
			}

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

		public static bool IsNonSerialized(ITypeDescriptor typeDeclaration)
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

			if (typeDeclaration.ToTypeSignature() is CorLibTypeSignature corLibTypeSignature && corLibTypeSignature.ElementType == ElementType.Object)
			{
				return true;
			}

			string fullName = typeDeclaration.FullName;
			if (fullName.StartsWith("System.")) //can this be done better?
			{
				return true;
			}

			if (typeDeclaration.IsArray())
			{
				return true;
			}

			if (fullName == EngineTypePredicates.MonoBehaviour)
			{
				return true;
			}

			if (fullName == EngineTypePredicates.ScriptableObject)
			{
				return true;
			}

			if (typeDeclaration.IsEnum())
			{
				return true;
			}

			return false;
		}

		public static bool ShouldImplementIDeserializable(ITypeDescriptor typeDeclaration)
		{
			if (typeDeclaration.FullName == "UnityEngine.ExposedReference`1")
			{
				return true;
			}

			if (IsNonSerialized(typeDeclaration))
			{
				return false;
			}

			try
			{
				if (EngineTypePredicates.ShouldHaveHadSerializableAttribute(typeDeclaration))
				{
					return true;
				}

				TypeDefinition resolvedTypeDeclaration = typeDeclaration.CheckedResolve();
				if (resolvedTypeDeclaration.IsValueType)
				{
					return resolvedTypeDeclaration.IsSerializable && !resolvedTypeDeclaration.CustomAttributes.Any(a => a.Constructor?.DeclaringType?.FullName.Contains("System.Runtime.CompilerServices.CompilerGenerated") ?? false);
				}
				else
				{
					return (resolvedTypeDeclaration.IsSerializable && !resolvedTypeDeclaration.CustomAttributes.Any(a => a.Constructor?.DeclaringType?.FullName.Contains("System.Runtime.CompilerServices.CompilerGenerated") ?? false))
						|| resolvedTypeDeclaration.IsSubclassOf(EngineTypePredicates.MonoBehaviour, EngineTypePredicates.ScriptableObject);
				}
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
