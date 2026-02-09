namespace AssetRipper.AssemblyDumper.Documentation;

internal static class IdEnumDocumenter
{
	public static void AddIdEnumDocumentation()
	{
		foreach ((FieldDefinition field, ClassGroup group) in Passes.Pass556_CreateClassIDTypeEnum.FieldGroupDictionary)
		{
			DocumentationHandler.AddFieldDefinitionLine(field, SeeXmlTagGenerator.MakeCRef(group.Interface));
			DocumentationHandler.AddFieldDefinitionLine(field, SeeXmlTagGenerator.MakeCRef(group.GetOrCreateMainClass()));
			DocumentationHandler.AddFieldDefinitionLine(field, InterfaceDocumenter.GetUnityVersionString(group));
		}
	}
}
