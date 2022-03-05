using AssemblyDumper.Utils;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Interfaces;

namespace AssemblyDumper.Passes
{
	public static class Pass300_ImplementHasNameInterface
	{
		const MethodAttributes InterfacePropertyImplementationAttributes =
			MethodAttributes.Public |
			MethodAttributes.Final |
			MethodAttributes.HideBySig |
			MethodAttributes.SpecialName |
			MethodAttributes.NewSlot | 
			MethodAttributes.Virtual;
		const string FieldName = "m_Name";

		public static void DoPass()
		{
			System.Console.WriteLine("Pass 300: Implement the Has Name Interface");
			ITypeDefOrRef hasName = SharedState.Importer.ImportCommonType<IHasName>();
			foreach(TypeDefinition type in SharedState.TypeDictionary.Values)
			{
				if (type.TryGetFieldByName(FieldName, out FieldDefinition? field) && IsCompatible(field))
				{
					type.Interfaces.Add(new InterfaceImplementation(hasName));
					type.ImplementStringProperty("Name", InterfacePropertyImplementationAttributes, field);
				}
			}
		}

		private static bool IsCompatible(FieldDefinition field)
		{
			return field.Signature?.FieldType.Name == Pass002_RenameSubnodes.Utf8StringName;
		}
	}
}
