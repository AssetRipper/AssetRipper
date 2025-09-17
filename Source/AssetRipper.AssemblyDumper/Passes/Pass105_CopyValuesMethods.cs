using AssetRipper.AssemblyDumper.InjectedTypes;
using AssetRipper.AssemblyDumper.Methods;
using AssetRipper.AssemblyDumper.Types;
using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.Metadata;
using System.Diagnostics;

namespace AssetRipper.AssemblyDumper.Passes;

internal static partial class Pass105_CopyValuesMethods
{
	private const string CopyValuesName = nameof(IUnityAssetBase.CopyValues);
	private const string DeepCloneName = "DeepClone";
	private static readonly Dictionary<TypeSignatureStruct, (IMethodDescriptor, CopyMethodType)> singleTypeDictionary = new();
	private static readonly Dictionary<(TypeSignatureStruct, TypeSignatureStruct), (IMethodDescriptor, CopyMethodType)> doubleTypeDictionary = new();
	private static readonly HashSet<ClassGroupBase> processedGroups = new();

#nullable disable
	private static TypeSignature pptrCommonType;
	private static IMethodDefOrRef pptrCommonGetFileIDMethod;
	private static IMethodDefOrRef pptrCommonGetPathIDMethod;

	private static IMethodDefOrRef ipptrGetFileIDMethod;
	private static IMethodDefOrRef ipptrGetPathIDMethod;

	private static ITypeDefOrRef pptrConverterType;
	private static IMethodDefOrRef pptrConverterGetSourceCollectionMethod;
	private static IMethodDefOrRef pptrConverterGetTargetCollectionMethod;

	private static TypeDefinition helperType;
	private static MethodDefinition duplicateArrayMethod;
	private static MethodDefinition duplicateArrayArrayMethod;
	private static IMethodDefOrRef pptrConvertMethod;

	private static ITypeDefOrRef accessPairBase;
	private static IMethodDefOrRef accessPairBaseGetKey;
	private static IMethodDefOrRef accessPairBaseSetKey;
	private static IMethodDefOrRef accessPairBaseGetValue;
	private static IMethodDefOrRef accessPairBaseSetValue;

	private static ITypeDefOrRef accessListBase;
	private static IMethodDefOrRef accessListBaseGetCount;
	private static IMethodDefOrRef accessListBaseSetCapacity;
	private static IMethodDefOrRef accessListBaseGetItem;

	private static ITypeDefOrRef accessDictionaryBase;
	private static IMethodDefOrRef accessDictionaryBaseGetCount;
	private static IMethodDefOrRef accessDictionaryBaseSetCapacity;
	private static IMethodDefOrRef accessDictionaryBaseGetPair;

	private static ITypeDefOrRef assetList;
	private static IMethodDefOrRef assetListAdd;
	private static IMethodDefOrRef assetListAddNew;
	private static IMethodDefOrRef assetListClear;

	private static ITypeDefOrRef assetDictionary;
	private static IMethodDefOrRef assetDictionaryAddNew;
	private static IMethodDefOrRef assetDictionaryClear;

	private static ITypeDefOrRef assetPair;
#nullable enable

