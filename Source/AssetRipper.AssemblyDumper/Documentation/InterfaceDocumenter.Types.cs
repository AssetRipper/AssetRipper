namespace AssetRipper.AssemblyDumper.Documentation;

internal static partial class InterfaceDocumenter
{
	private static void AddInterfaceTypeDocumentation(ClassGroupBase group)
	{
		if (group is ClassGroup classGroup)
		{
			DocumentationHandler.AddTypeDefinitionLine(group.Interface, $"Interface for the {string.Join(", ", classGroup.Names)} classes.");
			HashSet<int> typeIDs = classGroup.Classes.Select(c => c.OriginalTypeID).ToHashSet();
			if (typeIDs.Count == 1)
			{
				DocumentationHandler.AddTypeDefinitionLine(group.Interface, $"Type ID: {typeIDs.First()}");
			}
			else
			{
				DocumentationHandler.AddTypeDefinitionLine(group.Interface, $"Type IDs: {string.Join(", ", typeIDs)}");
			}
			if (group.Types.All(t => t.IsAbstract))
			{
				DocumentationHandler.AddTypeDefinitionLine(group.Interface, "Abstract");
			}
			else if (group.Types.Any(t => t.IsAbstract))
			{
				string rangeString = group.Instances
					.Where(i => i.Type.IsAbstract)
					.Select(i => i.VersionRange)
					.GetUnionedRanges()
					.GetString(SharedState.Instance.MinVersion);
				DocumentationHandler.AddTypeDefinitionLine(group.Interface, $"Abstract: {rangeString}");
			}
		}
		else
		{
			DocumentationHandler.AddTypeDefinitionLine(group.Interface, $"Interface for the {group.Name} classes.");
		}
		if (group.Instances.All(instance => instance.Class.IsReleaseOnly))
		{
			DocumentationHandler.AddTypeDefinitionLine(group.Interface, "Release Only");
		}
		if (group.Instances.All(instance => instance.Class.IsEditorOnly))
		{
			DocumentationHandler.AddTypeDefinitionLine(group.Interface, "Editor Only");
		}
		DocumentationHandler.AddTypeDefinitionLine(group.Interface, GetSerializedVersionString(group));
		DocumentationHandler.AddTypeDefinitionLine(group.Interface, GetUnityVersionString(group));
	}

	internal static string GetUnityVersionString(ClassGroupBase group)
	{
		return group.Instances
			.Select(instance => instance.VersionRange)
			.GetUnionedRanges()
			.GetString(SharedState.Instance.MinVersion);
	}

	private static string GetSerializedVersionString(ClassGroupBase group)
	{
		group.GetSerializedVersions(out int minimum, out int maximum);
		return minimum == maximum
			? $"Serialized Version: {minimum}"
			: $"Serialized Versions: {minimum} to {maximum}";
	}
}
