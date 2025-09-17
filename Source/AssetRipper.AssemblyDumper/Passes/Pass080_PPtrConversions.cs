using AssetRipper.AssemblyDumper.Attributes;
using AssetRipper.AssemblyDumper.InjectedTypes;
using AssetRipper.AssemblyDumper.Methods;
using AssetRipper.AssemblyDumper.Types;
using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass080_PPtrConversions
{
	public static IReadOnlyDictionary<TypeDefinition, TypeDefinition> PPtrsToParameters => pptrsToParameters;
	public static IReadOnlyDictionary<TypeDefinition, IMethodDescriptor> PPtrsToTryGetAssetMethods => pptrsToTryGetAssetMethods;
	public static IReadOnlyDictionary<TypeDefinition, IMethodDescriptor> PPtrsToSetAssetMethods => pptrsToSetAssetMethods;
#nullable disable
	private static ITypeDefOrRef pptrTypeImported;
	private static ITypeDefOrRef commonPPtrTypeGeneric;
	private static ITypeDefOrRef commonPPtrType;
	private static IMethodDefOrRef commonPPtrConstructor;
	private static IMethodDefOrRef commonPPtrGetFileIDMethod;
	private static IMethodDefOrRef commonPPtrGetPathIDMethod;
	private static MethodDefinition forceCreatePPtrHelper;
	private static MethodDefinition tryGetAssetHelper;
#nullable enable

	private static readonly Dictionary<TypeDefinition, TypeDefinition> pptrsToParameters = new();
	private static readonly Dictionary<TypeDefinition, IMethodDescriptor> pptrsToTryGetAssetMethods = new();
	private static readonly Dictionary<TypeDefinition, IMethodDescriptor> pptrsToSetAssetMethods = new();

	public static void DoPass()
	{
		pptrTypeImported = SharedState.Instance.Importer.ImportType(typeof(IPPtr<>));
		commonPPtrTypeGeneric = SharedState.Instance.Importer.ImportType(typeof(PPtr<>));
		commonPPtrType = SharedState.Instance.Importer.ImportType(typeof(PPtr));
		commonPPtrConstructor = SharedState.Instance.Importer.ImportConstructor<PPtr>(c => c.Parameters.Count == 2);
		commonPPtrGetFileIDMethod = SharedState.Instance.Importer.ImportMethod<PPtr>(m => m.Name == $"get_{nameof(PPtr.FileID)}");
		commonPPtrGetPathIDMethod = SharedState.Instance.Importer.ImportMethod<PPtr>(m => m.Name == $"get_{nameof(PPtr.PathID)}");
		forceCreatePPtrHelper = SharedState.Instance.InjectHelperType(typeof(PPtrHelper)).Methods.Single(m => m.Name == nameof(PPtrHelper.ForceCreatePPtr));
		tryGetAssetHelper = SharedState.Instance.InjectHelperType(typeof(PPtrHelper)).Methods.Single(m =>
		{
			return m.Name == nameof(PPtrHelper.TryGetAsset) && m.Signature!.ReturnType is CorLibTypeSignature;
		});

		IMethodDefOrRef pptrTryGetAssetMethod = SharedState.Instance.Importer.ImportMethod(typeof(IPPtr<>), method =>
		{
			return method.Name == nameof(IPPtr<IUnityObjectBase>.TryGetAsset) && method.Parameters.Count == 2;
		});
		IMethodDefOrRef pptrSetAssetMethod = SharedState.Instance.Importer.ImportMethod(typeof(IPPtr<>), method =>
		{
			return method.Name == nameof(IPPtr<IUnityObjectBase>.SetAsset) && method.Parameters.Count == 2;
		});

		foreach (SubclassGroup group in SharedState.Instance.SubclassGroups.Values.Where(g => g.IsPPtr))
		{
			bool usingMarkerInterface = !GetInterfaceParameterTypeDefinition(group, out TypeDefinition parameterType);
			group.Interface.AddPPtrInterfaceImplementation(parameterType);

			pptrsToTryGetAssetMethods.Add(group.Interface, MethodUtils.MakeMethodOnGenericType(
				SharedState.Instance.Importer,
				SharedState.Instance.Importer.ImportType(typeof(IPPtr<>)).MakeGenericInstanceType(parameterType.ToTypeSignature()),
				pptrTryGetAssetMethod));
			pptrsToSetAssetMethods.Add(group.Interface, MethodUtils.MakeMethodOnGenericType(
				SharedState.Instance.Importer,
				SharedState.Instance.Importer.ImportType(typeof(IPPtr<>)).MakeGenericInstanceType(parameterType.ToTypeSignature()),
				pptrSetAssetMethod));
			pptrsToParameters.Add(group.Interface, parameterType);

			foreach (GeneratedClassInstance instance in group.Instances)
			{
				DoPassOnInstance(usingMarkerInterface, parameterType, instance);
			}
		}
	}

	private static void DoPassOnInstance(bool usingMarkerInterface, TypeDefinition parameterType, GeneratedClassInstance instance)
	{
		instance.Type.ImplementFileIDAndPathID();

		TypeDefinition? instanceParameterType;
		if (usingMarkerInterface)
		{
			instanceParameterType = GetInstanceParameterTypeDefinition(instance);
			pptrsToParameters.Add(instance.Type, instanceParameterType);
			instance.Type.AddPPtrInterfaceImplementation(instanceParameterType);
		}
		else
		{
			instanceParameterType = null;
			pptrsToParameters.Add(instance.Type, parameterType);
		}

		AddTryGetAssetMethods(parameterType, instance, instanceParameterType);

		instance.Type.ImplementSetAssetMethods(parameterType.ToTypeSignature(), instanceParameterType?.ToTypeSignature());

		AddImplicitConversions(instance, parameterType, instanceParameterType);
	}

	private static void AddTryGetAssetMethods(TypeDefinition parameterType, GeneratedClassInstance instance, TypeDefinition? instanceParameterType)
	{
		MethodDefinition interfaceTryGetAssetMethod = instance.Type.ImplementTryGetAssetMethod(parameterType.ToTypeSignature());

		if (instanceParameterType is not null)
		{
			MethodDefinition instanceTryGetAssetMethod = instance.Type.ImplementTryGetAssetMethod(instanceParameterType.ToTypeSignature());
			pptrsToTryGetAssetMethods.Add(instance.Type, instanceTryGetAssetMethod);
		}
		else
		{
			pptrsToTryGetAssetMethods.Add(instance.Type, interfaceTryGetAssetMethod);
		}
	}

	private static void AddImplicitConversions(GeneratedClassInstance instance, TypeDefinition parameterType, TypeDefinition? instanceParameterType)
	{
		if (instanceParameterType is not null)
		{
			instance.Type.AddImplicitConversion(instanceParameterType.ToTypeSignature());
		}
		instance.Type.AddImplicitConversion(parameterType.ToTypeSignature());
		instance.Type.AddImplicitConversion<IUnityObjectBase>();
		instance.Type.AddImplicitConversion(commonPPtrType.ToTypeSignature(), commonPPtrConstructor);
	}

	private static void AddPPtrInterfaceImplementation(this TypeDefinition type, TypeDefinition parameterType)
	{
		GenericInstanceTypeSignature pptrInterface = pptrTypeImported.MakeGenericInstanceType(parameterType.ToTypeSignature());
		type.AddInterfaceImplementation(pptrInterface.ToTypeDefOrRef());
	}

	private static void ImplementFileIDAndPathID(this TypeDefinition pptrType)
	{
		pptrType.ImplementGetterProperty(
			nameof(IPPtr.FileID),
			InterfaceUtils.InterfacePropertyImplementation,
			SharedState.Instance.Importer.Int32,
			pptrType.GetFieldByName("m_FileID_"));

		FieldDefinition pathidField = pptrType.GetFieldByName("m_PathID_");
		PropertyDefinition property = pptrType.AddGetterProperty(nameof(IPPtr.PathID), InterfaceUtils.InterfacePropertyImplementation, SharedState.Instance.Importer.Int64);
		CilInstructionCollection instructions = property.GetMethod!.CilMethodBody!.Instructions;
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldfld, pathidField);
		if (pathidField.IsInt32Type())
		{
			instructions.Add(CilOpCodes.Conv_I8);
		}
		instructions.Add(CilOpCodes.Ret);
	}

	private static void ImplementSetAssetMethods(this TypeDefinition pptrType, TypeSignature groupParameterType, TypeSignature? instanceParameterType)
	{
		TypeSignature parameterType = instanceParameterType ?? groupParameterType;

		MethodDefinition mainMethod;
		{
			MethodDefinition method = mainMethod = pptrType.AddMethod(nameof(IPPtr<IUnityObjectBase>.SetAsset), InterfaceUtils.InterfaceMethodImplementation, SharedState.Instance.Importer.Void);
			method.AddParameter(SharedState.Instance.Importer.ImportType<AssetCollection>().ToTypeSignature(), "collection");
			method.AddParameter(parameterType, "asset").Definition!.AddNullableAttribute(NullableAnnotation.MaybeNull);
			CilInstructionCollection instructions = method.CilMethodBody!.Instructions;

			//Convert PPtr
			CilLocalVariable convertedPPtr = instructions.AddLocalVariable(commonPPtrType.ToTypeSignature());
			instructions.Add(CilOpCodes.Ldarg_1);
			instructions.Add(CilOpCodes.Ldarg_2);
			instructions.Add(CilOpCodes.Call, forceCreatePPtrHelper);
			instructions.Add(CilOpCodes.Stloc, convertedPPtr);

			//Store FileID
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Ldloca, convertedPPtr);
			instructions.Add(CilOpCodes.Call, commonPPtrGetFileIDMethod);
			instructions.Add(CilOpCodes.Stfld, pptrType.GetFieldByName("m_FileID_"));

			//Store PathID
			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Ldloca, convertedPPtr);
			instructions.Add(CilOpCodes.Call, commonPPtrGetPathIDMethod);
			FieldDefinition pathIDField = pptrType.GetFieldByName("m_PathID_");
			if (pathIDField.Signature!.FieldType is CorLibTypeSignature { ElementType: ElementType.I4 })
			{
				instructions.Add(CilOpCodes.Conv_Ovf_I4);//Convert I8 to I4
			}
			instructions.Add(CilOpCodes.Stfld, pathIDField);

			//Return
			instructions.Add(CilOpCodes.Ret);
		}

		pptrsToSetAssetMethods.Add(pptrType, mainMethod);

		//Secondary Method
		if (instanceParameterType is not null)
		{
			MethodDefinition method = pptrType.AddMethod(nameof(IPPtr<IUnityObjectBase>.SetAsset), InterfaceUtils.InterfaceMethodImplementation, SharedState.Instance.Importer.Void);
			method.AddParameter(SharedState.Instance.Importer.ImportType<AssetCollection>().ToTypeSignature(), "collection");
			method.AddParameter(groupParameterType, "asset").Definition!.AddNullableAttribute(NullableAnnotation.MaybeNull);
			CilInstructionCollection instructions = method.CilMethodBody!.Instructions;

			CilInstructionLabel returnLabel = new();
			CilInstructionLabel isNullLabel = new();

			instructions.Add(CilOpCodes.Ldarg_2);
			instructions.Add(CilOpCodes.Ldnull);
			instructions.Add(CilOpCodes.Cgt_Un);
			instructions.Add(CilOpCodes.Brfalse, isNullLabel);

			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Ldarg_1);
			instructions.Add(CilOpCodes.Ldarg_2);
			instructions.Add(CilOpCodes.Castclass, instanceParameterType.ToTypeDefOrRef());
			instructions.Add(CilOpCodes.Callvirt, mainMethod);
			instructions.Add(CilOpCodes.Br, returnLabel);

			isNullLabel.Instruction = instructions.Add(CilOpCodes.Nop);

			instructions.Add(CilOpCodes.Ldarg_0);
			instructions.Add(CilOpCodes.Callvirt, pptrType.GetMethodByName(nameof(IUnityAssetBase.Reset)));

			returnLabel.Instruction = instructions.Add(CilOpCodes.Nop);

			//Return
			instructions.Add(CilOpCodes.Ret);
		}
	}

	private static MethodDefinition ImplementTryGetAssetMethod(this TypeDefinition pptrType, TypeSignature parameterType)
	{
		MethodDefinition method = pptrType.AddMethod(nameof(IPPtr<IUnityObjectBase>.TryGetAsset), InterfaceUtils.InterfaceMethodImplementation, SharedState.Instance.Importer.Boolean);
		method.AddParameter(SharedState.Instance.Importer.ImportType<AssetCollection>().ToTypeSignature(), "collection");
		ParameterDefinition outParameter = method.AddParameter(parameterType.MakeByReferenceType(), "asset").Definition!;
		outParameter.IsOut = true;
		outParameter.AddNullableAttribute(NullableAnnotation.MaybeNull);
		outParameter.AddNotNullWhenAttribute(SharedState.Instance.Importer, true);
		CilInstructionCollection instructions = method.CilMethodBody!.Instructions;

		//Load container
		instructions.Add(CilOpCodes.Ldarg_1);

		//Load FileID
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldfld, pptrType.GetFieldByName("m_FileID_"));

		//Load PathID
		instructions.Add(CilOpCodes.Ldarg_0);
		FieldDefinition pathIDField = pptrType.GetFieldByName("m_PathID_");
		instructions.Add(CilOpCodes.Ldfld, pathIDField);
		if (pathIDField.Signature!.FieldType is CorLibTypeSignature { ElementType: ElementType.I4 })
		{
			instructions.Add(CilOpCodes.Conv_I8);//Convert I4 to I8
		}

		//Load out parameter
		instructions.Add(CilOpCodes.Ldarg_2);

		//Call TryGetAsset helper
		instructions.Add(CilOpCodes.Call, tryGetAssetHelper.MakeGenericInstanceMethod(parameterType));

		//Return
		instructions.Add(CilOpCodes.Ret);

		return method;
	}

	private static bool IsInt32Type(this FieldDefinition field) => field.Signature!.FieldType is CorLibTypeSignature { ElementType: ElementType.I4 };

	private static MethodDefinition AddImplicitConversion(this TypeDefinition pptrType, TypeSignature parameterType)
	{
		GenericInstanceTypeSignature resultPPtrSignature = commonPPtrTypeGeneric.MakeGenericInstanceType(parameterType);
		IMethodDefOrRef constructor = MethodUtils.MakeConstructorOnGenericType(SharedState.Instance.Importer, resultPPtrSignature, 2);
		return pptrType.AddImplicitConversion(resultPPtrSignature, constructor);
	}

	private static MethodDefinition AddImplicitConversion<T>(this TypeDefinition pptrType)
	{
		ITypeDefOrRef importedInterface = SharedState.Instance.Importer.ImportType<T>();
		return pptrType.AddImplicitConversion(importedInterface.ToTypeSignature());
	}

	private static MethodDefinition AddImplicitConversion(this TypeDefinition pptrType, TypeSignature resultTypeSignature, IMethodDefOrRef constructor)
	{
		FieldDefinition fileID = pptrType.Fields.Single(field => field.Name == "m_FileID_");
		FieldDefinition pathID = pptrType.Fields.Single(f => f.Name == "m_PathID_");

		MethodDefinition method = pptrType.AddEmptyConversion(pptrType.ToTypeSignature(), resultTypeSignature, true);

		CilInstructionCollection instructions = method.CilMethodBody!.Instructions;

		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldfld, fileID);
		instructions.Add(CilOpCodes.Ldarg_0);
		instructions.Add(CilOpCodes.Ldfld, pathID);
		if (pathID.IsInt32Type())
		{
			instructions.Add(CilOpCodes.Conv_I8);
		}

		instructions.Add(CilOpCodes.Newobj, constructor);
		instructions.Add(CilOpCodes.Ret);

		return method;
	}

	internal static bool GetInterfaceParameterTypeDefinition(SubclassGroup pptrGroup, out TypeDefinition type)
	{
		string parameterTypeName = pptrGroup.Name.Substring(5);
		if (SharedState.Instance.NameToTypeID.TryGetValue(parameterTypeName, out HashSet<int>? idList) && idList.Count == 1)
		{
			type = SharedState.Instance.ClassGroups[idList.First()].Interface;
			return true;
		}
		else
		{
			type = SharedState.Instance.MarkerInterfaces[parameterTypeName];
			return false;
		}
	}

	internal static TypeDefinition GetInstanceParameterTypeDefinition(GeneratedClassInstance pptrInstance)
	{
		string parameterTypeName = pptrInstance.Name.Substring(5);
		if (SharedState.Instance.NameToTypeID.TryGetValue(parameterTypeName, out HashSet<int>? list))
		{
			List<GeneratedClassInstance> instances = new();

			foreach (int id in list)
			{
				ClassGroup group = SharedState.Instance.ClassGroups[id];
				foreach (GeneratedClassInstance instance in group.Instances)
				{
					if (instance.VersionRange.Intersects(pptrInstance.VersionRange) && parameterTypeName == instance.Name)
					{
						instances.Add(instance);
					}
				}
			}

			if (instances.Count == 0)
			{
				throw new Exception($"Could not find type {parameterTypeName} on version {pptrInstance.VersionRange.Start} to {pptrInstance.VersionRange.End}");
			}
			else if (instances.Count == 1)
			{
				return instances[0].Type;
			}
			else if (instances.Select(instance => instance.Group).Distinct().Count() == 1)
			{
				return instances[0].Group.Interface;
			}
			else
			{
				return SharedState.Instance.MarkerInterfaces[parameterTypeName];
			}
		}
		else
		{
			throw new Exception($"Could not find {parameterTypeName} in the name dictionary");
		}
	}
}
