using AssemblyDumper.Utils;
using AssetRipper.Core.Classes.Meta.Importers;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;

namespace AssemblyDumper.Passes
{
	public static class Pass942_MakeImporterFactory
	{
		const TypeAttributes SealedClassAttributes = TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.Public | TypeAttributes.Sealed;
		const MethodAttributes InterfaceOverrideAttributes = MethodAttributes.Public | MethodAttributes.NewSlot | MethodAttributes.HideBySig | MethodAttributes.Final | MethodAttributes.Virtual;

		public static TypeDefinition? ImporterFactoryDefinition { get; private set; }

		public static void DoPass()
		{
			Console.WriteLine("Pass 942: Make Importer Factory");
			ImporterFactoryDefinition = CreateFactoryDefinition();
			ImporterFactoryDefinition.AddCreateImporterMethod<IDefaultImporter>(nameof(IAssetImporterFactory.CreateDefaultImporter), "DefaultImporter");
			ImporterFactoryDefinition.AddCreateImporterMethod<INativeFormatImporter>(nameof(IAssetImporterFactory.CreateNativeFormatImporter), "NativeFormatImporter");
			ImporterFactoryDefinition.AddCreateImporterMethod<IMonoImporter>(nameof(IAssetImporterFactory.CreateMonoImporter), "MonoImporter");
		}

		private static TypeDefinition CreateFactoryDefinition()
		{
			TypeDefinition result = new TypeDefinition(SharedState.RootNamespace, "AssetImporterFactory", SealedClassAttributes, SystemTypeGetter.Object.ToTypeDefOrRef());
			SharedState.Module.TopLevelTypes.Add(result);
			result.Interfaces.Add(new InterfaceImplementation(SharedState.Importer.ImportCommonType<IAssetImporterFactory>()));
			ConstructorUtils.AddDefaultConstructor(result);
			return result;
		}

		private static void AddCreateImporterMethod<TInterface>(this TypeDefinition factoryDefinition, string methodName, string className)
		{
			ITypeDefOrRef inativeImporter = SharedState.Importer.ImportCommonType<TInterface>();
			ITypeDefOrRef layoutInfoType = SharedState.Importer.ImportCommonType<LayoutInfo>();

			MethodDefinition constructor = SharedState.TypeDictionary[className].Methods
				.Single(m => m.IsConstructor && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.Name == nameof(LayoutInfo));

			MethodDefinition method = factoryDefinition.AddMethod(methodName, InterfaceOverrideAttributes, inativeImporter);
			method.AddParameter("layout", layoutInfoType);

			CilInstructionCollection processor = method.CilMethodBody!.Instructions;
			processor.Add(CilOpCodes.Ldarg_1); //layout
			processor.Add(CilOpCodes.Newobj, constructor);
			processor.Add(CilOpCodes.Ret);
		}
	}
}