	public static void DoPass()
	{
		pptrCommonType = SharedState.Instance.Importer.ImportType<PPtr>().ToTypeSignature();
		pptrCommonGetFileIDMethod = SharedState.Instance.Importer.ImportMethod<PPtr>(m => m.Name == $"get_{nameof(PPtr.FileID)}");
		pptrCommonGetPathIDMethod = SharedState.Instance.Importer.ImportMethod<PPtr>(m => m.Name == $"get_{nameof(PPtr.PathID)}");

		ipptrGetFileIDMethod = SharedState.Instance.Importer.ImportMethod<IPPtr>(m => m.Name == $"get_{nameof(IPPtr.FileID)}");
		ipptrGetPathIDMethod = SharedState.Instance.Importer.ImportMethod<IPPtr>(m => m.Name == $"get_{nameof(IPPtr.PathID)}");

		pptrConverterType = SharedState.Instance.Importer.ImportType<PPtrConverter>();
		pptrConverterGetSourceCollectionMethod = SharedState.Instance.Importer.ImportMethod<PPtrConverter>(m => m.Name == "get_" + nameof(PPtrConverter.SourceCollection));
		pptrConverterGetTargetCollectionMethod = SharedState.Instance.Importer.ImportMethod<PPtrConverter>(m => m.Name == "get_" + nameof(PPtrConverter.TargetCollection));
		helperType = InjectHelper();

		accessPairBase = SharedState.Instance.Importer.ImportType(typeof(AccessPairBase<,>));
		accessPairBaseGetKey = SharedState.Instance.Importer.ImportMethod(typeof(AccessPairBase<,>), m => m.Name == $"get_{nameof(AccessPairBase<int, int>.Key)}");
		accessPairBaseSetKey = SharedState.Instance.Importer.ImportMethod(typeof(AccessPairBase<,>), m => m.Name == $"set_{nameof(AccessPairBase<int, int>.Key)}");
		accessPairBaseGetValue = SharedState.Instance.Importer.ImportMethod(typeof(AccessPairBase<,>), m => m.Name == $"get_{nameof(AccessPairBase<int, int>.Value)}");
		accessPairBaseSetValue = SharedState.Instance.Importer.ImportMethod(typeof(AccessPairBase<,>), m => m.Name == $"set_{nameof(AccessPairBase<int, int>.Value)}");

		accessListBase = SharedState.Instance.Importer.ImportType(typeof(AccessListBase<>));
		accessListBaseGetCount = SharedState.Instance.Importer.ImportMethod(typeof(AccessListBase<>), m => m.Name == $"get_{nameof(AccessListBase<int>.Count)}");
		accessListBaseSetCapacity = SharedState.Instance.Importer.ImportMethod(typeof(AccessListBase<>), m => m.Name == $"set_{nameof(AccessListBase<int>.Capacity)}");
		accessListBaseGetItem = SharedState.Instance.Importer.ImportMethod(typeof(AccessListBase<>), m => m.Name == "get_Item");

		accessDictionaryBase = SharedState.Instance.Importer.ImportType(typeof(AccessDictionaryBase<,>));
		accessDictionaryBaseGetCount = SharedState.Instance.Importer.ImportMethod(typeof(AccessDictionaryBase<,>), m => m.Name == $"get_{nameof(AccessDictionaryBase<int, int>.Count)}");
		accessDictionaryBaseSetCapacity = SharedState.Instance.Importer.ImportMethod(typeof(AccessDictionaryBase<,>), m => m.Name == $"set_{nameof(AccessDictionaryBase<int, int>.Capacity)}");
		accessDictionaryBaseGetPair = SharedState.Instance.Importer.ImportMethod(typeof(AccessDictionaryBase<,>), m => m.Name == nameof(AccessDictionaryBase<int, int>.GetPair));

		assetList = SharedState.Instance.Importer.ImportType(typeof(AssetList<>));
		assetListAdd = SharedState.Instance.Importer.ImportMethod(typeof(AssetList<>), m => m.Name == nameof(AssetList<int>.Add));
		assetListAddNew = SharedState.Instance.Importer.ImportMethod(typeof(AssetList<>), m => m.Name == nameof(AssetList<int>.AddNew));
		assetListClear = SharedState.Instance.Importer.ImportMethod(typeof(AssetList<>), m => m.Name == nameof(AssetList<int>.Clear));

		assetDictionary = SharedState.Instance.Importer.ImportType(typeof(AssetDictionary<,>));
		assetDictionaryAddNew = SharedState.Instance.Importer.ImportMethod(typeof(AssetDictionary<,>), m => m.Name == nameof(AssetDictionary<int, int>.AddNew));
		assetDictionaryClear = SharedState.Instance.Importer.ImportMethod(typeof(AssetDictionary<,>), m => m.Name == nameof(AssetDictionary<int, int>.Clear));

		assetPair = SharedState.Instance.Importer.ImportType(typeof(AssetPair<,>));

		foreach (ClassGroupBase group in SharedState.Instance.AllGroups)
		{
			EnsureGroupProcessed(group);
		}


		foreach (SubclassGroup group in SharedState.Instance.SubclassGroups.Values)
		{
			bool needsConverter = GetPrimaryCopyValuesMethod(group.Interface).Parameters.Count == 2;
			{
				MethodDefinition method = group.Interface.AddMethod(DeepCloneName, InterfaceUtils.InterfaceMethodDeclaration, group.Interface.ToTypeSignature());
				if (needsConverter)
				{
					method.AddParameter(pptrConverterType.ToTypeSignature(), "converter");
				}
			}
			foreach (TypeDefinition type in group.Types)
			{
				MethodDefinition copyValuesMethod = GetPrimaryCopyValuesMethod(type);
				MethodDefinition method = type.AddMethod(DeepCloneName, InterfaceUtils.InterfaceMethodImplementation, group.Interface.ToTypeSignature());
				CilInstructionCollection instructions = method.GetInstructions();
				instructions.Add(CilOpCodes.Newobj, type.GetDefaultConstructor());
				instructions.Add(CilOpCodes.Dup);
				instructions.Add(CilOpCodes.Ldarg_0);
				if (needsConverter)
				{
					method.AddParameter(pptrConverterType.ToTypeSignature(), "converter");
					instructions.Add(CilOpCodes.Ldarg_1);
				}
				instructions.Add(CilOpCodes.Call, copyValuesMethod);
				instructions.Add(CilOpCodes.Ret);
			}
		}


		IMethodDefOrRef pptrConverterConstructor = SharedState.Instance.Importer.ImportMethod<PPtrConverter>(m =>
		{
			return m.IsConstructor && m.Parameters.Count == 2 && m.Parameters[0].ParameterType.Name == nameof(IUnityObjectBase);
		});
		foreach (ClassGroup group in SharedState.Instance.ClassGroups.Values)
		{
			if (GetPrimaryCopyValuesMethod(group.Interface).Parameters.Count == 2)//Has converter
			{
				{
					MethodDefinition method = group.Interface.AddMethod(CopyValuesName, InterfaceUtils.InterfaceMethodDeclaration, SharedState.Instance.Importer.Void);
					method.AddParameter(group.Interface.ToTypeSignature(), "source");
				}
				foreach (TypeDefinition type in group.Types)
				{
					MethodDefinition originalCopyValuesMethod = GetPrimaryCopyValuesMethod(type);
					MethodDefinition method = type.AddMethod(CopyValuesName, InterfaceUtils.InterfaceMethodImplementation, SharedState.Instance.Importer.Void);
					method.AddParameter(group.Interface.ToTypeSignature(), "source");
					CilInstructionCollection instructions = method.GetInstructions();

					instructions.Add(CilOpCodes.Ldarg_0);
					instructions.Add(CilOpCodes.Ldarg_1);
					instructions.Add(CilOpCodes.Ldarg_1);
					instructions.Add(CilOpCodes.Ldarg_0);
					instructions.Add(CilOpCodes.Newobj, pptrConverterConstructor);
					instructions.Add(CilOpCodes.Call, originalCopyValuesMethod);

					instructions.Add(CilOpCodes.Ret);
				}
			}
		}

		TypeSignature unityAssetBaseInterfaceRef = SharedState.Instance.Importer.ImportTypeSignature<IUnityAssetBase>();
		Dictionary<TypeDefinition, MethodDefinition> overridenMethods = new();
		foreach (ClassGroupBase group in SharedState.Instance.AllGroups)
		{
			foreach (TypeDefinition type in group.Types)
			{
				MethodDefinition copyValuesMethod = type.AddMethod(
					nameof(UnityAssetBase.CopyValues),
					Pass063_CreateEmptyMethods.OverrideMethodAttributes,
					SharedState.Instance.Importer.Void);
				copyValuesMethod.AddParameter(unityAssetBaseInterfaceRef, "source");
				copyValuesMethod.AddParameter(pptrConverterType.ToTypeSignature(), "converter");
				copyValuesMethod.AddNullableContextAttribute(NullableAnnotation.MaybeNull);
				overridenMethods.Add(type, copyValuesMethod);
			}
		}
		foreach (ClassGroupBase group in SharedState.Instance.AllGroups)
		{
			foreach (GeneratedClassInstance instance in group.Instances)
			{
				MethodDefinition primaryMethod = GetPrimaryCopyValuesMethod(instance.Type);
				MethodDefinition thisMethod = overridenMethods[instance.Type];
				MethodDefinition? baseMethod = instance.Base is null ? null : overridenMethods[instance.Base.Type];
				CilInstructionCollection instructions = thisMethod.GetInstructions();

				if (group is SubclassGroup)//Optimization for subclasses since 2 null checks is unnecessary
				{
					instructions.Add(CilOpCodes.Ldarg_0);
					instructions.Add(CilOpCodes.Ldarg_1);
					instructions.Add(CilOpCodes.Isinst, group.Interface);
					if (primaryMethod.Parameters.Count == 2)
					{
						instructions.Add(CilOpCodes.Ldarg_2);//Converter is needed
					}
					instructions.Add(CilOpCodes.Callvirt, primaryMethod);
					instructions.Add(CilOpCodes.Ret);
				}
				else
				{
					CilInstructionLabel returnLabel = new();
					CilInstructionLabel isNullLabel = new();
					CilLocalVariable castedArgumentLocal = instructions.AddLocalVariable(group.Interface.ToTypeSignature());

					instructions.Add(CilOpCodes.Ldarg_1);
					instructions.Add(CilOpCodes.Isinst, group.Interface);
					instructions.Add(CilOpCodes.Stloc, castedArgumentLocal);

					instructions.Add(CilOpCodes.Ldloc, castedArgumentLocal);
					instructions.Add(CilOpCodes.Ldnull);
					instructions.Add(CilOpCodes.Cgt_Un);
					instructions.Add(CilOpCodes.Brfalse, isNullLabel);

					instructions.Add(CilOpCodes.Ldarg_0);
					instructions.Add(CilOpCodes.Ldloc, castedArgumentLocal);
					if (primaryMethod.Parameters.Count == 2)
					{
						instructions.Add(CilOpCodes.Ldarg_2);//Converter is needed
					}
					instructions.Add(CilOpCodes.Callvirt, primaryMethod);
					instructions.Add(CilOpCodes.Br, returnLabel);

					isNullLabel.Instruction = instructions.Add(CilOpCodes.Nop);

					if (baseMethod is null)//Object
					{
						instructions.Add(CilOpCodes.Ldarg_0);
						instructions.Add(CilOpCodes.Callvirt, instance.Type.GetMethodByName(nameof(IUnityAssetBase.Reset)));
					}
					else
					{
						instructions.Add(CilOpCodes.Ldarg_0);
						instructions.Add(CilOpCodes.Ldarg_1);
						instructions.Add(CilOpCodes.Ldarg_2);
						instructions.Add(CilOpCodes.Call, baseMethod);
					}

					returnLabel.Instruction = instructions.Add(CilOpCodes.Ret);
				}
			}
		}

		singleTypeDictionary.Clear();
		doubleTypeDictionary.Clear();
		processedGroups.Clear();
		overridenMethods.Clear();
	}

