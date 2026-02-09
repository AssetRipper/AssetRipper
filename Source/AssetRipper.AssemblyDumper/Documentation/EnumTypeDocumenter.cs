using AssetRipper.AssemblyDumper.Enums;
using AssetRipper.AssemblyDumper.Types;
using AssetRipper.AssemblyDumper.Utils;
using AssetRipper.DocExtraction.DataStructures;

namespace AssetRipper.AssemblyDumper.Documentation;

internal static class EnumTypeDocumenter
{
	public static void AddEnumTypeDocumentation(TypeDefinition type, EnumDefinitionBase definition)
	{
		string fullNameList = string.Join(", ", definition.FullNames.Select(fullName => $"\"{XmlUtils.EscapeXmlInvalidCharacters(fullName)}\""));
		DocumentationHandler.AddTypeDefinitionLine(type, $"Full Names: {fullNameList}");
		if (definition is SingleEnumDefinition singleEnumDefinition)
		{
			EnumHistory history = singleEnumDefinition.History;

			if (!string.IsNullOrEmpty(history.InjectedDocumentation))
			{
				DocumentationHandler.AddTypeDefinitionLine(type, "Summary: " + history.InjectedDocumentation);
			}

			VersionedListDocumenter.AddSet(type, history.DocumentationString, "Summary: ");
			VersionedListDocumenter.AddList(type, history.ObsoleteMessage, "Obsolete Message: ");

			UnityVersion minimumVersion = definition.MinimumVersion;
			foreach ((string memberName, EnumMemberHistory memberHistory) in history.Members)
			{
				if (memberHistory.TryGetUniqueValue(out _, out IEnumerable<KeyValuePair<string, long>>? pairs))
				{
					FieldDefinition field = type.GetFieldByName(memberName);
					if (!string.IsNullOrEmpty(memberHistory.InjectedDocumentation))
					{
						DocumentationHandler.AddFieldDefinitionLine(field, "Summary: " + memberHistory.InjectedDocumentation);
					}
					VersionedListDocumenter.AddSet(field, memberHistory.DocumentationString, "Summary: ");
					VersionedListDocumenter.AddList(field, memberHistory.ObsoleteMessage, "Obsolete Message: ");
					DocumentationHandler.AddFieldDefinitionLine(field, memberHistory.GetVersionRange().GetUnionedRanges().GetString(minimumVersion));
				}
				else
				{
					foreach ((string fieldName, long value) in pairs)
					{
						FieldDefinition field = type.GetFieldByName(fieldName);
						if (!string.IsNullOrEmpty(memberHistory.InjectedDocumentation))
						{
							DocumentationHandler.AddFieldDefinitionLine(field, "Summary: " + memberHistory.InjectedDocumentation);
						}
						VersionedListDocumenter.AddSet(field, memberHistory.DocumentationString, "Summary: ");
						VersionedListDocumenter.AddList(field, memberHistory.ObsoleteMessage, "Obsolete Message: ");
						DocumentationHandler.AddFieldDefinitionLine(field,
							GetVersionRange(memberHistory.Exists, memberHistory.Value, value).GetUnionedRanges().GetString(minimumVersion));
					}
				}
			}

			//SharedState.Instance.MinVersion isn't used here because enums don't have the version type stripped.
			DocumentationHandler.AddTypeDefinitionLine(type, history.GetVersionRange().GetUnionedRanges().GetString(SharedState.Instance.MinSourceVersion));
		}
	}

	private static IEnumerable<UnityVersionRange> GetVersionRange(VersionedList<bool> existence, VersionedList<long> values, long value)
	{
		int existenceIndex = 0;
		int valuesIndex = 0;
		while (existenceIndex < existence.Count && valuesIndex < values.Count)
		{
			if (!existence[existenceIndex].Value) //Field does not exist in this range.
			{
				existenceIndex++;
				continue;
			}
			if (values[valuesIndex].Value != value) //Value does not match in this range.
			{
				valuesIndex++;
				continue;
			}

			UnityVersionRange valuesRange = values.GetRange(valuesIndex);
			UnityVersionRange existenceRange = existence.GetRange(existenceIndex);
			if (valuesRange.End <= existenceRange.Start) //Value range is lagging behind existence range.
			{
				valuesIndex++;
				continue;
			}
			if (existenceRange.End <= valuesRange.Start) //Existence range is lagging behind value range.
			{
				existenceIndex++;
				continue;
			}

			//At this point, the value range intersects the existence range, the field exists, and the value matches.

			UnityVersionRange intersection = valuesRange.MakeIntersection(existenceRange);
			yield return intersection;

			if (valuesRange.End <= existenceRange.End) //Existence range may still have space to intersect with the next value range.
			{
				valuesIndex++;
			}
			else // Vice versa.
			{
				existenceIndex++;
			}
		}
	}
}
