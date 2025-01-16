using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
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
			Dictionary<ITypeDefOrRef, MonoType> typeCache,
			[NotNullWhen(true)] out MonoType? result,
			[NotNullWhen(false)] out string? failureReason)
		{
			// 检查是否为Naninovel脚本
			string? typeNamespace = typeDefinition.Namespace;
			if (!string.IsNullOrEmpty(typeNamespace) && typeNamespace.Contains("Naninovel"))
			{
				return TryCreateNaninovelType(typeDefinition, typeCache, out result, out failureReason);
			}

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
			Dictionary<ITypeDefOrRef, MonoType> typeCache,
			[NotNullWhen(true)] out MonoType? result,
			[NotNullWhen(false)] out string? failureReason)
		{
			List<Field> fields = new();

			result = new(genericInst.GenericType, fields);
			typeCache.Add(genericInst.ToTypeDefOrRef(), result);

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
						typeCache.Remove(genericInst.ToTypeDefOrRef());
						result = null;
						return false;
					}
				}
			}

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
			Dictionary<ITypeDefOrRef, MonoType> typeCache,
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
			Dictionary<ITypeDefOrRef, MonoType> typeCache,
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

					result = new Field(fieldType, arrayDepth, name, true);
					failureReason = null;
					return true;

				case CorLibTypeSignature corLibTypeSignature:
					result = new Field(SerializablePrimitiveType.GetOrCreate(corLibTypeSignature.ToPrimitiveType()), arrayDepth, name, true);
					failureReason = null;
					return true;

				case SzArrayTypeSignature szArrayTypeSignature:
					return TryCreateSerializableField(name, szArrayTypeSignature.BaseType, arrayDepth + 1, typeCache, out result, out failureReason);

				case GenericInstanceTypeSignature genericInstanceTypeSignature:
					if (typeCache.TryGetValue(genericInstanceTypeSignature.ToTypeDefOrRef(), out MonoType? cachedGenericMonoType))
					{
						result = new Field(cachedGenericMonoType, arrayDepth, name, true);
						failureReason = null;
						return true;
					}
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
			Dictionary<ITypeDefOrRef, MonoType> typeCache,
			out Field result,
			[NotNullWhen(false)] out string? failureReason)
		{
			if (typeSignature.GenericType.FullName is "System.Collections.Generic.List`1")
			{
				return TryCreateSerializableField(name, typeSignature.TypeArguments[0], arrayDepth + 1, typeCache, out result, out failureReason);
			}
			else if (TryCreate(typeSignature, typeCache, out MonoType? monoType, out failureReason))
			{
				result = new(monoType, arrayDepth, name, true);
				return true;
			}
			else
			{
				result = default;
				return false;
			}
		}

		private static bool TryCreateNaninovelType(
			TypeDefinition typeDefinition,
			Dictionary<ITypeDefOrRef, MonoType> typeCache,
			[NotNullWhen(true)] out MonoType? result,
			[NotNullWhen(false)] out string? failureReason)
		{
			List<Field> fields = new();
			
			// 首先处理基类
			if (typeDefinition.BaseType != null)
			{
				TypeDefinition? baseType = typeDefinition.BaseType.Resolve();
				if (baseType != null)
				{
					// 检查基类是否已经在缓存中
					if (!typeCache.TryGetValue(baseType, out var baseMonoType))
					{
						// 如果基类也是Naninovel类型，递归处理
						if (baseType.Namespace?.Contains("Naninovel") == true)
						{
							if (!TryCreateNaninovelType(baseType, typeCache, out baseMonoType, out failureReason))
							{
								result = null;
								return false;
							}
						}
						else if (!TryCreate(baseType, typeCache, out baseMonoType, out failureReason))
						{
							result = null;
							return false;
						}
					}

					// 添加基类的字段
					foreach (var field in baseMonoType.Fields)
					{
						fields.Add(field);
					}
				}
			}

			// 处理当前类型的字段
			foreach (FieldDefinition field in typeDefinition.Fields)
			{
				if (field.IsStatic || !field.IsPublic)
					continue;

				// 检查字段类型
				TypeSignature fieldType = field.Signature!.FieldType;
				bool isNaninovelField = IsNaninovelField(field, fieldType);

				try
				{
					if (isNaninovelField)
					{
						if (TryCreateNaninovelField(field, fieldType, typeCache, out Field serializableField, out failureReason))
						{
							fields.Add(serializableField);
						}
						else
						{
							// 如果Naninovel字段创建失败，尝试标准序列化
							if (TryCreateSerializableField(field, fieldType, typeCache, out serializableField, out _))
							{
								fields.Add(serializableField);
							}
						}
					}
					else if (FieldSerializationLogic.WillUnitySerialize(field, fieldType))
					{
						if (TryCreateSerializableField(field, fieldType, typeCache, out Field serializableField, out _))
						{
							fields.Add(serializableField);
						}
					}
				}
				catch (Exception)
				{
					// 忽略字段创建失败，继续处理其他字段
					continue;
				}
			}

			result = new MonoType(typeDefinition, fields);
			typeCache.Add(typeDefinition, result);
			failureReason = null;
			return true;
		}

		private static bool IsNaninovelField(FieldDefinition field, TypeSignature fieldType)
		{
			// 检查类型名称
			string? fullName = fieldType.FullName;
			if (!string.IsNullOrEmpty(fullName) && fullName.Contains("Naninovel"))
			{
				return true;
			}

			// 检查字段名称模式
			string fieldName = field.Name ?? "";
			if (!string.IsNullOrEmpty(fieldName))
			{
				string[] naninovelPatterns = new[]
				{
					"Script", "Text", "Command", "Parameter", "Expression",
					"Config", "Data", "Localization", "Variable", "State",
					"Choice", "Event", "Action", "Condition"
				};

				foreach (var pattern in naninovelPatterns)
				{
					if (fieldName.Contains(pattern, StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
				}
			}

			// 检查自定义特性
			foreach (var attribute in field.CustomAttributes)
			{
				var declaringType = attribute.Constructor?.DeclaringType;
				if (declaringType != null && declaringType.FullName != null)
				{
					string attrName = declaringType.FullName;
					if (attrName.Contains("Naninovel") ||
						attrName.Contains("SerializeField") ||
						attrName.Contains("SerializeReference"))
					{
						return true;
					}
				}
			}

			return false;
		}

		private static bool TryCreateNaninovelField(
			FieldDefinition fieldDefinition,
			TypeSignature fieldType,
			Dictionary<ITypeDefOrRef, MonoType> typeCache,
			out Field result,
			[NotNullWhen(false)] out string? failureReason)
		{
			string? fullName = fieldType.FullName;
			
			if (!string.IsNullOrEmpty(fullName))
			{
				// Naninovel特定类型列表
				string[] stringTypes = new[]
				{
					"NaniScript", "Script", "Command", "LocalizableText",
					"TranslatableText", "Expression", "Parameter",
					"Choice", "DialogueLine", "Variable", "State"
				};

				// 检查是否为需要作为字符串处理的类型
				foreach (var type in stringTypes)
				{
					if (fullName.Contains(type))
					{
						result = new Field(SerializablePrimitiveType.GetOrCreate(PrimitiveType.String), 0, fieldDefinition.Name ?? "", true);
						failureReason = null;
						return true;
					}
				}

				// 检查是否为数组或列表类型
				if (fieldType is GenericInstanceTypeSignature genericType)
				{
					if (genericType.GenericType.FullName == "System.Collections.Generic.List`1")
					{
						// 递归处理列表元素类型
						return TryCreateNaninovelField(fieldDefinition, genericType.TypeArguments[0], typeCache, out result, out failureReason);
					}
				}
			}

			// 对于其他Naninovel字段,尝试标准序列化
			return TryCreateSerializableField(fieldDefinition, fieldType, typeCache, out result, out failureReason);
		}
	}
}