	[MemberNotNull(nameof(duplicateArrayMethod))]
	[MemberNotNull(nameof(duplicateArrayArrayMethod))]
	[MemberNotNull(nameof(pptrConvertMethod))]
	private static TypeDefinition InjectHelper()
	{
		TypeDefinition clonedType = SharedState.Instance.InjectHelperType(typeof(CopyValuesHelper));
		duplicateArrayMethod = clonedType.GetMethodByName(nameof(CopyValuesHelper.DuplicateArray));
		duplicateArrayArrayMethod = clonedType.GetMethodByName(nameof(CopyValuesHelper.DuplicateArrayArray));
		pptrConvertMethod = clonedType.GetMethodByName(nameof(CopyValuesHelper.ConvertPPtr));
		return clonedType;
	}

	private static void EnsureGroupProcessed(ClassGroupBase group)
	{
		if (!processedGroups.Add(group))
		{
			return;
		}

		if (group.IsPPtr)
		{
			{
				MethodDefinition method = group.Interface.AddMethod(CopyValuesName, InterfaceUtils.InterfaceMethodDeclaration, SharedState.Instance.Importer.Void);
				method.AddParameter(group.Interface.ToTypeSignature(), "source");
				method.AddParameter(pptrConverterType.ToTypeSignature(), "converter");
				method.AddNullableContextAttribute(NullableAnnotation.MaybeNull);
				singleTypeDictionary.Add(group.Interface.ToTypeSignature(), (method, CopyMethodType.Callvirt | CopyMethodType.HasConverter));
			}
			foreach (TypeDefinition type in group.Types)
			{
				MethodDefinition method = type.AddMethod(CopyValuesName, InterfaceUtils.InterfaceMethodImplementation, SharedState.Instance.Importer.Void);
				method.AddParameter(group.Interface.ToTypeSignature(), "source");
				Parameter converterParam = method.AddParameter(pptrConverterType.ToTypeSignature(), "converter");
				method.AddNullableContextAttribute(NullableAnnotation.MaybeNull);
				CilInstructionCollection instructions = method.GetInstructions();
				CilInstructionLabel returnLabel = new();
				CilInstructionLabel isNullLabel = new();
				CilInstructionLabel isSameCollectionLabel = new();

				//If other is null
				instructions.Add(CilOpCodes.Ldarg_1);
				instructions.Add(CilOpCodes.Ldnull);
				instructions.Add(CilOpCodes.Cgt_Un);
				instructions.Add(CilOpCodes.Brfalse, isNullLabel);

				//If source collection == target collection
				instructions.Add(CilOpCodes.Ldarga, converterParam);
				instructions.Add(CilOpCodes.Call, pptrConverterGetSourceCollectionMethod);
				instructions.Add(CilOpCodes.Ldarga, converterParam);
				instructions.Add(CilOpCodes.Call, pptrConverterGetTargetCollectionMethod);
				instructions.Add(CilOpCodes.Ceq);
				instructions.Add(CilOpCodes.Brtrue, isSameCollectionLabel);

				//Not same collection
				{
					//Convert PPtr
					CilLocalVariable convertedPPtr = instructions.AddLocalVariable(pptrCommonType);
					instructions.Add(CilOpCodes.Ldarg_1);
					instructions.Add(CilOpCodes.Ldarg_2);
					instructions.Add(CilOpCodes.Call, pptrConvertMethod.MakeGenericInstanceMethod(GetPPtrTypeArgument(type, group.Interface)));
					instructions.Add(CilOpCodes.Stloc, convertedPPtr);

					//Store FileID
					instructions.Add(CilOpCodes.Ldarg_0);
					instructions.Add(CilOpCodes.Ldloca, convertedPPtr);
					instructions.Add(CilOpCodes.Call, pptrCommonGetFileIDMethod);
					instructions.Add(CilOpCodes.Stfld, type.GetFieldByName("m_FileID_"));

					//Store PathID
					instructions.Add(CilOpCodes.Ldarg_0);
					instructions.Add(CilOpCodes.Ldloca, convertedPPtr);
					instructions.Add(CilOpCodes.Call, pptrCommonGetPathIDMethod);
					FieldDefinition pathIDField = type.GetFieldByName("m_PathID_");
					if (pathIDField.Signature!.FieldType is CorLibTypeSignature { ElementType: ElementType.I4 })
					{
						instructions.Add(CilOpCodes.Conv_Ovf_I4);//Convert I8 to I4
					}
					instructions.Add(CilOpCodes.Stfld, pathIDField);

					instructions.Add(CilOpCodes.Br, returnLabel);
				}

				//Same collection
				{
					isSameCollectionLabel.Instruction = instructions.Add(CilOpCodes.Nop);

					//Store FileID
					instructions.Add(CilOpCodes.Ldarg_0);
					instructions.Add(CilOpCodes.Ldarg_1);
					instructions.Add(CilOpCodes.Callvirt, ipptrGetFileIDMethod);
					instructions.Add(CilOpCodes.Stfld, type.GetFieldByName("m_FileID_"));

					//Store PathID
					instructions.Add(CilOpCodes.Ldarg_0);
					instructions.Add(CilOpCodes.Ldarg_1);
					instructions.Add(CilOpCodes.Callvirt, ipptrGetPathIDMethod);
					FieldDefinition pathIDField = type.GetFieldByName("m_PathID_");
					if (pathIDField.Signature!.FieldType is CorLibTypeSignature { ElementType: ElementType.I4 })
					{
						instructions.Add(CilOpCodes.Conv_Ovf_I4);//Convert I8 to I4
					}
					instructions.Add(CilOpCodes.Stfld, pathIDField);

					instructions.Add(CilOpCodes.Br, returnLabel);
				}

				isNullLabel.Instruction = instructions.Add(CilOpCodes.Nop);
				instructions.Add(CilOpCodes.Ldarg_0);
				instructions.Add(CilOpCodes.Callvirt, type.GetMethodByName(nameof(IUnityAssetBase.Reset)));

				returnLabel.Instruction = instructions.Add(CilOpCodes.Ret);
				singleTypeDictionary.Add(type.ToTypeSignature(), (method, CopyMethodType.HasConverter));
			}
		}
		else
		{
			bool needsConverter = false;
			bool needsNullCheck = group is SubclassGroup;
			Dictionary<TypeDefinition, MethodDefinition> instanceMethods = new();
			foreach (GeneratedClassInstance instance in group.Instances)
			{
				MethodDefinition method = instance.Type.AddMethod(CopyValuesName, InterfaceUtils.InterfaceMethodImplementation, SharedState.Instance.Importer.Void);
				method.AddParameter(group.Interface.ToTypeSignature(), "source");
				CilInstructionCollection instructions = method.GetInstructions();
				CilInstructionLabel returnLabel = new();
				CilInstructionLabel isNullLabel = new();
				if (needsNullCheck)
				{
					method.AddNullableContextAttribute(NullableAnnotation.MaybeNull);
					instructions.Add(CilOpCodes.Ldarg_1);
					instructions.Add(CilOpCodes.Ldnull);
					instructions.Add(CilOpCodes.Cgt_Un);
					instructions.Add(CilOpCodes.Brfalse, isNullLabel);
				}

				foreach (ClassProperty classProperty in instance.Properties)
				{
					if (classProperty.BackingField is not null)
					{
						TypeSignature fieldTypeSignature = classProperty.BackingField.Signature!.FieldType;
						if (fieldTypeSignature is CorLibTypeSignature)
						{
							instructions.Add(CilOpCodes.Ldarg_0);
							instructions.Add(CilOpCodes.Ldarg_1);
							instructions.Add(CilOpCodes.Callvirt, classProperty.Base.Definition.GetMethod!);
							instructions.Add(CilOpCodes.Stfld, classProperty.BackingField);
						}
						else if (fieldTypeSignature is TypeDefOrRefSignature { Namespace: "AssetRipper.Primitives", Name: nameof(Utf8String) })
						{
							// m_Field = source.Property ?? Utf8String.Empty;
							CilInstructionLabel stfldLabel = new();
							instructions.Add(CilOpCodes.Ldarg_0);
							instructions.Add(CilOpCodes.Ldarg_1);
							instructions.Add(CilOpCodes.Callvirt, classProperty.Base.Definition.GetMethod!);
							instructions.Add(CilOpCodes.Dup);
							instructions.Add(CilOpCodes.Brtrue, stfldLabel);
							instructions.Add(CilOpCodes.Pop);
							instructions.Add(CilOpCodes.Call, new MemberReference(fieldTypeSignature.ToTypeDefOrRef(), "get_Empty", MethodSignature.CreateStatic(fieldTypeSignature)));
							stfldLabel.Instruction = instructions.Add(CilOpCodes.Stfld, classProperty.BackingField);
						}
						else if (fieldTypeSignature is SzArrayTypeSignature arrayTypeSignature)
						{
							instructions.Add(CilOpCodes.Ldarg_0);
							instructions.Add(CilOpCodes.Ldarg_1);
							instructions.Add(CilOpCodes.Callvirt, classProperty.Base.Definition.GetMethod!);
							instructions.Add(CilOpCodes.Call, MakeDuplicateArrayMethod(arrayTypeSignature));
							instructions.Add(CilOpCodes.Stfld, classProperty.BackingField);
						}
						else
						{
							(IMethodDescriptor fieldCopyMethod, CopyMethodType copyMethodType) = GetOrMakeMethod(
								fieldTypeSignature,
								classProperty.Base.Definition.Signature!.ReturnType);
							instructions.Add(CilOpCodes.Ldarg_0);
							instructions.Add(CilOpCodes.Ldfld, classProperty.BackingField);
							instructions.Add(CilOpCodes.Ldarg_1);
							instructions.Add(CilOpCodes.Callvirt, classProperty.Base.Definition.GetMethod!);
							if (HasConverter(copyMethodType))
							{
								instructions.Add(CilOpCodes.Ldarg_2);
								needsConverter = true;
							}
							instructions.Add(GetCallOpCode(copyMethodType), fieldCopyMethod);
						}
					}
				}

				if (needsNullCheck)
				{
					instructions.Add(CilOpCodes.Br, returnLabel);

					isNullLabel.Instruction = instructions.Add(CilOpCodes.Nop);
					instructions.Add(CilOpCodes.Ldarg_0);
					instructions.Add(CilOpCodes.Callvirt, instance.Type.GetMethodByName(nameof(IUnityAssetBase.Reset)));
				}

				returnLabel.Instruction = instructions.Add(CilOpCodes.Ret);
				instanceMethods.Add(instance.Type, method);
			}

			foreach ((TypeDefinition type, MethodDefinition method) in instanceMethods)
			{
				if (needsConverter)
				{
					method.AddParameter(pptrConverterType.ToTypeSignature(), "converter");
					singleTypeDictionary.Add(type.ToTypeSignature(), (method, CopyMethodType.HasConverter));
				}
				else
				{
					singleTypeDictionary.Add(type.ToTypeSignature(), (method, CopyMethodType.None));
				}
			}

			{
				MethodDefinition method = group.Interface.AddMethod(CopyValuesName, InterfaceUtils.InterfaceMethodDeclaration, SharedState.Instance.Importer.Void);
				method.AddParameter(group.Interface.ToTypeSignature(), "source");
				if (needsNullCheck)
				{
					method.AddNullableContextAttribute(NullableAnnotation.MaybeNull);
				}
				if (needsConverter)
				{
					method.AddParameter(pptrConverterType.ToTypeSignature(), "converter");
					singleTypeDictionary.Add(group.Interface.ToTypeSignature(), (method, CopyMethodType.Callvirt | CopyMethodType.HasConverter));
				}
				else
				{
					singleTypeDictionary.Add(group.Interface.ToTypeSignature(), (method, CopyMethodType.Callvirt));
				}
			}
		}
	}

