using AssetRipper.AssemblyDumper.Documentation;
using AssetRipper.Assets.Metadata;

namespace AssetRipper.AssemblyDumper.Passes;

public static class Pass500_PPtrFixes
{
	public static void DoPass()
	{
		foreach (SubclassGroup group in SharedState.Instance.SubclassGroups.Values)
		{
			if (group.IsPPtr)
			{
				foreach (GeneratedClassInstance instance in group.Instances)
				{
					TypeDefinition parameterType = Pass080_PPtrConversions.PPtrsToParameters[instance.Type];

					//DebuggerDisplay
					{
						string parameterTypeName = group.Name.Substring("PPtr_".Length);
						instance.Type.AddDebuggerDisplayAttribute($"{parameterTypeName} FileID: {{FileID}} PathID: {{PathID}}");
					}

					//Documentation
					{
						DocumentationHandler.AddTypeDefinitionLine(instance.Type, $"{SeeXmlTagGenerator.MakeCRef(typeof(PPtr))} for {SeeXmlTagGenerator.MakeCRef(parameterType)}");
					}
				}
			}
		}
	}
}
