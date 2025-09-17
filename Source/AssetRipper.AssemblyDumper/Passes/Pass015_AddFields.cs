namespace AssetRipper.AssemblyDumper.Passes;

public static class Pass015_AddFields
{
	public static void DoPass()
	{
		foreach (ClassGroup group in SharedState.Instance.ClassGroups.Values)
		{
			foreach (GeneratedClassInstance instance in group.Instances)
			{
				ProcessNodeInformation(instance.Class, instance.Type, instance.VersionRange.Start);
			}
		}
		foreach (SubclassGroup group in SharedState.Instance.SubclassGroups.Values)
		{
			foreach (GeneratedClassInstance instance in group.Instances)
			{
				ProcessNodeInformation(instance.Class, instance.Type, instance.VersionRange.Start);
			}
		}
	}

	private static void ProcessNodeInformation(UniversalClass unityClass, TypeDefinition type, UnityVersion version)
	{
		if (unityClass.EditorRootNode == null && unityClass.ReleaseRootNode == null)
		{
			return; //No fields.
		}

		GetFieldNodeSets(unityClass, out List<UniversalNode> releaseOnly, out List<UniversalNode> editorOnly, out List<(UniversalNode, UniversalNode)> releaseAndEditor);

		foreach (UniversalNode releaseOnlyField in releaseOnly)
		{
			TypeSignature releaseOnlyFieldType = GenericTypeResolver.ResolveNode(releaseOnlyField, version);
			type.AddFieldForNode(releaseOnlyField, releaseOnlyFieldType);
		}

		foreach (UniversalNode editorOnlyField in editorOnly)
		{
			TypeSignature editorOnlyFieldType = GenericTypeResolver.ResolveNode(editorOnlyField, version);
			type.AddFieldForNode(editorOnlyField, editorOnlyFieldType);
		}

		foreach ((UniversalNode releaseField, _) in releaseAndEditor)
		{
			TypeSignature fieldType = GenericTypeResolver.ResolveNode(releaseField, version);
			type.AddFieldForNode(releaseField, fieldType);
		}
	}

	private static void GetFieldNodeSets(UniversalClass unityClass, out List<UniversalNode> releaseOnly, out List<UniversalNode> editorOnly, out List<(UniversalNode, UniversalNode)> releaseAndEditor)
	{
		List<UniversalNode> editorNodes = unityClass.GetNonInheritedEditorNodes();
		List<UniversalNode> releaseNodes = unityClass.GetNonInheritedReleaseNodes();

		Dictionary<string, UniversalNode> releaseFields = releaseNodes.ToDictionary(x => x.Name, x => x);
		Dictionary<string, UniversalNode> editorFields = editorNodes.ToDictionary(x => x.Name, x => x);

		List<UniversalNode> releaseOnlyResult = releaseNodes.Where(node => !editorFields.ContainsKey(node.Name)).ToList();
		//Need to use a result local field here becuase out parameters can't be used in lambda expressions
		editorOnly = editorNodes.Where(node => !releaseFields.ContainsKey(node.Name)).ToList();

		releaseAndEditor = releaseNodes.
			Where(anyRelease => !releaseOnlyResult.Contains(anyRelease)).
			Select(releaseWithEditor => (releaseWithEditor, editorFields[releaseWithEditor.Name])).
			ToList();

		releaseOnly = releaseOnlyResult;
	}

	private static List<UniversalNode> GetNonInheritedEditorNodes(this UniversalClass unityClass)
	{
		List<UniversalNode> editorNodes = unityClass.EditorRootNode?.SubNodes ?? new();
		return editorNodes.Where(node => !IsFieldInBaseType(unityClass, node.Name)).ToList();
	}

	private static List<UniversalNode> GetNonInheritedReleaseNodes(this UniversalClass unityClass)
	{
		List<UniversalNode> releaseNodes = unityClass.ReleaseRootNode?.SubNodes ?? new();
		return releaseNodes.Where(node => !IsFieldInBaseType(unityClass, node.Name)).ToList();
	}

	private static void AddFieldForNode(this TypeDefinition type, UniversalNode mainNode, TypeSignature fieldType)
	{
		FieldDefinition fieldDefinition = new FieldDefinition(mainNode.Name, FieldAttributes.Assembly, new FieldSignature(fieldType));
		type.Fields.Add(fieldDefinition);
		fieldDefinition.AddDebuggerBrowsableNeverAttribute();
	}

	private static bool IsFieldInBaseType(UniversalClass unityClass, string fieldName)
	{
		UniversalClass? baseType = unityClass.BaseClass;
		while (baseType is not null)
		{

			if (baseType.EditorRootNode?.SubNodes.Any(n => n.Name == fieldName) == true)
			{
				return true;
			}

			if (baseType.ReleaseRootNode?.SubNodes.Any(n => n.Name == fieldName) == true)
			{
				return true;
			}

			baseType = baseType.BaseClass;
		}

		return false;
	}
}