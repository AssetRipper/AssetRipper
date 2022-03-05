using AssemblyDumper.Utils;
using AssetRipper.Core.Parser.Files;
using UnityHandlerBase = AssetRipper.Core.VersionHandling.UnityHandlerBase;

namespace AssemblyDumper.Passes
{
	public static class Pass950_UnityVersionHandler
	{
		const TypeAttributes SealedClassAttributes = TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.Public | TypeAttributes.Sealed;
		const MethodAttributes ConstructorAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RuntimeSpecialName;
		const MethodAttributes PropertyOverrideAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public static TypeDefinition HandlerDefinition { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public static void DoPass()
		{
			Console.WriteLine("Pass 950: Unity Version Handler");
			ITypeDefOrRef baseHandler = SharedState.Importer.ImportCommonType<UnityHandlerBase>();
			HandlerDefinition = new TypeDefinition(SharedState.RootNamespace, "UnityVersionHandler", SealedClassAttributes, baseHandler);
			SharedState.Module.TopLevelTypes.Add(HandlerDefinition);
			HandlerDefinition.AddConstructor();
		}

		private static void AddConstructor(this TypeDefinition type)
		{
			MethodDefinition? constructor = type.AddMethod(".ctor", ConstructorAttributes, SystemTypeGetter.Void);
			
			CilInstructionCollection processor = constructor.CilMethodBody!.Instructions;
			processor.AddBaseConstructorCall();
			processor.AddAssetFactoryAssignment();
			processor.AddImporterFactoryAssignment();
			processor.AddUnityVersionAssignment();
			processor.AddCommonStringAssignment();
			processor.AddClassIDTypeEnumAssignment();
			processor.Add(CilOpCodes.Ret);
			processor.OptimizeMacros();
		}

		private static void AddBaseConstructorCall(this CilInstructionCollection processor)
		{
			IMethodDefOrRef baseConstructor = SharedState.Importer.ImportCommonConstructor<UnityHandlerBase>();
			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Call, baseConstructor);
		}

		private static void AddAssetFactoryAssignment(this CilInstructionCollection processor)
		{
			IMethodDefOrRef baseAssetFactorySetter = SharedState.Importer.ImportCommonMethod<UnityHandlerBase>(m => m.Name == "set_AssetFactory");
			MethodDefinition assetFactoryConstructor = Pass940_MakeAssetFactory.FactoryDefinition.Methods.Single(c => c.IsConstructor && !c.IsStatic);
			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Newobj, assetFactoryConstructor);
			processor.Add(CilOpCodes.Call, baseAssetFactorySetter);
		}

		private static void AddImporterFactoryAssignment(this CilInstructionCollection processor)
		{
			IMethodDefOrRef baseAssetImporterFactorySetter = SharedState.Importer.ImportCommonMethod<UnityHandlerBase>(m => m.Name == "set_ImporterFactory");
			MethodDefinition assetImporterFactoryConstructor = Pass942_MakeImporterFactory.ImporterFactoryDefinition.Methods.Single(c => c.IsConstructor && !c.IsStatic);
			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Newobj, assetImporterFactoryConstructor);
			processor.Add(CilOpCodes.Call, baseAssetImporterFactorySetter);
		}

		private static void AddCommonStringAssignment(this CilInstructionCollection processor)
		{
			IMethodDefOrRef baseCommonStringSetter = SharedState.Importer.ImportCommonMethod<UnityHandlerBase>(m => m.Name == "set_CommonStringDictionary");
			FieldDefinition field = Pass001_CreateBasicTypes.CommonStringTypeDefinition.GetFieldByName("dictionary");
			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Ldsfld, field);
			processor.Add(CilOpCodes.Call, baseCommonStringSetter);
		}

		private static void AddClassIDTypeEnumAssignment(this CilInstructionCollection processor)
		{
			IMethodDefOrRef baseClassIdSetter = SharedState.Importer.ImportCommonMethod<UnityHandlerBase>(m => m.Name == "set_ClassIDTypeEnum");
			IMethodDefOrRef getTypeFromHandle = SharedState.Importer.ImportSystemMethod<Type>(m => m.Name == "GetTypeFromHandle");
			TypeDefinition type = Pass001_CreateBasicTypes.ClassIDTypeDefinition;
			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Ldtoken, type);
			processor.Add(CilOpCodes.Call, getTypeFromHandle);
			processor.Add(CilOpCodes.Call, baseClassIdSetter);
		}

		private static void AddUnityVersionAssignment(this CilInstructionCollection processor)
		{
			IMethodDefOrRef baseUnityVersionSetter = SharedState.Importer.ImportCommonMethod<UnityHandlerBase>(m => m.Name == "set_UnityVersion");
			ITypeDefOrRef unityVersionRef = SharedState.Importer.ImportCommonType<UnityVersion>();
			processor.Add(CilOpCodes.Ldarg_0);
			processor.AddUnityVersion();
			processor.Add(CilOpCodes.Call, baseUnityVersionSetter);
		}

		private static void AddUnityVersion(this CilInstructionCollection processor)
		{
			UnityVersion version = GetUnityVersion();
			IMethodDefOrRef constructor = SharedState.Importer.ImportCommonConstructor<UnityVersion>(5);
			processor.Add(CilOpCodes.Ldc_I4, version.Major);
			processor.Add(CilOpCodes.Ldc_I4, version.Minor);
			processor.Add(CilOpCodes.Ldc_I4, version.Build);
			processor.Add(CilOpCodes.Ldc_I4, (int)version.Type);
			processor.Add(CilOpCodes.Ldc_I4, version.TypeNumber);
			processor.Add(CilOpCodes.Newobj, constructor);
		}

		private static UnityVersion GetUnityVersion() => UnityVersion.Parse(SharedState.Version);
	}
}
