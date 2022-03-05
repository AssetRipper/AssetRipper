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

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public static TypeDefinition ImporterFactoryDefinition { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

		public static void DoPass()
		{
			Console.WriteLine("Pass 942: Make Importer Factory");
			ImporterFactoryDefinition = CreateFactoryDefinition();
			ImporterFactoryDefinition.AddCreateDefaultImporter();
			ImporterFactoryDefinition.AddCreateNativeFormatImporter();
		}

		private static TypeDefinition CreateFactoryDefinition()
		{
			TypeDefinition? result = new TypeDefinition(SharedState.RootNamespace, "AssetImporterFactory", SealedClassAttributes, SystemTypeGetter.Object.ToTypeDefOrRef());
			SharedState.Module.TopLevelTypes.Add(result);
			result.Interfaces.Add(new InterfaceImplementation(SharedState.Importer.ImportCommonType<IAssetImporterFactory>()));
			ConstructorUtils.AddDefaultConstructor(result);
			return result;
		}

		private static void AddCreateDefaultImporter(this TypeDefinition factoryDefinition)
		{
			ITypeDefOrRef idefaultImporter = SharedState.Importer.ImportCommonType<IDefaultImporter>();
			ITypeDefOrRef layoutInfoType = SharedState.Importer.ImportCommonType<LayoutInfo>();

			MethodDefinition constructor = SharedState.TypeDictionary["DefaultImporter"].Methods
				.Single(m => m.IsConstructor && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.Name == "LayoutInfo");

			MethodDefinition method = factoryDefinition.AddMethod("CreateDefaultImporter", InterfaceOverrideAttributes, idefaultImporter);
			method.AddParameter("layout", layoutInfoType);
			
			CilInstructionCollection processor = method.CilMethodBody!.Instructions;
			processor.Add(CilOpCodes.Ldarg_1); //layout
			processor.Add(CilOpCodes.Newobj, constructor);
			processor.Add(CilOpCodes.Ret);
		}

		private static void AddCreateNativeFormatImporter(this TypeDefinition factoryDefinition)
		{
			ITypeDefOrRef inativeImporter = SharedState.Importer.ImportCommonType<INativeFormatImporter>();
			ITypeDefOrRef layoutInfoType = SharedState.Importer.ImportCommonType<LayoutInfo>();

			MethodDefinition constructor = SharedState.TypeDictionary["NativeFormatImporter"].Methods
				.Single(m => m.IsConstructor && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.Name == "LayoutInfo");

			MethodDefinition method = factoryDefinition.AddMethod("CreateNativeFormatImporter", InterfaceOverrideAttributes, inativeImporter);
			method.AddParameter("layout", layoutInfoType);

			CilInstructionCollection processor = method.CilMethodBody!.Instructions;
			processor.Add(CilOpCodes.Ldarg_1); //layout
			processor.Add(CilOpCodes.Newobj, constructor);
			processor.Add(CilOpCodes.Ret);
		}
	}
}
