using AssetRipper.AssemblyDumper.Documentation;
using AssetRipper.AssemblyDumper.Enums;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass999_Documentation
{
	public static void DoPass()
	{
		InjectedDocumenter.AddDocumentation();

		foreach (ClassGroupBase group in SharedState.Instance.AllGroups)
		{
			InterfaceDocumenter.AddInterfaceDocumentation(group);

			foreach (GeneratedClassInstance instance in group.Instances)
			{
				ClassDocumenter.AddClassDocumentation(instance);
			}
		}

		IdEnumDocumenter.AddIdEnumDocumentation();

		foreach ((TypeDefinition type, EnumDefinitionBase definition) in Pass040_AddEnums.EnumDictionary.Values.Distinct())
		{
			EnumTypeDocumenter.AddEnumTypeDocumentation(type, definition);
		}

		DocumentationHandler.MakeDocumentationFile();
	}
}
