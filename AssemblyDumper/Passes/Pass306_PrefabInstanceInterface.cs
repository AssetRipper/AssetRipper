using AssemblyDumper.Utils;
using AssetRipper.Core.Classes.GameObject;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.PrefabInstance;

namespace AssemblyDumper.Passes
{
	public static class Pass306_PrefabInstanceInterface
	{
		const MethodAttributes InterfacePropertyImplementationAttributes =
			MethodAttributes.Public |
			MethodAttributes.Final |
			MethodAttributes.HideBySig |
			MethodAttributes.SpecialName |
			MethodAttributes.NewSlot |
			MethodAttributes.Virtual;

		public static void DoPass()
		{
			Console.WriteLine("Pass 306: PrefabInstance Interface");
			if (SharedState.TypeDictionary.TryGetValue("PrefabInstance", out TypeDefinition? type))
			{
				type.ImplementPrefabInstanceInterface();
			}
			if (SharedState.TypeDictionary.TryGetValue("DataTemplate", out type))
			{
				type.ImplementPrefabInstanceInterface();
			}
			if (SharedState.TypeDictionary.TryGetValue("Prefab", out type))
			{
				type.ImplementPrefabInstanceInterface();
			}
		}

		private static void ImplementPrefabInstanceInterface(this TypeDefinition type)
		{
			ITypeDefOrRef prefabInstanceInterface = SharedState.Importer.ImportCommonType<IPrefabInstance>();
			type.Interfaces.Add(new InterfaceImplementation(prefabInstanceInterface));
			type.ImplementRootGameObjectProperty();
			type.ImplementSourcePrefabProperty();
			type.ImplementIsPrefabAssetProperty();
		}

		private static void ImplementRootGameObjectProperty(this TypeDefinition type)
		{
			ITypeDefOrRef commonPPtrType = SharedState.Importer.ImportCommonType("AssetRipper.Core.Classes.Misc.PPtr`1");
			ITypeDefOrRef gameObjectInterface = SharedState.Importer.ImportCommonType<IGameObject>();
			GenericInstanceTypeSignature gameObjectPPtrType = commonPPtrType.MakeGenericInstanceType(gameObjectInterface.ToTypeSignature());

			MethodDefinition explicitConversionMethod = PPtrUtils.GetExplicitConversion<IGameObject>(SharedState.TypeDictionary["PPtr_GameObject_"]);

			if(type.TryGetFieldByName("m_RootGameObject", out FieldDefinition? field))
			{
				PropertyDefinition property = type.AddFullProperty(nameof(IPrefabInstance.RootGameObjectPtr), InterfacePropertyImplementationAttributes, gameObjectPPtrType);

				CilInstructionCollection getProcessor = property.GetMethod!.CilMethodBody!.Instructions;
				getProcessor.Add(CilOpCodes.Ldarg_0);
				getProcessor.Add(CilOpCodes.Ldfld, field);
				getProcessor.Add(CilOpCodes.Call, explicitConversionMethod);
				getProcessor.Add(CilOpCodes.Ret);

				CilInstructionCollection setProcessor = property.SetMethod!.CilMethodBody!.Instructions;
				IMethodDefOrRef pptrSetMethod = SharedState.Importer.ImportCommonMethod(typeof(PPtr), m => m.Name == "SetValues");
				setProcessor.Add(CilOpCodes.Ldarg_0);
				setProcessor.Add(CilOpCodes.Ldfld, field);
				setProcessor.Add(CilOpCodes.Ldarg_1);
				setProcessor.Add(CilOpCodes.Call, pptrSetMethod);
				setProcessor.Add(CilOpCodes.Ret);

				setProcessor.Owner.VerifyLabels();
			}
			else
			{
				type.ImplementFullProperty(nameof(IPrefabInstance.RootGameObjectPtr), InterfacePropertyImplementationAttributes, gameObjectPPtrType, null);
			}
		}

