using AssemblyDumper.Utils;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.OcclusionCullingData;
using AssetRipper.Core.Classes.OcclusionCullingSettings;
using AssetRipper.Core.Classes.Renderer;

namespace AssemblyDumper.Passes
{
	public static class Pass343_SceneInterfaces
	{
		public static void DoPass()
		{
			Console.WriteLine("Pass 343: Scene Interfaces");
			if(SharedState.TypeDictionary.TryGetValue("SceneObjectIdentifier", out TypeDefinition? sceneObjectIdentifier))
			{
				sceneObjectIdentifier.ImplementSceneObjectIdentifier();
			}
			if (SharedState.TypeDictionary.TryGetValue("OcclusionScene", out TypeDefinition? occlusionScene))
			{
				occlusionScene.ImplementOcclusionScene();
			}
			if (SharedState.TypeDictionary.TryGetValue("OcclusionCullingData", out TypeDefinition? occlusionCullingData))
			{
				occlusionCullingData.ImplementOcclusionCullingData();
			}
			if (SharedState.TypeDictionary.TryGetValue("OcclusionCullingSettings", out TypeDefinition? occlusionCullingSettings)
				|| SharedState.TypeDictionary.TryGetValue("SceneSettings", out occlusionCullingSettings)
				|| SharedState.TypeDictionary.TryGetValue("Scene", out occlusionCullingSettings))
			{
				occlusionCullingSettings.ImplementOcclusionCullingSettings();
			}
		}

		private static void ImplementSceneObjectIdentifier(this TypeDefinition type)
		{
			type.AddInterfaceImplementation<ISceneObjectIdentifier>();
			type.ImplementFullProperty(nameof(ISceneObjectIdentifier.TargetObject), InterfaceUtils.InterfacePropertyImplementation, SystemTypeGetter.Int64, type.GetFieldByName("targetObject"));
			type.ImplementFullProperty(nameof(ISceneObjectIdentifier.TargetPrefab), InterfaceUtils.InterfacePropertyImplementation, SystemTypeGetter.Int64, type.GetFieldByName("targetPrefab"));
		}

		private static void ImplementOcclusionScene(this TypeDefinition type)
		{
			type.AddInterfaceImplementation<IOcclusionScene>();
			type.ImplementFullProperty(nameof(IOcclusionScene.IndexRenderers), InterfaceUtils.InterfacePropertyImplementation, SystemTypeGetter.Int32, type.GetFieldByName("indexRenderers"));
			type.ImplementFullProperty(nameof(IOcclusionScene.SizeRenderers), InterfaceUtils.InterfacePropertyImplementation, SystemTypeGetter.Int32, type.GetFieldByName("sizeRenderers"));
			type.ImplementFullProperty(nameof(IOcclusionScene.IndexPortals), InterfaceUtils.InterfacePropertyImplementation, SystemTypeGetter.Int32, type.GetFieldByName("indexPortals"));
			type.ImplementFullProperty(nameof(IOcclusionScene.SizePortals), InterfaceUtils.InterfacePropertyImplementation, SystemTypeGetter.Int32, type.GetFieldByName("sizePortals"));

			FieldDefinition guidField = type.GetFieldByName("scene");

			//specific to common
			MethodDefinition implicitConversion = guidField.Signature!.FieldType.Resolve()!.Methods.Single(m => m.Name == "op_Implicit");
			//common to specific
			MethodDefinition explicitConversion = guidField.Signature.FieldType.Resolve()!.Methods.Single(m => m.Name == "op_Explicit");

			ITypeDefOrRef unityGuid = SharedState.Importer.ImportCommonType<AssetRipper.Core.Classes.Misc.UnityGUID>();
			PropertyDefinition property = type.AddFullProperty(nameof(IOcclusionScene.Scene), InterfaceUtils.InterfacePropertyImplementation, unityGuid.ToTypeSignature());
			CilInstructionCollection getProcessor = property.GetMethod!.CilMethodBody!.Instructions;
			getProcessor.Add(CilOpCodes.Ldarg_0);
			getProcessor.Add(CilOpCodes.Ldfld, guidField);
			getProcessor.Add(CilOpCodes.Call, implicitConversion);
			getProcessor.Add(CilOpCodes.Ret);
			CilInstructionCollection setProcessor = property.SetMethod!.CilMethodBody!.Instructions;
			setProcessor.Add(CilOpCodes.Ldarg_0);
			setProcessor.Add(CilOpCodes.Ldarg_1);
			setProcessor.Add(CilOpCodes.Call, explicitConversion);
			setProcessor.Add(CilOpCodes.Stfld, guidField);
			setProcessor.Add(CilOpCodes.Ret);
		}

