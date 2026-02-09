using AssetRipper.DocExtraction.DataStructures;

namespace AssetRipper.AssemblyDumper.Documentation;

internal static partial class InterfaceDocumenter
{
	public static void AddInterfaceDocumentation(ClassGroupBase group)
	{
		AddDocumentationFromHistory(group);
		AddInterfaceTypeDocumentation(group);
		AddInterfacePropertyDocumentation(group);
	}

	private static void AddDocumentationFromHistory(ClassGroupBase group)
	{
		if (group.History is not null)
		{
			VersionedListDocumenter.AddSet(group.Interface, group.History.NativeName, "Native Name: ");
			VersionedListDocumenter.AddSet(group.Interface, group.History.DocumentationString, "Summary: ");
			VersionedListDocumenter.AddList(group.Interface, group.History.ObsoleteMessage, "Obsolete Message: ");
		}

		foreach (InterfaceProperty interfaceProperty in group.InterfaceProperties)
		{
			if (interfaceProperty.History is not null)
			{
				AddPropertyDocumentationFromHistory(interfaceProperty.Definition, interfaceProperty.History);

				if (interfaceProperty.SpecialDefinition is not null)
				{
					AddPropertyDocumentationFromHistory(interfaceProperty.SpecialDefinition, interfaceProperty.History);
				}
			}
			if (interfaceProperty.SpecialDefinition is not null)
			{
				if (interfaceProperty.HasEnumVariant)
				{
					DocumentationHandler.AddPropertyDefinitionLine(interfaceProperty.Definition, "Enum variant available.");
				}
				else
				{
					DocumentationHandler.AddPropertyDefinitionLine(interfaceProperty.Definition, "PPtr variant available.");
				}
			}
		}
	}

	private static void AddPropertyDocumentationFromHistory(PropertyDefinition property, DataMemberHistory history)
	{
		VersionedListDocumenter.AddSet(property, history.NativeName, "Native Name: ");
		VersionedListDocumenter.AddList(property, history.TypeFullName, "Managed Type: ");
		VersionedListDocumenter.AddSet(property, history.DocumentationString, "Summary: ");
		VersionedListDocumenter.AddList(property, history.ObsoleteMessage, "Obsolete Message: ");
	}
}