		private static void ImplementSourcePrefabProperty(this TypeDefinition type)
		{
			ITypeDefOrRef commonPPtrType = SharedState.Importer.ImportCommonType("AssetRipper.Core.Classes.Misc.PPtr`1");
			ITypeDefOrRef prefabInstanceInterface = SharedState.Importer.ImportCommonType<IPrefabInstance>();
			GenericInstanceTypeSignature prefabInstancePPtrType = commonPPtrType.MakeGenericInstanceType(prefabInstanceInterface.ToTypeSignature());

			if (type.TryGetFieldByName("m_SourcePrefab", out FieldDefinition? field))
			{
				type.ImplementSourcePrefabProperty(field, prefabInstancePPtrType);
			}
			else if (type.TryGetFieldByName("m_ParentPrefab", out field))
			{
				type.ImplementSourcePrefabProperty(field, prefabInstancePPtrType);
			}
			else if(type.TryGetFieldByName("m_Father", out field))
			{
				type.ImplementSourcePrefabProperty(field, prefabInstancePPtrType);
			}
			else
			{
				type.ImplementFullProperty(nameof(IPrefabInstance.SourcePrefabPtr), InterfacePropertyImplementationAttributes, prefabInstancePPtrType, null);
			}
		}

		private static void ImplementSourcePrefabProperty(this TypeDefinition type, FieldDefinition field, GenericInstanceTypeSignature prefabInstancePPtrType)
		{
			MethodDefinition explicitConversionMethod = PPtrUtils.GetExplicitConversion<IPrefabInstance>(field.Signature!.FieldType.Resolve()!);

			PropertyDefinition property = type.AddFullProperty(nameof(IPrefabInstance.SourcePrefabPtr), InterfacePropertyImplementationAttributes, prefabInstancePPtrType);

			CilInstructionCollection getProcessor = property.GetMethod!.CilMethodBody!.Instructions;
			getProcessor.Add(CilOpCodes.Ldarg_0);
			getProcessor.Add(CilOpCodes.Ldfld, field);
			getProcessor.Add(CilOpCodes.Call, explicitConversionMethod);
			getProcessor.Add(CilOpCodes.Ret);

			CilInstructionCollection setProcessor = property.SetMethod!.CilMethodBody!.Instructions;
			IMethodDefOrRef pptrSetMethod = SharedState.Importer.ImportCommonMethod(typeof(PPtr), m => m.Name == "SetValues");
			setProcessor.Add(CilOpCodes.Ldarg_0);
			setProcessor.Add(CilOpCodes.Ldfld, field);
			setProcessor.Add(CilOpCodes.Ldarg_1);
			setProcessor.Add(CilOpCodes.Call, pptrSetMethod);
			setProcessor.Add(CilOpCodes.Ret);
		}

		private static void ImplementIsPrefabAssetProperty(this TypeDefinition type)
		{
			if(type.TryGetFieldByName("m_IsPrefabAsset", out FieldDefinition? field))
			{
				type.ImplementFullProperty(nameof(IPrefabInstance.IsPrefabAsset), InterfacePropertyImplementationAttributes, SystemTypeGetter.Boolean, field);
			}
			else if (type.TryGetFieldByName("m_IsPrefabParent", out field))
			{
				type.ImplementFullProperty(nameof(IPrefabInstance.IsPrefabAsset), InterfacePropertyImplementationAttributes, SystemTypeGetter.Boolean, field);
			}
			else if (type.TryGetFieldByName("m_IsDataTemplate", out field))
			{
				type.ImplementFullProperty(nameof(IPrefabInstance.IsPrefabAsset), InterfacePropertyImplementationAttributes, SystemTypeGetter.Boolean, field);
			}
			else
			{
				type.ImplementFullProperty(nameof(IPrefabInstance.IsPrefabAsset), InterfacePropertyImplementationAttributes, SystemTypeGetter.Boolean, null);
			}
		}
	}
}