	private static (IMethodDescriptor, CopyMethodType) GetOrMakeMethod(TypeSignature targetSignature, TypeSignature sourceSignature)
	{
		if (singleTypeDictionary.TryGetValue(targetSignature, out (IMethodDescriptor, CopyMethodType) pair))
		{
			return pair;
		}
		else if (doubleTypeDictionary.TryGetValue((targetSignature, sourceSignature), out pair))
		{
			return pair;
		}

		switch (targetSignature)
		{
			case TypeDefOrRefSignature typeDefOrRefSignature:
				TypeDefinition type = (TypeDefinition)typeDefOrRefSignature.Type;
				EnsureGroupProcessed(SharedState.Instance.TypesToGroups[type]);
				return singleTypeDictionary[type.ToTypeSignature()];
			case GenericInstanceTypeSignature targetGenericSignature:
				{
					bool needsConverter = false;
					GenericInstanceTypeSignature sourceGenericSignature = (GenericInstanceTypeSignature)sourceSignature;
					MethodDefinition method = helperType.AddMethod(
						MakeUniqueCopyValuesName(targetSignature, sourceSignature),
						StaticClassCreator.StaticMethodAttributes,
						SharedState.Instance.Importer.Void);
					method.AddParameter(targetSignature, "target");
					method.AddParameter(sourceSignature, "source");
					CilInstructionCollection instructions = method.GetInstructions();
					switch (targetGenericSignature.GenericType.Name?.ToString())
					{
						case $"{nameof(AssetDictionary<int, int>)}`2":
							{
								//Argument 0 (target) is AssetDictionary`2. Argument 1 (source) is AccessDictionaryBase`2.

								TypeSignature targetKeyTypeSignature = targetGenericSignature.TypeArguments[0];
								TypeSignature targetValueTypeSignature = targetGenericSignature.TypeArguments[1];
								TypeSignature targetPairTypeSignature = assetPair.MakeGenericInstanceType(targetKeyTypeSignature, targetValueTypeSignature);
								TypeSignature sourceKeyTypeSignature = sourceGenericSignature.TypeArguments[0];
								TypeSignature sourceValueTypeSignature = sourceGenericSignature.TypeArguments[1];
								TypeSignature sourcePairTypeSignature = accessPairBase.MakeGenericInstanceType(sourceKeyTypeSignature, sourceValueTypeSignature);

								CilInstructionLabel returnLabel = new();
								CilInstructionLabel isNullLabel = new();

								instructions.Add(CilOpCodes.Ldarg_0);
								instructions.Add(CilOpCodes.Callvirt, MakeAssetDictionaryClearMethod(targetKeyTypeSignature, targetValueTypeSignature));

								instructions.Add(CilOpCodes.Ldarg_1);
								instructions.Add(CilOpCodes.Ldnull);
								instructions.Add(CilOpCodes.Cgt_Un);
								instructions.Add(CilOpCodes.Brfalse, isNullLabel);

								CilLocalVariable countLocal = instructions.AddLocalVariable(SharedState.Instance.Importer.Int32);
								instructions.Add(CilOpCodes.Ldarg_1);
								instructions.Add(CilOpCodes.Callvirt, MakeDictionaryGetCountMethod(sourceKeyTypeSignature, sourceValueTypeSignature));
								instructions.Add(CilOpCodes.Stloc, countLocal);

								instructions.Add(CilOpCodes.Ldarg_0);
								instructions.Add(CilOpCodes.Ldloc, countLocal);
								instructions.Add(CilOpCodes.Callvirt, MakeDictionarySetCapacityMethod(targetKeyTypeSignature, targetValueTypeSignature));

								CilLocalVariable iLocal = instructions.AddLocalVariable(SharedState.Instance.Importer.Int32);
								instructions.Add(CilOpCodes.Ldc_I4_0);
								instructions.Add(CilOpCodes.Stloc, iLocal);

								CilInstructionLabel conditionLabel = new();
								instructions.Add(CilOpCodes.Br, conditionLabel);

								CilInstructionLabel forStartLabel = new();
								forStartLabel.Instruction = instructions.Add(CilOpCodes.Nop);

								(IMethodDescriptor copyMethod, CopyMethodType copyMethodType) = GetOrMakeMethod(targetPairTypeSignature, sourcePairTypeSignature);
								instructions.Add(CilOpCodes.Ldarg_0);
								instructions.Add(CilOpCodes.Callvirt, MakeAssetDictionaryAddNewMethod(targetKeyTypeSignature, targetValueTypeSignature));
								instructions.Add(CilOpCodes.Ldarg_1);
								instructions.Add(CilOpCodes.Ldloc, iLocal);
								instructions.Add(CilOpCodes.Callvirt, MakeDictionaryGetPairMethod(sourceKeyTypeSignature, sourceValueTypeSignature));
								if (HasConverter(copyMethodType))
								{
									instructions.Add(CilOpCodes.Ldarg_2);
									needsConverter = true;
								}
								instructions.Add(GetCallOpCode(copyMethodType), copyMethod);

								instructions.Add(CilOpCodes.Ldloc, iLocal);
								instructions.Add(CilOpCodes.Ldc_I4_1);
								instructions.Add(CilOpCodes.Add);
								instructions.Add(CilOpCodes.Stloc, iLocal);

								conditionLabel.Instruction = instructions.Add(CilOpCodes.Nop);
								instructions.Add(CilOpCodes.Ldloc, iLocal);
								instructions.Add(CilOpCodes.Ldloc, countLocal);
								instructions.Add(CilOpCodes.Clt);
								instructions.Add(CilOpCodes.Brtrue, forStartLabel);

								instructions.Add(CilOpCodes.Br, returnLabel);

								isNullLabel.Instruction = instructions.Add(CilOpCodes.Nop);
								instructions.Add(CilOpCodes.Ldarg_0);
								instructions.Add(CilOpCodes.Ldc_I4_0);
								instructions.Add(CilOpCodes.Callvirt, MakeDictionarySetCapacityMethod(targetKeyTypeSignature, targetValueTypeSignature));

								returnLabel.Instruction = instructions.Add(CilOpCodes.Ret);
							}
							break;
						case $"{nameof(AssetList<int>)}`1":
							{
								//Argument 0 (target) is AssetList`1. Argument 1 (source) is AccessListBase`1.

								TypeSignature targetElementTypeSignature = targetGenericSignature.TypeArguments[0];
								TypeSignature sourceElementTypeSignature = sourceGenericSignature.TypeArguments[0];

								CilInstructionLabel returnLabel = new();
								CilInstructionLabel isNullLabel = new();

								instructions.Add(CilOpCodes.Ldarg_0);
								instructions.Add(CilOpCodes.Callvirt, MakeAssetListClearMethod(targetElementTypeSignature));

								instructions.Add(CilOpCodes.Ldarg_1);
								instructions.Add(CilOpCodes.Ldnull);
								instructions.Add(CilOpCodes.Cgt_Un);
								instructions.Add(CilOpCodes.Brfalse, isNullLabel);

								CilLocalVariable countLocal = instructions.AddLocalVariable(SharedState.Instance.Importer.Int32);
								instructions.Add(CilOpCodes.Ldarg_1);
								instructions.Add(CilOpCodes.Callvirt, MakeListGetCountMethod(sourceElementTypeSignature));
								instructions.Add(CilOpCodes.Stloc, countLocal);

								instructions.Add(CilOpCodes.Ldarg_0);
								instructions.Add(CilOpCodes.Ldloc, countLocal);
								instructions.Add(CilOpCodes.Callvirt, MakeListSetCapacityMethod(targetElementTypeSignature));

								CilLocalVariable iLocal = instructions.AddLocalVariable(SharedState.Instance.Importer.Int32);
								instructions.Add(CilOpCodes.Ldc_I4_0);
								instructions.Add(CilOpCodes.Stloc, iLocal);

								CilInstructionLabel conditionLabel = new();
								instructions.Add(CilOpCodes.Br, conditionLabel);

								CilInstructionLabel forStartLabel = new();
								forStartLabel.Instruction = instructions.Add(CilOpCodes.Nop);

								if (targetElementTypeSignature is CorLibTypeSignature or TypeDefOrRefSignature { Namespace: "AssetRipper.Primitives", Name: nameof(Utf8String) })
								{
									instructions.Add(CilOpCodes.Ldarg_0);
									instructions.Add(CilOpCodes.Ldarg_1);
									instructions.Add(CilOpCodes.Ldloc, iLocal);
									instructions.Add(CilOpCodes.Callvirt, MakeListGetItemMethod(sourceElementTypeSignature));
									instructions.Add(CilOpCodes.Callvirt, MakeAssetListAddMethod(targetElementTypeSignature));
								}
								else if (targetElementTypeSignature is SzArrayTypeSignature keyArrayTypeSignature)
								{
									throw new NotSupportedException();
								}
								else
								{
									(IMethodDescriptor copyMethod, CopyMethodType copyMethodType) = GetOrMakeMethod(targetElementTypeSignature, sourceElementTypeSignature);

									instructions.Add(CilOpCodes.Ldarg_0);
									instructions.Add(CilOpCodes.Callvirt, MakeAssetListAddNewMethod(targetElementTypeSignature));
									instructions.Add(CilOpCodes.Ldarg_1);
									instructions.Add(CilOpCodes.Ldloc, iLocal);
									instructions.Add(CilOpCodes.Callvirt, MakeListGetItemMethod(sourceElementTypeSignature));
									if (HasConverter(copyMethodType))
									{
										instructions.Add(CilOpCodes.Ldarg_2);
										needsConverter = true;
									}
									instructions.Add(GetCallOpCode(copyMethodType), copyMethod);
								}

								instructions.Add(CilOpCodes.Ldloc, iLocal);
								instructions.Add(CilOpCodes.Ldc_I4_1);
								instructions.Add(CilOpCodes.Add);
								instructions.Add(CilOpCodes.Stloc, iLocal);

								conditionLabel.Instruction = instructions.Add(CilOpCodes.Nop);
								instructions.Add(CilOpCodes.Ldloc, iLocal);
								instructions.Add(CilOpCodes.Ldloc, countLocal);
								instructions.Add(CilOpCodes.Clt);
								instructions.Add(CilOpCodes.Brtrue, forStartLabel);

								instructions.Add(CilOpCodes.Br, returnLabel);

								isNullLabel.Instruction = instructions.Add(CilOpCodes.Nop);
								instructions.Add(CilOpCodes.Ldarg_0);
								instructions.Add(CilOpCodes.Ldc_I4_0);
								instructions.Add(CilOpCodes.Callvirt, MakeListSetCapacityMethod(targetElementTypeSignature));

								returnLabel.Instruction = instructions.Add(CilOpCodes.Ret);
							}
							break;
						case $"{nameof(AssetPair<int, int>)}`2" or $"{nameof(AccessPairBase<int, int>)}`2":
							{
								TypeSignature targetKeyTypeSignature = targetGenericSignature.TypeArguments[0];
								TypeSignature sourceKeyTypeSignature = sourceGenericSignature.TypeArguments[0];
								TypeSignature targetValueTypeSignature = targetGenericSignature.TypeArguments[1];
								TypeSignature sourceValueTypeSignature = sourceGenericSignature.TypeArguments[1];

								CilInstructionLabel returnLabel = new();
								instructions.Add(CilOpCodes.Ldarg_1);
								instructions.Add(CilOpCodes.Ldnull);
								instructions.Add(CilOpCodes.Cgt_Un);
								instructions.Add(CilOpCodes.Brfalse, returnLabel);

								if (targetKeyTypeSignature is CorLibTypeSignature or TypeDefOrRefSignature { Namespace: "AssetRipper.Primitives", Name: nameof(Utf8String) })
								{
									instructions.Add(CilOpCodes.Ldarg_0);
									instructions.Add(CilOpCodes.Ldarg_1);
									instructions.Add(CilOpCodes.Callvirt, MakePairGetKeyMethod(sourceKeyTypeSignature, sourceValueTypeSignature));
									instructions.Add(CilOpCodes.Callvirt, MakePairSetKeyMethod(targetKeyTypeSignature, targetValueTypeSignature));
								}
								else if (targetKeyTypeSignature is SzArrayTypeSignature keyArrayTypeSignature)
								{
									instructions.Add(CilOpCodes.Ldarg_0);
									instructions.Add(CilOpCodes.Ldarg_1);
									instructions.Add(CilOpCodes.Callvirt, MakePairGetKeyMethod(sourceKeyTypeSignature, sourceValueTypeSignature));
									instructions.Add(CilOpCodes.Call, MakeDuplicateArrayMethod(keyArrayTypeSignature));
									instructions.Add(CilOpCodes.Callvirt, MakePairSetKeyMethod(targetKeyTypeSignature, targetValueTypeSignature));
								}
								else
								{
									(IMethodDescriptor keyCopyMethod, CopyMethodType keyCopyMethodType) = GetOrMakeMethod(targetKeyTypeSignature, sourceKeyTypeSignature);
									instructions.Add(CilOpCodes.Ldarg_0);
									instructions.Add(CilOpCodes.Callvirt, MakePairGetKeyMethod(targetKeyTypeSignature, targetValueTypeSignature));
									instructions.Add(CilOpCodes.Ldarg_1);
									instructions.Add(CilOpCodes.Callvirt, MakePairGetKeyMethod(sourceKeyTypeSignature, sourceValueTypeSignature));
									if (HasConverter(keyCopyMethodType))
									{
										instructions.Add(CilOpCodes.Ldarg_2);
										needsConverter = true;
									}
									instructions.Add(GetCallOpCode(keyCopyMethodType), keyCopyMethod);
								}

								if (targetValueTypeSignature is CorLibTypeSignature or TypeDefOrRefSignature { Namespace: "AssetRipper.Primitives", Name: nameof(Utf8String) })
								{
									instructions.Add(CilOpCodes.Ldarg_0);
									instructions.Add(CilOpCodes.Ldarg_1);
									instructions.Add(CilOpCodes.Callvirt, MakePairGetValueMethod(sourceKeyTypeSignature, sourceValueTypeSignature));
									instructions.Add(CilOpCodes.Callvirt, MakePairSetValueMethod(targetKeyTypeSignature, targetValueTypeSignature));
								}
								else if (targetValueTypeSignature is SzArrayTypeSignature valueArrayTypeSignature)
								{
									instructions.Add(CilOpCodes.Ldarg_0);
									instructions.Add(CilOpCodes.Ldarg_1);
									instructions.Add(CilOpCodes.Callvirt, MakePairGetValueMethod(sourceKeyTypeSignature, sourceValueTypeSignature));
									instructions.Add(CilOpCodes.Call, MakeDuplicateArrayMethod(valueArrayTypeSignature));
									instructions.Add(CilOpCodes.Callvirt, MakePairSetValueMethod(targetKeyTypeSignature, targetValueTypeSignature));
								}
								else
								{
									(IMethodDescriptor valueCopyMethod, CopyMethodType valueCopyMethodType) = GetOrMakeMethod(targetValueTypeSignature, sourceValueTypeSignature);
									instructions.Add(CilOpCodes.Ldarg_0);
									instructions.Add(CilOpCodes.Callvirt, MakePairGetValueMethod(targetKeyTypeSignature, targetValueTypeSignature));
									instructions.Add(CilOpCodes.Ldarg_1);
									instructions.Add(CilOpCodes.Callvirt, MakePairGetValueMethod(sourceKeyTypeSignature, sourceValueTypeSignature));
									if (HasConverter(valueCopyMethodType))
									{
										instructions.Add(CilOpCodes.Ldarg_2);
										needsConverter = true;
									}
									instructions.Add(GetCallOpCode(valueCopyMethodType), valueCopyMethod);
								}
								returnLabel.Instruction = instructions.Add(CilOpCodes.Ret);
							}
							break;
						default:
							throw new NotSupportedException();
					}
					(IMethodDescriptor, CopyMethodType) result;
					if (needsConverter)
					{
						method.AddParameter(pptrConverterType.ToTypeSignature(), "converter");
						result = (method, CopyMethodType.HasConverter);
					}
					else
					{
						result = (method, CopyMethodType.None);
					}
					doubleTypeDictionary.Add((targetSignature, sourceSignature), result);
					return result;
				}
			default:
				throw new NotSupportedException();
		}
	}

