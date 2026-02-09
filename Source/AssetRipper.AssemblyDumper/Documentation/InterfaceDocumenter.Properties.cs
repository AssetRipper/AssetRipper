using AssetRipper.DocExtraction.Extensions;

namespace AssetRipper.AssemblyDumper.Documentation;

internal static partial class InterfaceDocumenter
{
	private static void AddInterfacePropertyDocumentation(ClassGroupBase group)
	{
		foreach (InterfaceProperty interfaceProperty in group.InterfaceProperties)
		{
			if (!interfaceProperty.ReleaseOnlyRange.IsEmpty())
			{
				if (interfaceProperty.ReleaseOnlyMethod is null)
				{
					DocumentationHandler.AddPropertyDefinitionLine(interfaceProperty, "Release Only");
				}
				else
				{
					string versionString = interfaceProperty.ReleaseOnlyRange.GetString(interfaceProperty.Group.MinimumVersion);
					DocumentationHandler.AddPropertyDefinitionLine(interfaceProperty, $"Sometimes Release Only: {versionString}");
					DocumentationHandler.AddMethodDefinitionLine(interfaceProperty.ReleaseOnlyMethod, versionString);
				}
			}

			if (!interfaceProperty.EditorOnlyRange.IsEmpty())
			{
				if (interfaceProperty.EditorOnlyMethod is null)
				{
					DocumentationHandler.AddPropertyDefinitionLine(interfaceProperty, "Editor Only");
				}
				else
				{
					string versionString = interfaceProperty.EditorOnlyRange.GetString(interfaceProperty.Group.MinimumVersion);
					DocumentationHandler.AddPropertyDefinitionLine(interfaceProperty, $"Sometimes Editor Only: {versionString}");
					DocumentationHandler.AddMethodDefinitionLine(interfaceProperty.EditorOnlyMethod, versionString);
				}
			}

			if (interfaceProperty.HasMethod is not null)
			{
				string versionString = interfaceProperty.PresentRange.GetString(group.MinimumVersion);
				DocumentationHandler.AddMethodDefinitionLine(interfaceProperty.HasMethod, versionString);
				DocumentationHandler.AddPropertyDefinitionLine(interfaceProperty, versionString);
			}
			else
			{
				DocumentationHandler.AddPropertyDefinitionLine(interfaceProperty, interfaceProperty.Definition.IsValueType() ? "Not absent" : "Not null");
			}
		}
	}
}
