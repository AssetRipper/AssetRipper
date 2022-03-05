using AssemblyDumper.Utils;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Classes.Misc;

namespace AssemblyDumper.Passes
{
	public static class Pass080_PPtrConversions
	{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		private static ITypeDefOrRef commonPPtrType;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		const MethodAttributes InterfacePropertyImplementationAttributes =
			MethodAttributes.Public |
			MethodAttributes.Final |
			MethodAttributes.HideBySig |
			MethodAttributes.SpecialName |
			MethodAttributes.NewSlot |
			MethodAttributes.Virtual;
		const MethodAttributes ConversionAttributes = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

		public static void DoPass()
		{
			System.Console.WriteLine("Pass 080: PPtr Interface and Conversions");

			commonPPtrType = SharedState.Importer.ImportCommonType("AssetRipper.Core.Classes.Misc.PPtr`1");

			foreach (string name in SharedState.ClassDictionary.Keys)
			{
				if (name.StartsWith("PPtr_"))
				{
					TypeDefinition pptrType = SharedState.TypeDictionary[name];

					pptrType.ImplementPPtrInterface();

					string parameterTypeName = name.Substring(5, name.LastIndexOf('_') - 5);
					TypeDefinition parameterType = SharedState.TypeDictionary[parameterTypeName];
					GenericInstanceTypeSignature implicitConversionResultType = commonPPtrType.MakeGenericInstanceType(parameterType.ToTypeSignature());

					pptrType.AddImplicitConversion(implicitConversionResultType);
					pptrType.AddExplicitConversion<IUnityObjectBase>();
					if (name == "PPtr_GameObject_")
					{
						pptrType.AddExplicitConversion<AssetRipper.Core.Classes.GameObject.IGameObject>();
					}
					else if (name == "PPtr_Component_")
					{
						pptrType.AddExplicitConversion<AssetRipper.Core.Classes.IComponent>();
					}
					else if (name == "PPtr_MonoScript_")
					{
						pptrType.AddExplicitConversion<AssetRipper.Core.Classes.IMonoScript>();
					}
					else if (name == "PPtr_Transform_")
					{
						pptrType.AddExplicitConversion<AssetRipper.Core.Classes.ITransform>();
					}
					else if (name == "PPtr_Renderer_")
					{
						pptrType.AddExplicitConversion<AssetRipper.Core.Classes.Renderer.IRenderer>();
					}
					else if (name == "PPtr_OcclusionPortal_")
					{
						pptrType.AddExplicitConversion<AssetRipper.Core.Classes.IOcclusionPortal>();
					}
					else if (name == "PPtr_OcclusionCullingData_")
					{
						pptrType.AddExplicitConversion<AssetRipper.Core.Classes.OcclusionCullingData.IOcclusionCullingData>();
					}
					else if (name == "PPtr_PrefabInstance_")
					{
						pptrType.AddExplicitConversion<AssetRipper.Core.Classes.PrefabInstance.IPrefabInstance>();
					}
					else if (name == "PPtr_DataTemplate_")
					{
						pptrType.AddExplicitConversion<AssetRipper.Core.Classes.PrefabInstance.IPrefabInstance>();
					}
					else if (name == "PPtr_Prefab_")// && !SharedState.TypeDictionary.ContainsKey("PPtr_PrefabInstance_"))
					{
						pptrType.AddExplicitConversion<AssetRipper.Core.Classes.PrefabInstance.IPrefabInstance>();
					}
					else if (name == "PPtr_TerrainData_")
					{
						pptrType.AddExplicitConversion<AssetRipper.Core.Classes.TerrainData.ITerrainData>();
					}
				}
			}
		}

		private static void ImplementPPtrInterface(this TypeDefinition pptrType)
		{
			pptrType.Interfaces.Add(new InterfaceImplementation(SharedState.Importer.ImportCommonType<IPPtr>()));
			pptrType.ImplementFullProperty(nameof(IPPtr.FileIndex), InterfacePropertyImplementationAttributes, SystemTypeGetter.Int32, pptrType.GetFieldByName("m_FileID"));
			FieldDefinition pathidField = pptrType.GetFieldByName("m_PathID");
			PropertyDefinition property = pptrType.AddFullProperty(nameof(IPPtr.PathID), InterfacePropertyImplementationAttributes, SystemTypeGetter.Int64);
			CilInstructionCollection getProcessor = property.GetMethod!.CilMethodBody!.Instructions;
			getProcessor.Add(CilOpCodes.Ldarg_0);
			getProcessor.Add(CilOpCodes.Ldfld, pathidField);
			getProcessor.Add(CilOpCodes.Ret);
			CilInstructionCollection setProcessor = property.SetMethod!.CilMethodBody!.Instructions;
			setProcessor.Add(CilOpCodes.Ldarg_0);
			setProcessor.Add(CilOpCodes.Ldarg_1);
			if(pathidField.Signature!.FieldType.Name == "Int32")
				setProcessor.Add(CilOpCodes.Conv_Ovf_I4);
			setProcessor.Add(CilOpCodes.Stfld, pathidField);
			setProcessor.Add(CilOpCodes.Ret);
		}

		private static MethodDefinition AddImplicitConversion(this TypeDefinition pptrType, GenericInstanceTypeSignature resultTypeSignature)
		{
			return pptrType.AddConversion(resultTypeSignature, false);
		}

		private static MethodDefinition AddExplicitConversion(this TypeDefinition pptrType, GenericInstanceTypeSignature resultTypeSignature)
		{
			return pptrType.AddConversion(resultTypeSignature, true);
		}

		private static MethodDefinition AddExplicitConversion<T>(this TypeDefinition pptrType)
		{
			ITypeDefOrRef importedInterface = SharedState.Importer.ImportCommonType<T>();
			GenericInstanceTypeSignature resultPPtrSignature = commonPPtrType.MakeGenericInstanceType(importedInterface.ToTypeSignature());
			return pptrType.AddExplicitConversion(resultPPtrSignature);
		}

		private static MethodDefinition AddConversion(this TypeDefinition pptrType, GenericInstanceTypeSignature resultTypeSignature, bool isExplicit)
		{
			IMethodDefOrRef constructor = MethodUtils.MakeConstructorOnGenericType(resultTypeSignature, 2);

			FieldDefinition fileID = pptrType.Fields.Single(field => field.Name == "m_FileID");
			FieldDefinition pathID = pptrType.Fields.Single(f => f.Name == "m_PathID");

			string methodName = isExplicit ? "op_Explicit" : "op_Implicit";
			MethodDefinition method = pptrType.AddMethod(methodName, ConversionAttributes, resultTypeSignature);
			method.AddParameter("value", pptrType);

			CilInstructionCollection processor = method.CilMethodBody!.Instructions;

			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Ldfld, fileID);
			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Ldfld, pathID);
			processor.Add(CilOpCodes.Newobj, constructor);
			processor.Add(CilOpCodes.Ret);

			return method;
		}
	}
}
