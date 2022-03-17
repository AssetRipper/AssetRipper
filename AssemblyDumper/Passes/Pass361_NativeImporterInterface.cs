using AssemblyDumper.Utils;
using AssetRipper.Core.Classes.Meta.Importers;

namespace AssemblyDumper.Passes
{
	public static class Pass361_NativeImporterInterface
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
			Console.WriteLine("Pass 361: Implement Mono and Native Format Importer Interfaces");
			ImplementNativeImporter();
			ImplementMonoImporter();
		}

		private static void ImplementNativeImporter()
		{
			ITypeDefOrRef importerInterface = SharedState.Importer.ImportCommonType<INativeFormatImporter>();
			if (SharedState.TypeDictionary.TryGetValue("NativeFormatImporter", out TypeDefinition? type))
			{
				type.Interfaces.Add(new InterfaceImplementation(importerInterface));
				FieldDefinition? field = type.TryGetFieldByName("m_MainObjectFileID");
				type.ImplementFullProperty(nameof(INativeFormatImporter.MainObjectFileID), InterfacePropertyImplementationAttributes, SystemTypeGetter.Int64, field);
			}
			else
			{
				throw new Exception("NativeFormatImporter not found");
			}
		}

		private static void ImplementMonoImporter()
		{
			ITypeDefOrRef importerInterface = SharedState.Importer.ImportCommonType<IMonoImporter>();
			if (SharedState.TypeDictionary.TryGetValue("MonoImporter", out TypeDefinition? type))
			{
				type.Interfaces.Add(new InterfaceImplementation(importerInterface));
				FieldDefinition? field = type.TryGetFieldByName("executionOrder");
				type.ImplementFullProperty(nameof(IMonoImporter.ExecutionOrder), InterfacePropertyImplementationAttributes, SystemTypeGetter.Int16, field);
			}
			else
			{
				throw new Exception("MonoImporter not found");
			}
		}
	}
}
