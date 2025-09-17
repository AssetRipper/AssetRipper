using AssetRipper.AssemblyDumper.Utils;
using AssetRipper.DocExtraction.MetaData;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.AssemblyDumper.Documentation;

internal static class ClassDocumenter
{
	public static void AddClassDocumentation(GeneratedClassInstance instance)
	{
		AddDocumentationFromHistory(instance);

		AddClassTypeDocumentation(instance);

		if (instance.Class.EditorRootNode is not null || instance.Class.ReleaseRootNode is not null)
		{
			foreach (ClassProperty classProperty in instance.Properties)
			{
				AddPropertyDocumentation(instance, classProperty);
			}
		}
	}

	private static void AddClassTypeDocumentation(GeneratedClassInstance instance)
	{
		if (instance.Group is ClassGroup)
		{
			DocumentationHandler.AddTypeDefinitionLine(instance.Type, $"Type ID: {instance.Class.OriginalTypeID}");
		}

		if (instance.Type.Name != instance.Class.OriginalName)
		{
			DocumentationHandler.AddTypeDefinitionLine(instance.Type, $"Original Name: \"{instance.Class.OriginalName.EscapeXml()}\"");
		}

		DocumentationHandler.AddTypeDefinitionLine(instance.Type, $"Serialized Version: {instance.GetSerializedVersion()}");

		if (instance.Class.IsReleaseOnly)
		{
			DocumentationHandler.AddTypeDefinitionLine(instance.Type, "Release Only");
		}

		if (instance.Class.IsEditorOnly)
		{
			DocumentationHandler.AddTypeDefinitionLine(instance.Type, "Editor Only");
		}

		if (instance.Class.IsStripped)
		{
			DocumentationHandler.AddTypeDefinitionLine(instance.Type, "Stripped");
		}
		DocumentationHandler.AddTypeDefinitionLine(instance.Type, UnityVersionRangeUtils.GetUnityVersionRangeString(instance.VersionRange));
	}

	private static void AddPropertyDocumentation(GeneratedClassInstance instance, ClassProperty classProperty)
	{
		if (classProperty.BackingField?.Name is null)
		{
			DocumentationHandler.AddPropertyDefinitionLine(classProperty, "Not present in this version range");
			return;
		}

		UniversalNode? releaseNode;
		UniversalNode? editorNode;
		if (classProperty.IsInjected)
		{
			releaseNode = null;
			editorNode = null;
		}
		else
		{
			string fieldName = classProperty.BackingField.Name;

			releaseNode = instance.GetReleaseFieldByName(fieldName);
			editorNode = instance.GetEditorFieldByName(fieldName);
			UniversalNode mainNode = releaseNode ?? editorNode ?? throw new Exception($"In {instance.Name}, could not find nodes for {fieldName}");

			if (mainNode.Name != mainNode.OriginalName)
			{
				DocumentationHandler.AddPropertyDefinitionLine(classProperty, $"Original field name: \"{XmlUtils.EscapeXmlInvalidCharacters(mainNode.OriginalName)}\"");
			}

			if (mainNode.TypeName != mainNode.OriginalTypeName)
			{
				DocumentationHandler.AddPropertyDefinitionLine(classProperty, $"Original type: \"{XmlUtils.EscapeXmlInvalidCharacters(mainNode.OriginalTypeName)}\"");
			}
		}

		if (classProperty.HasEnumVariant)
		{
			DocumentationHandler.AddPropertyDefinitionLineNotSpecial(classProperty, "Enum variant available.");
		}
		else if (classProperty.SpecialDefinition is not null)
		{
			DocumentationHandler.AddPropertyDefinitionLineNotSpecial(classProperty, "PPtr variant available.");
		}

		if (!classProperty.IsInjected)
		{
			DocumentationHandler.AddPropertyDefinitionLine(classProperty, releaseNode is null ? "Editor Only" : $"Release Flags: {GetMetaFlagString(releaseNode.MetaFlag)}");
			DocumentationHandler.AddPropertyDefinitionLine(classProperty, editorNode is null ? "Release Only" : $"Editor Flags: {GetMetaFlagString(editorNode.MetaFlag)}");
		}
	}

	private static UniversalNode? GetReleaseFieldByName(this GeneratedClassInstance instance, string fieldName)
	{
		return instance.Class.ReleaseRootNode?.SubNodes.SingleOrDefault(n => n.Name == fieldName);
	}

	private static UniversalNode? GetEditorFieldByName(this GeneratedClassInstance instance, string fieldName)
	{
		return instance.Class.EditorRootNode?.SubNodes.SingleOrDefault(n => n.Name == fieldName);
	}

	private static string GetMetaFlagString(TransferMetaFlags flag)
	{
		return string.Join(" | ", flag.Split());
	}

	private static void AddDocumentationFromHistory(GeneratedClassInstance instance)
	{
		if (instance.History is not null)
		{
			VersionedListDocumenter.AddSet(instance.Type, instance.History.NativeName.GetSubList(instance.VersionRange), "Native Name: ");
			VersionedListDocumenter.AddSet(instance.Type, instance.History.DocumentationString.GetSubList(instance.VersionRange), "Summary: ");
			VersionedListDocumenter.AddList(instance.Type, instance.History.ObsoleteMessage.GetSubList(instance.VersionRange), "Obsolete Message: ");
		}

		foreach (ClassProperty classProperty in instance.Properties)
		{
			if (classProperty.History is not null)
			{
				VersionedList<string> nativeNameSubList = classProperty.History.NativeName.GetSubList(instance.VersionRange);
				VersionedList<FullNameRecord> managedTypeSubList = classProperty.History.TypeFullName.GetSubList(instance.VersionRange);
				VersionedList<string> docStringSubList = classProperty.History.DocumentationString.GetSubList(instance.VersionRange);
				VersionedList<string> obsoleteMessageSubList = classProperty.History.ObsoleteMessage.GetSubList(instance.VersionRange);

				VersionedListDocumenter.AddSet(classProperty.Definition, nativeNameSubList, "Native Name: ");
				VersionedListDocumenter.AddList(classProperty.Definition, managedTypeSubList, "Managed Type: ");
				VersionedListDocumenter.AddSet(classProperty.Definition, docStringSubList, "Summary: ");
				VersionedListDocumenter.AddList(classProperty.Definition, obsoleteMessageSubList, "Obsolete Message: ");

				if (classProperty.SpecialDefinition is not null)
				{
					VersionedListDocumenter.AddSet(classProperty.SpecialDefinition, nativeNameSubList, "Native Name: ");
					VersionedListDocumenter.AddList(classProperty.SpecialDefinition, managedTypeSubList, "Managed Type: ");
					VersionedListDocumenter.AddSet(classProperty.SpecialDefinition, docStringSubList, "Summary: ");
					VersionedListDocumenter.AddList(classProperty.SpecialDefinition, obsoleteMessageSubList, "Obsolete Message: ");
				}

				if (classProperty.HasBackingFieldInDeclaringType)
				{
					VersionedListDocumenter.AddSet(classProperty.BackingField, nativeNameSubList, "Native Name: ");
					VersionedListDocumenter.AddList(classProperty.BackingField, managedTypeSubList, "Managed Type: ");
					VersionedListDocumenter.AddSet(classProperty.BackingField, docStringSubList, "Summary: ");
					VersionedListDocumenter.AddList(classProperty.BackingField, obsoleteMessageSubList, "Obsolete Message: ");
				}
			}
		}
	}
}