		private static void ImplementOcclusionCullingData(this TypeDefinition type)
		{
			type.AddInterfaceImplementation<IOcclusionCullingData>();
			TypeSignature sceneObjectIdentifierArray = SharedState.Importer.ImportCommonType<ISceneObjectIdentifier>().MakeSzArrayType();
			TypeSignature occlusionSceneArray = SharedState.Importer.ImportCommonType<IOcclusionScene>().MakeSzArrayType();
			type.ImplementFullProperty(nameof(IOcclusionCullingData.PVSData), InterfaceUtils.InterfacePropertyImplementation, SystemTypeGetter.UInt8.MakeSzArrayType(), type.TryGetFieldByName("m_PVSData"));
			type.ImplementGetterProperty(nameof(IOcclusionCullingData.Scenes), InterfaceUtils.InterfacePropertyImplementation, occlusionSceneArray, type.TryGetFieldByName("m_Scenes"));
			type.ImplementGetterProperty(nameof(IOcclusionCullingData.StaticRenderers), InterfaceUtils.InterfacePropertyImplementation, sceneObjectIdentifierArray, type.TryGetFieldByName("m_StaticRenderers"));
			type.ImplementGetterProperty(nameof(IOcclusionCullingData.Portals), InterfaceUtils.InterfacePropertyImplementation, sceneObjectIdentifierArray, type.TryGetFieldByName("m_Portals"));
			type.ImplementInitializeMethod(nameof(IOcclusionCullingData.InitializeScenes), "m_Scenes");
			type.ImplementInitializeMethod(nameof(IOcclusionCullingData.InitializeStaticRenderers), "m_StaticRenderers");
			type.ImplementInitializeMethod(nameof(IOcclusionCullingData.InitializePortals), "m_Portals");
		}

		private static void ImplementInitializeMethod(this TypeDefinition type, string methodName, string fieldName)
		{
			MethodDefinition initializeMethod = type.AddMethod(methodName, InterfaceUtils.InterfaceMethodImplementation, SystemTypeGetter.Void);
			initializeMethod.AddParameter("count", SystemTypeGetter.Int32);
			initializeMethod.CilMethodBody!.InitializeLocals = true;
			CilInstructionCollection processor = initializeMethod.CilMethodBody.Instructions;

			if(type.TryGetFieldByName(fieldName, out FieldDefinition? field))
			{
				processor.AddInitializeArrayField(field);
			}
			
			processor.Add(CilOpCodes.Ret);
		}

		private static void ImplementOcclusionCullingSettings(this TypeDefinition type)
		{
			type.AddInterfaceImplementation<IOcclusionCullingSettings>();
			type.ImplementFullProperty(nameof(IOcclusionCullingSettings.PVSData), InterfaceUtils.InterfacePropertyImplementation, SystemTypeGetter.UInt8.MakeSzArrayType(), type.TryGetFieldByName("m_PVSData"));
			type.ImplementSceneGuidProperty();
			type.ImplementStaticRenderersProperty();
			type.ImplementPortalsProperty();
			type.ImplementOcclusionCullingDataProperty();
		}

		private static void ImplementSceneGuidProperty(this TypeDefinition type)
		{
			TypeSignature commonGuidSignature = SharedState.Importer.ImportCommonType<AssetRipper.Core.Classes.Misc.UnityGUID>().ToTypeSignature();
			if (type.TryGetFieldByName("m_SceneGUID", out FieldDefinition? sceneGuidField))
			{
				//specific to common
				MethodDefinition implicitConversion = sceneGuidField.Signature!.FieldType.Resolve()!.Methods.Single(m => m.Name == "op_Implicit");
				//common to specific
				MethodDefinition explicitConversion = sceneGuidField.Signature.FieldType.Resolve()!.Methods.Single(m => m.Name == "op_Explicit");

				PropertyDefinition property = type.AddFullProperty(nameof(IOcclusionCullingSettings.SceneGUID), InterfaceUtils.InterfacePropertyImplementation, commonGuidSignature);
				CilInstructionCollection getProcessor = property.GetMethod!.CilMethodBody!.Instructions;
				getProcessor.Add(CilOpCodes.Ldarg_0);
				getProcessor.Add(CilOpCodes.Ldfld, sceneGuidField);
				getProcessor.Add(CilOpCodes.Call, implicitConversion);
				getProcessor.Add(CilOpCodes.Ret);
				CilInstructionCollection setProcessor = property.SetMethod!.CilMethodBody!.Instructions;
				setProcessor.Add(CilOpCodes.Ldarg_0);
				setProcessor.Add(CilOpCodes.Ldarg_1);
				setProcessor.Add(CilOpCodes.Call, explicitConversion);
				setProcessor.Add(CilOpCodes.Stfld, sceneGuidField);
				setProcessor.Add(CilOpCodes.Ret);
			}
			else
			{
				type.ImplementFullProperty(nameof(IOcclusionCullingSettings.SceneGUID), InterfaceUtils.InterfacePropertyImplementation, commonGuidSignature, null);
			}
		}

