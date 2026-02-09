using AssetRipper.AssemblyDumper.Methods;
using AssetRipper.AssemblyDumper.Types;
using AssetRipper.Assets.Generics;
using System.Text;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass052_InterfacePropertiesAndMethods
{
	private static readonly SignatureComparer signatureComparer = new(SignatureComparisonFlags.VersionAgnostic);

	public static void DoPass()
	{
		foreach (ClassGroupBase group in SharedState.Instance.AllGroups)
		{
			group.ImplementProperties();
		}
	}

	private static void ImplementProperties(this ClassGroupBase group)
	{
		Dictionary<string, (string, TypeSignature, bool)> propertyDictionary = group.GetPropertyDictionary();
		HashSet<string> differingFieldNames = group.GetDifferingFieldNames();

		foreach ((string propertyName, (string fieldName, TypeSignature propertyTypeSignature, bool hasConflictingTypes)) in propertyDictionary)
		{
			bool missingOnSomeVersions = differingFieldNames.Contains(fieldName);
			bool isValueType = propertyTypeSignature.IsValueType;

			PropertyDefinition propertyDeclaration = group.AddInterfacePropertyDeclaration(propertyName, propertyTypeSignature);
			InterfaceProperty interfaceProperty = new InterfaceProperty(propertyDeclaration, group);
			group.InterfaceProperties.Add(interfaceProperty);

			foreach (GeneratedClassInstance instance in group.Instances)
			{
				FieldDefinition? field = instance.Type.TryGetFieldByName(fieldName, true);
				PropertyDefinition property;
				ClassProperty classProperty;
				if (hasConflictingTypes || missingOnSomeVersions)
				{
					if (field is not { Signature.FieldType: { } fieldType } || !instance.Class.ContainsField(fieldName))
					{
						// Field either:
						// * doesn't exist
						// * exists in a base class, but isn't present in this derived class
						property = instance.ImplementInterfaceProperty(interfaceProperty, null);
						classProperty = new ClassProperty(property, null, interfaceProperty, instance);
					}
					else if (!hasConflictingTypes || signatureComparer.Equals(fieldType, propertyTypeSignature))
					{
						property = instance.ImplementInterfaceProperty(interfaceProperty, field);
						classProperty = new ClassProperty(property, field, interfaceProperty, instance);

						if (propertyTypeSignature is SzArrayTypeSignature)
						{
							property.FixNullableArraySetMethod(field);
						}
					}
					else if (interfaceProperty.HasSetAccessor && fieldType.IsIntegerPrimitive(out ElementType fieldPrimitive) && propertyTypeSignature.IsIntegerPrimitive(out ElementType propertyPrimitive))
					{
						// Field exists, but is the wrong primitive type, so we convert.
						// Because we check HasSetAccessor, this does not run on PPtrs, which is fine.
						property = instance.Type.AddFullProperty(propertyName, InterfaceUtils.InterfacePropertyImplementation, propertyTypeSignature);

						// get
						{
							CilInstructionCollection instructions = property.GetMethod!.CilMethodBody!.Instructions;
							instructions.Add(CilOpCodes.Ldarg_0);
							instructions.Add(CilOpCodes.Ldfld, field);
							instructions.Add(propertyPrimitive switch
							{
								ElementType.U1 => CilOpCodes.Conv_U1,
								ElementType.U2 => CilOpCodes.Conv_U2,
								ElementType.U4 => CilOpCodes.Conv_U4,
								ElementType.U => CilOpCodes.Conv_U,
								ElementType.U8 => CilOpCodes.Conv_U8,
								ElementType.I1 => CilOpCodes.Conv_I1,
								ElementType.I2 => CilOpCodes.Conv_I2,
								ElementType.I4 => CilOpCodes.Conv_I4,
								ElementType.I => CilOpCodes.Conv_I,
								ElementType.I8 => CilOpCodes.Conv_I8,
								_ => throw new NotSupportedException(),
							});
							instructions.Add(CilOpCodes.Ret);
						}

						// set
						{
							CilInstructionCollection instructions = property.SetMethod!.CilMethodBody!.Instructions;
							instructions.Add(CilOpCodes.Ldarg_0);
							instructions.Add(CilOpCodes.Ldarg_1);
							instructions.Add(fieldPrimitive switch
							{
								ElementType.U1 => CilOpCodes.Conv_U1,
								ElementType.U2 => CilOpCodes.Conv_U2,
								ElementType.U4 => CilOpCodes.Conv_U4,
								ElementType.U => CilOpCodes.Conv_U,
								ElementType.U8 => CilOpCodes.Conv_U8,
								ElementType.I1 => CilOpCodes.Conv_I1,
								ElementType.I2 => CilOpCodes.Conv_I2,
								ElementType.I4 => CilOpCodes.Conv_I4,
								ElementType.I => CilOpCodes.Conv_I,
								ElementType.I8 => CilOpCodes.Conv_I8,
								_ => throw new NotSupportedException(),
							});
							instructions.Add(CilOpCodes.Stfld, field);
							instructions.Add(CilOpCodes.Ret);
						}

						// We give a null backing field here because it doesn't actually have one.
						// The other property is the one that actually has a backing field.
						classProperty = new ClassProperty(property, null, interfaceProperty, instance);
					}
					else
					{
						// Field is not compatible with the property
						property = instance.ImplementInterfaceProperty(interfaceProperty, null);
						classProperty = new ClassProperty(property, null, interfaceProperty, instance);
					}

					instance.Properties.Add(classProperty);
				}
				else
				{
					property = instance.ImplementInterfaceProperty(interfaceProperty, field);
					classProperty = new ClassProperty(property, field, interfaceProperty, instance);
					instance.Properties.Add(classProperty);
				}

				if (instance.Type.IsAbstract || instance.Group.ID is 2)
				{
					property.AddDebuggerBrowsableNeverAttribute();//Properties in base classes are redundant in the debugger.
				}
				else if (classProperty.IsAbsent)
				{
					property.AddDebuggerBrowsableNeverAttribute();//Dummy properties should not be visible in the debugger.
				}
				else if (classProperty.Name == "Name_R")
				{
					property.AddDebuggerBrowsableNeverAttribute();//Name_R is redundant in the debugger because a Name property exists.
				}
			}
		}
	}

	private static HashSet<string> GetDifferingFieldNames(this ClassGroupBase group)
	{
		List<(GeneratedClassInstance, List<string>)> data = new();
		List<string> allFieldNames = new();
		foreach (GeneratedClassInstance instance in group.Instances)
		{
			List<string> instanceFieldNames = instance.Class.GetFieldNames().ToList();
			data.Add((instance, instanceFieldNames));
			allFieldNames.AddRange(instanceFieldNames);
		}
		return allFieldNames.Distinct().Where(f => data.Any(pair => !pair.Item2.Contains(f))).ToHashSet();
	}

	/// <summary>
	/// Field name : List of field types
	/// </summary>
	private static Dictionary<string, List<TypeSignature>> GetFieldTypeListDictionary(this ClassGroupBase group)
	{
		Dictionary<string, List<TypeSignature>> result = new();
		foreach (GeneratedClassInstance instance in group.Instances)
		{
			foreach (string fieldName in instance.Class.GetFieldNames())
			{
				TypeSignature fieldType = instance.Type.GetFieldByName(fieldName, true).Signature!.FieldType;
				List<TypeSignature> typeList = result.GetOrAdd(fieldName);
				if (!typeList.Any(sig => signatureComparer.Equals(sig, fieldType)))
				{
					typeList.Add(fieldType);
				}
			}
		}
		return result;
	}

	private static Dictionary<string, T> SortStringDictionary<T>(this Dictionary<string, T> dictionary)
	{
		var keyList = dictionary.Keys.ToList();
		keyList.Sort();
		return keyList.ToDictionary(key => key, key => dictionary[key]);
	}

	/// <summary>
	/// Property name : field name, property type, type conflict
	/// </summary>
	private static Dictionary<string, (string, TypeSignature, bool)> GetPropertyDictionary(this ClassGroupBase group)
	{
		Dictionary<string, List<TypeSignature>> fieldTypeDictionary = group.GetFieldTypeListDictionary().SortStringDictionary();
		Dictionary<string, (string, TypeSignature, bool)> propertyDictionary = new();

		foreach ((string fieldName, List<TypeSignature> fieldTypeList) in fieldTypeDictionary)
		{
			string propertyName = GeneratedInterfaceUtils.GetPropertyNameFromFieldName(fieldName, group);
			if (fieldTypeList.Count == 1)
			{
				propertyDictionary.Add(propertyName, (fieldName, fieldTypeList[0], false));
			}
			else if (TryGetCommonInheritor(fieldTypeList, out TypeSignature? baseInterface))
			{
				propertyDictionary.Add(propertyName, (fieldName, baseInterface, false));
			}
			else if (TryGetCommonGenericInstance(fieldTypeList, out TypeSignature? accessTypeSignature))
			{
				propertyDictionary.Add(propertyName, (fieldName, accessTypeSignature, false));
			}
			else
			{
				foreach (TypeSignature fieldType in fieldTypeList)
				{
					string fieldTypeName = GetName(fieldType);
					propertyDictionary.Add($"{propertyName}_{fieldTypeName}", (fieldName, fieldType, true));
				}
			}
		}

		return propertyDictionary;
	}

	private static bool TryGetCommonGenericInstance(List<TypeSignature> fieldTypeList, [NotNullWhen(true)] out TypeSignature? accessTypeSignature)
	{
		accessTypeSignature = null;
		if (fieldTypeList.TryCast(out List<GenericInstanceTypeSignature>? genericInstanceFields))
		{
			if (genericInstanceFields.All(genericInstance => genericInstance.GenericType.Name == "AssetList`1"))
			{
				List<TypeSignature> typeArguments = genericInstanceFields.Select(genericInstance => genericInstance.TypeArguments.Single()).ToList();
				if (TryGetCommonInheritor(typeArguments, out TypeSignature? commonInterface))
				{
					accessTypeSignature = SharedState.Instance.Importer.ImportTypeSignature(typeof(AccessListBase<>)).MakeGenericInstanceType(commonInterface);
				}
			}
			else if (genericInstanceFields.All(genericInstance => genericInstance.GenericType.Name == "AssetDictionary`2"))
			{
				List<TypeSignature> keyTypeArguments = genericInstanceFields.Select(genericInstance => genericInstance.TypeArguments[0]).ToList();
				List<TypeSignature> valueTypeArguments = genericInstanceFields.Select(genericInstance => genericInstance.TypeArguments[1]).ToList();
				if (keyTypeArguments.TryGetEqualityOrCommonInheritor(out TypeSignature? commonKeyType)
					&& valueTypeArguments.TryGetEqualityOrCommonInheritor(out TypeSignature? commonValueType))
				{
					accessTypeSignature = SharedState.Instance.Importer.ImportTypeSignature(typeof(AccessDictionaryBase<,>))
						.MakeGenericInstanceType(commonKeyType, commonValueType);
				}
			}
			//Pair only used by Sprite and it's AssetPair<GUID, long>
		}
		return accessTypeSignature != null;
	}

	private static bool TryGetEqualityOrCommonInheritor(this List<TypeSignature> types, [NotNullWhen(true)] out TypeSignature? commonType)
	{
		return types.TryGetEquality(out commonType) || types.TryGetCommonInheritor(out commonType);
	}

	private static bool TryGetEquality(this List<TypeSignature> types, [NotNullWhen(true)] out TypeSignature? commonType)
	{
		TypeSignature first = types.First();
		if (types.All(type => signatureComparer.Equals(type, first)))
		{
			commonType = first;
			return true;
		}
		else
		{
			commonType = null;
			return false;
		}
	}

	private static bool TryGetCommonInheritor(this List<TypeSignature> types, [NotNullWhen(true)] out TypeSignature? baseInterface)
	{
		if (types.Count == 0)
		{
			throw new ArgumentException(null, nameof(types));
		}

		if (TryGetTypeDefinitionsForTypeSignatures(types, out List<TypeDefinition>? typeDefinitions))
		{
			ClassGroupBase group = SharedState.Instance.TypesToGroups[typeDefinitions[0]];
			if (!typeDefinitions.Any(def => !group.ContainsTypeDefinition(def)))
			{
				baseInterface = group.GetSingularTypeOrInterface().ToTypeSignature();
				return true;
			}
		}
		baseInterface = null;
		return false;
	}

	private static bool ContainsTypeDefinition(this ClassGroupBase group, TypeDefinition type)
	{
		//any instance where an instance's type definition is equal or the group interface is equal
		return group.Instances.Any(instance => signatureComparer.Equals(type, instance.Type)) || signatureComparer.Equals(type, group.Interface);
	}

	private static bool TryGetTypeDefinitionForTypeSignature(TypeSignature typeSignature, [NotNullWhen(true)] out TypeDefinition? typeDefinition)
	{
		typeDefinition = (typeSignature as TypeDefOrRefSignature)?.Type as TypeDefinition;
		return typeDefinition != null;
	}

	private static TypeDefinition? TryGetTypeDefinitionForTypeSignature(TypeSignature typeSignature)
	{
		return (typeSignature as TypeDefOrRefSignature)?.Type as TypeDefinition;
	}

	private static bool TryGetTypeDefinitionsForTypeSignatures(List<TypeSignature> typeSignatures, [NotNullWhen(true)] out List<TypeDefinition>? typeDefinitions)
	{
		typeDefinitions = new List<TypeDefinition>(typeSignatures.Count);
		foreach (TypeSignature typeSignature in typeSignatures)
		{
			if (TryGetTypeDefinitionForTypeSignature(typeSignature, out TypeDefinition? typeDefinition))
			{
				typeDefinitions.Add(typeDefinition);
			}
			else
			{
				typeDefinitions = null;
				return false;
			}
		}
		return true;
	}

	private static PropertyDefinition AddInterfacePropertyDeclaration(this ClassGroupBase group, string propertyName, TypeSignature propertyType)
	{
		return !group.IsPPtr && ShouldUseFullProperty(propertyType)
			? group.Interface.AddFullProperty(propertyName, InterfaceUtils.InterfacePropertyDeclaration, propertyType)
			: group.Interface.AddGetterProperty(propertyName, InterfaceUtils.InterfacePropertyDeclaration, propertyType);
	}

	private static bool ShouldUseFullProperty(TypeSignature propertyType)
	{
		return propertyType is SzArrayTypeSignature or CorLibTypeSignature || propertyType.IsUtf8String() || propertyType.IsValueType;
	}

	private static PropertyDefinition ImplementInterfaceProperty(this GeneratedClassInstance instance, InterfaceProperty interfaceProperty, FieldDefinition? field)
	{
		TypeDefinition declaringType = instance.Type;
		string propertyName = interfaceProperty.Name;
		TypeSignature propertyType = interfaceProperty.Definition.Signature!.ReturnType;
		if (interfaceProperty.HasSetAccessor)
		{
			return declaringType.ImplementFullProperty(propertyName, InterfaceUtils.InterfacePropertyImplementation, propertyType, field);
		}
		else if (field is not null && propertyType is GenericInstanceTypeSignature genericPropertyType)
		{
			IMethodDefOrRef constructor;
			switch (genericPropertyType.GenericType.Name?.Value)
			{
				case "AccessListBase`1":
					{
						GenericInstanceTypeSignature fieldType = field.Signature?.FieldType as GenericInstanceTypeSignature ?? throw new();
						TypeSignature underlyingType = fieldType.TypeArguments.Single();
						TypeSignature baseType = genericPropertyType.TypeArguments.Single();
						GenericInstanceTypeSignature accessListSignature = SharedState.Instance.Importer
							.ImportTypeSignature(typeof(AccessList<,>))
							.MakeGenericInstanceType(underlyingType, baseType);
						constructor = MethodUtils.MakeConstructorOnGenericType(SharedState.Instance.Importer, accessListSignature, 1);
					}
					break;
				case "AccessDictionaryBase`2":
					{
						GenericInstanceTypeSignature fieldType = field.Signature?.FieldType as GenericInstanceTypeSignature ?? throw new();
						TypeSignature keyNormalType = fieldType.TypeArguments[0];
						TypeSignature valueNormalType = fieldType.TypeArguments[1];
						TypeSignature keyBaseType = genericPropertyType.TypeArguments[0];
						TypeSignature valueBaseType = genericPropertyType.TypeArguments[1];
						GenericInstanceTypeSignature accessDictionarySignature = SharedState.Instance.Importer
							.ImportTypeSignature(typeof(AccessDictionary<,,,>))
							.MakeGenericInstanceType(keyNormalType, valueNormalType, keyBaseType, valueBaseType);
						constructor = MethodUtils.MakeConstructorOnGenericType(SharedState.Instance.Importer, accessDictionarySignature, 1);
					}
					break;
				case "AccessPairBase`2":
					//Pair only used by Sprite and it's AssetPair<GUID, long>
					throw new NotSupportedException("AccessPair not supported.");
				default:
					//Handles AssetList, AssetDictionary, and AssetPair
					return declaringType.ImplementGetterProperty(propertyName, InterfaceUtils.InterfacePropertyImplementation, propertyType, field);
			}
			PropertyDefinition property = declaringType.AddGetterProperty(propertyName, InterfaceUtils.InterfacePropertyImplementation, propertyType);
			FieldDefinition cacheField = declaringType
				.AddField($"_{propertyName}_k__BackingField", propertyType, visibility: Visibility.Private)
				.AddNullableAttributesForMaybeNull();
			cacheField.AddDebuggerBrowsableNeverAttribute();
			CilInstructionCollection instructions = property.GetMethod!.CilMethodBody!.Instructions;

			CilInstructionLabel afterAssignmentLabel = new();

			//if accessValue is null
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Ldfld, cacheField);
			instructions.Add(CilOpCodes.Ldnull);
			instructions.Add(CilOpCodes.Ceq);
			instructions.Add(CilOpCodes.Brfalse_S, afterAssignmentLabel);

			//accessValue = new(referenceValue);
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Ldfld, field);
			instructions.Add(CilOpCodes.Newobj, constructor);
			instructions.Add(CilOpCodes.Stfld, cacheField);

			//return accessValue
			afterAssignmentLabel.Instruction = instructions.Add(CilOpCodes.Nop);
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Ldfld, cacheField);
			instructions.Add(CilOpCodes.Ret);
			return property;
		}
		else
		{
			return declaringType.ImplementGetterProperty(propertyName, InterfaceUtils.InterfacePropertyImplementation, propertyType, field);
		}
	}

	/// <summary>
	/// Ensures that the <paramref name="field"/> is set to <see cref="Array.Empty{T}"/> instead of null.
	/// </summary>
	private static void FixNullableArraySetMethod(this PropertyDefinition property, FieldDefinition field)
	{
		TypeSignature elementType = ((SzArrayTypeSignature)field.Signature!.FieldType).BaseType;
		MethodSpecification emptyArrayMethod = SharedState.Instance.Importer
			.ImportMethod(typeof(Array), m => m.Name == nameof(Array.Empty))
			.MakeGenericInstanceMethod(elementType);

		CilInstructionCollection instructions = property.SetMethod!.CilMethodBody!.Instructions;
		instructions.Clear();
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldarg_1); //value

		CilInstructionLabel label = new();
		instructions.Add(CilOpCodes.Dup);
		instructions.Add(CilOpCodes.Brtrue, label);
		instructions.Add(CilOpCodes.Pop);
		instructions.Add(CilOpCodes.Call, emptyArrayMethod);

		label.Instruction = instructions.Add(CilOpCodes.Stfld, field);
		instructions.Add(CilOpCodes.Ret);
		instructions.OptimizeMacros();
	}

	private static IEnumerable<string> GetFieldNames(this UniversalClass universalClass)
	{
		return universalClass.EditorRootNode.GetFieldNames().Union(universalClass.ReleaseRootNode.GetFieldNames()).Distinct();
	}

	private static IEnumerable<string> GetFieldNames(this UniversalNode? rootNode)
	{
		return rootNode?.SubNodes.Select(node => node.Name) ?? Enumerable.Empty<string>();
	}

	private static string GetName(TypeSignature type)
	{
		if (type is CorLibTypeSignature)
		{
			return type.Name ?? throw new NullReferenceException();
		}
		else if (type is TypeDefOrRefSignature normalType)
		{
			string asmName = normalType.Name;
			int index = asmName.IndexOf('`');
			return index > -1 ? asmName.Substring(0, index) : asmName;
		}
		else if (type is SzArrayTypeSignature arrayType)
		{
			return $"{GetName(arrayType.BaseType)}_Array";
		}
		else if (type is GenericInstanceTypeSignature genericInstanceType)
		{
			string baseTypeName = GetName(genericInstanceType.GenericType.ToTypeSignature());
			StringBuilder sb = new();
			sb.Append(baseTypeName);
			foreach (TypeSignature typeArgument in genericInstanceType.TypeArguments)
			{
				sb.Append('_');
				sb.Append(GetName(typeArgument));
			}
			return sb.ToString();
		}
		else
		{
			throw new NotSupportedException($"GetName not support for {type.FullName} of type {type.GetType()}");
		}
	}

	private static bool TryCast<T, TCast>(this List<T> originalList, [NotNullWhen(true)] out List<TCast>? castedList)
	{
		castedList = new List<TCast>(originalList.Count);
		foreach (T element in originalList)
		{
			if (element is TCast castedElement)
			{
				castedList.Add(castedElement);
			}
			else
			{
				castedList = null;
				return false;
			}
		}
		return true;
	}
}