	private static MethodDefinition GetPrimaryCopyValuesMethod(this TypeDefinition type)
	{
		return (MethodDefinition)singleTypeDictionary[type.ToTypeSignature()].Item1;
	}

	private static IMethodDefOrRef MakeDictionaryGetCountMethod(TypeSignature keyTypeSignature, TypeSignature valueTypeSignature)
	{
		return MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			accessDictionaryBase.MakeGenericInstanceType(keyTypeSignature, valueTypeSignature),
			accessDictionaryBaseGetCount);
	}

	private static IMethodDefOrRef MakeDictionarySetCapacityMethod(TypeSignature keyTypeSignature, TypeSignature valueTypeSignature)
	{
		return MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			accessDictionaryBase.MakeGenericInstanceType(keyTypeSignature, valueTypeSignature),
			accessDictionaryBaseSetCapacity);
	}

	private static IMethodDefOrRef MakeDictionaryGetPairMethod(TypeSignature keyTypeSignature, TypeSignature valueTypeSignature)
	{
		return MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			accessDictionaryBase.MakeGenericInstanceType(keyTypeSignature, valueTypeSignature),
			accessDictionaryBaseGetPair);
	}

	private static IMethodDefOrRef MakeAssetDictionaryAddNewMethod(TypeSignature keyTypeSignature, TypeSignature valueTypeSignature)
	{
		return MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			assetDictionary.MakeGenericInstanceType(keyTypeSignature, valueTypeSignature),
			assetDictionaryAddNew);
	}

	private static IMethodDefOrRef MakeAssetDictionaryClearMethod(TypeSignature keyTypeSignature, TypeSignature valueTypeSignature)
	{
		return MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			assetDictionary.MakeGenericInstanceType(keyTypeSignature, valueTypeSignature),
			assetDictionaryClear);
	}

	private static IMethodDefOrRef MakeListGetCountMethod(TypeSignature elementTypeSignature)
	{
		return MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			accessListBase.MakeGenericInstanceType(elementTypeSignature),
			accessListBaseGetCount);
	}

	private static IMethodDefOrRef MakeListSetCapacityMethod(TypeSignature elementTypeSignature)
	{
		return MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			accessListBase.MakeGenericInstanceType(elementTypeSignature),
			accessListBaseSetCapacity);
	}

	private static IMethodDefOrRef MakeListGetItemMethod(TypeSignature elementTypeSignature)
	{
		return MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			accessListBase.MakeGenericInstanceType(elementTypeSignature),
			accessListBaseGetItem);
	}

	private static IMethodDefOrRef MakeAssetListAddNewMethod(TypeSignature elementTypeSignature)
	{
		return MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			assetList.MakeGenericInstanceType(elementTypeSignature),
			assetListAddNew);
	}

	private static IMethodDefOrRef MakeAssetListAddMethod(TypeSignature elementTypeSignature)
	{
		return MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			assetList.MakeGenericInstanceType(elementTypeSignature),
			assetListAdd);
	}

	private static IMethodDefOrRef MakeAssetListClearMethod(TypeSignature elementTypeSignature)
	{
		return MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			assetList.MakeGenericInstanceType(elementTypeSignature),
			assetListClear);
	}

	private static IMethodDefOrRef MakePairGetKeyMethod(TypeSignature keyTypeSignature, TypeSignature valueTypeSignature)
	{
		return MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			accessPairBase.MakeGenericInstanceType(keyTypeSignature, valueTypeSignature),
			accessPairBaseGetKey);
	}

	private static IMethodDefOrRef MakePairSetKeyMethod(TypeSignature keyTypeSignature, TypeSignature valueTypeSignature)
	{
		return MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			accessPairBase.MakeGenericInstanceType(keyTypeSignature, valueTypeSignature),
			accessPairBaseSetKey);
	}

	private static IMethodDefOrRef MakePairGetValueMethod(TypeSignature keyTypeSignature, TypeSignature valueTypeSignature)
	{
		return MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			accessPairBase.MakeGenericInstanceType(keyTypeSignature, valueTypeSignature),
			accessPairBaseGetValue);
	}

	private static IMethodDefOrRef MakePairSetValueMethod(TypeSignature keyTypeSignature, TypeSignature valueTypeSignature)
	{
		return MethodUtils.MakeMethodOnGenericType(
			SharedState.Instance.Importer,
			accessPairBase.MakeGenericInstanceType(keyTypeSignature, valueTypeSignature),
			accessPairBaseSetValue);
	}

	private static string MakeUniqueCopyValuesName(TypeSignature target, TypeSignature source)
	{
		return $"{CopyValuesName}__{UniqueNameFactory.MakeUniqueName(target)}__{UniqueNameFactory.MakeUniqueName(source)}";
	}

	private static IMethodDescriptor MakeDuplicateArrayMethod(SzArrayTypeSignature arrayTypeSignature)
	{
		TypeSignature elementType = arrayTypeSignature.BaseType;
		if (elementType is SzArrayTypeSignature nestedArray)
		{
			Debug.Assert(nestedArray.BaseType is CorLibTypeSignature or TypeDefOrRefSignature { Namespace: "AssetRipper.Primitives", Name: nameof(Utf8String) });
			return duplicateArrayArrayMethod.MakeGenericInstanceMethod(nestedArray.BaseType);
		}
		else
		{
			Debug.Assert(elementType is CorLibTypeSignature or TypeDefOrRefSignature { Namespace: "AssetRipper.Primitives", Name: nameof(Utf8String) });
			return duplicateArrayMethod.MakeGenericInstanceMethod(elementType);
		}
	}

	private static TypeSignature GetPPtrTypeArgument(TypeDefinition type, TypeDefinition groupInterface)
	{
		return TryGetPPtrTypeArgument(type)
			?? TryGetPPtrTypeArgument(groupInterface)
			?? throw new Exception("Could not get PPtr type argument.");
	}

	private static TypeSignature? TryGetPPtrTypeArgument(TypeDefinition type)
	{
		foreach (var implem in type.Interfaces)
		{
			if (implem.Interface is TypeSpecification specification
				&& specification.Signature is GenericInstanceTypeSignature genericInstanceTypeSignature
				&& genericInstanceTypeSignature.GenericType.Name == $"{nameof(IPPtr<IUnityObjectBase>)}`1")
			{
				return genericInstanceTypeSignature.TypeArguments[0];
			}
		}

		return null;
	}

	private static bool HasConverter(CopyMethodType copyMethodType)
	{
		return (copyMethodType & CopyMethodType.HasConverter) != 0;
	}

	private static CilOpCode GetCallOpCode(CopyMethodType copyMethodType)
	{
		return (copyMethodType & CopyMethodType.Callvirt) != 0 ? CilOpCodes.Callvirt : CilOpCodes.Call;
	}
}