		private static void ImplementStaticRenderersProperty(this TypeDefinition type)
		{
			TypeSignature rendererTypeSignature = SharedState.Importer.ImportCommonType<IRenderer>().ToTypeSignature();
			TypeSignature returnTypeSignature = SharedState.Importer.ImportTypeSignature(
				CommonTypeGetter.LookupCommonType<IOcclusionCullingSettings>()!
				.Properties
				.Single(m => m.Name == nameof(IOcclusionCullingSettings.StaticRenderers))
				.Signature!
				.ReturnType);
			if (type.TryGetFieldByName("m_StaticRenderers", out FieldDefinition? field) || type.TryGetFieldByName("m_PVSObjectsArray", out field))
			{
				IMethodDefOrRef castMethod = SharedState.Importer.ImportCommonMethod(typeof(PPtr), m => m.Name == nameof(PPtr.CastArray));
				MethodSpecification? castMethodInstance = MethodUtils.MakeGenericInstanceMethod(castMethod, rendererTypeSignature);
				PropertyDefinition property = type.AddGetterProperty(nameof(IOcclusionCullingSettings.StaticRenderers), InterfaceUtils.InterfacePropertyImplementation, returnTypeSignature);
				CilInstructionCollection processor = property.GetMethod!.CilMethodBody!.Instructions;
				processor.Add(CilOpCodes.Ldarg_0);
				processor.Add(CilOpCodes.Ldfld, field);
				processor.Add(CilOpCodes.Call, castMethodInstance);
				processor.Add(CilOpCodes.Ret);
			}
			else
			{
				type.ImplementGetterProperty(nameof(IOcclusionCullingSettings.StaticRenderers), InterfaceUtils.InterfacePropertyImplementation, returnTypeSignature, null);
			}
		}

		private static void ImplementPortalsProperty(this TypeDefinition type)
		{
			TypeSignature portalTypeSignature = SharedState.Importer.ImportCommonType<IOcclusionPortal>().ToTypeSignature();
			TypeSignature returnTypeSignature = SharedState.Importer.ImportTypeSignature(
				CommonTypeGetter.LookupCommonType<IOcclusionCullingSettings>()!
				.Properties
				.Single(m => m.Name == nameof(IOcclusionCullingSettings.Portals))
				.Signature!
				.ReturnType);
			if (type.TryGetFieldByName("m_Portals", out FieldDefinition? field) || type.TryGetFieldByName("m_PVSPortalsArray", out field))
			{
				IMethodDefOrRef castMethod = SharedState.Importer.ImportCommonMethod(typeof(PPtr), m => m.Name == nameof(PPtr.CastArray));
				MethodSpecification? castMethodInstance = MethodUtils.MakeGenericInstanceMethod(castMethod, portalTypeSignature);
				PropertyDefinition property = type.AddGetterProperty(nameof(IOcclusionCullingSettings.Portals), InterfaceUtils.InterfacePropertyImplementation, returnTypeSignature);
				CilInstructionCollection processor = property.GetMethod!.CilMethodBody!.Instructions;
				processor.Add(CilOpCodes.Ldarg_0);
				processor.Add(CilOpCodes.Ldfld, field);
				processor.Add(CilOpCodes.Call, castMethodInstance);
				processor.Add(CilOpCodes.Ret);
			}
			else
			{
				type.ImplementGetterProperty(nameof(IOcclusionCullingSettings.Portals), InterfaceUtils.InterfacePropertyImplementation, returnTypeSignature, null);
			}
		}

		private static void ImplementOcclusionCullingDataProperty(this TypeDefinition type)
		{
			TypeSignature returnTypeSignature = SharedState.Importer.ImportTypeSignature(
				CommonTypeGetter.LookupCommonType<IOcclusionCullingSettings>()!
				.Properties
				.Single(m => m.Name == nameof(IOcclusionCullingSettings.OcclusionCullingData))
				.Signature!
				.ReturnType);
			if (type.TryGetFieldByName("m_OcclusionCullingData", out FieldDefinition? field))
			{
				TypeSignature fieldType = field.Signature!.FieldType;
				MethodDefinition explicitConversionMethod = PPtrUtils.GetExplicitConversion<IOcclusionCullingData>(fieldType.Resolve()!);
				PropertyDefinition property = type.AddGetterProperty(nameof(IOcclusionCullingSettings.OcclusionCullingData), InterfaceUtils.InterfacePropertyImplementation, returnTypeSignature);
				CilInstructionCollection processor = property.GetMethod!.CilMethodBody!.Instructions;
				processor.Add(CilOpCodes.Ldarg_0);
				processor.Add(CilOpCodes.Ldfld, field);
				processor.Add(CilOpCodes.Call, explicitConversionMethod);
				processor.Add(CilOpCodes.Ret);
			}
			else
			{
				type.ImplementGetterProperty(nameof(IOcclusionCullingSettings.OcclusionCullingData), InterfaceUtils.InterfacePropertyImplementation, returnTypeSignature, null);
			}
		}
	}
}
