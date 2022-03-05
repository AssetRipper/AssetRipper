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
		const string PropertyName = "MainObjectFileID";
		const string FieldName = "m_" + PropertyName;

		public static void DoPass()
		{
			Console.WriteLine("Pass 361: Implement Native Format Importer Interface");
			ITypeDefOrRef nativeImporterInterface = SharedState.Importer.ImportCommonType<INativeFormatImporter>();
			if (SharedState.TypeDictionary.TryGetValue("NativeFormatImporter", out TypeDefinition? type))
			{
				type.Interfaces.Add(new InterfaceImplementation(nativeImporterInterface));
				FieldDefinition? field = type.TryGetFieldByName(FieldName);
				type.ImplementFullProperty(PropertyName, InterfacePropertyImplementationAttributes, SystemTypeGetter.Int64, field);
			}
			else
			{
				throw new Exception("NativeFormatImporter not found");
			}
		}
	}
}
