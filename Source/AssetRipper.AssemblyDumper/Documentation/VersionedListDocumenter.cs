using AssetRipper.AssemblyDumper.Utils;

namespace AssetRipper.AssemblyDumper.Documentation;

internal static class VersionedListDocumenter
{
	public static void AddList<T>(TypeDefinition type, VersionedList<T> versionedList, string prefix)
	{
		foreach (string? str in GetContentFromList(versionedList, prefix))
		{
			AddMultiLineString(type, str);
		}
	}

	public static void AddList<T>(FieldDefinition field, VersionedList<T> versionedList, string prefix)
	{
		foreach (string? str in GetContentFromList(versionedList, prefix))
		{
			AddMultiLineString(field, str);
		}
	}

	public static void AddList<T>(PropertyDefinition property, VersionedList<T> versionedList, string prefix)
	{
		foreach (string? str in GetContentFromList(versionedList, prefix))
		{
			AddMultiLineString(property, str);
		}
	}

	public static void AddSet<T>(TypeDefinition type, VersionedList<T> versionedList, string prefix)
	{
		foreach (string? str in GetContentFromSet(versionedList, prefix))
		{
			AddMultiLineString(type, str);
		}
	}

	public static void AddSet<T>(FieldDefinition field, VersionedList<T> versionedList, string prefix)
	{
		foreach (string? str in GetContentFromSet(versionedList, prefix))
		{
			AddMultiLineString(field, str);
		}
	}

	public static void AddSet<T>(PropertyDefinition property, VersionedList<T> versionedList, string prefix)
	{
		foreach (string? str in GetContentFromSet(versionedList, prefix))
		{
			AddMultiLineString(property, str);
		}
	}

	private static void AddMultiLineString(TypeDefinition type, string? str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return;
		}
		else if (!str.Contains('\n'))
		{
			DocumentationHandler.AddTypeDefinitionLine(type, str.Trim().EscapeXml());
		}
		else
		{
			foreach (string line in str.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
			{
				DocumentationHandler.AddTypeDefinitionLine(type, line.EscapeXml());
			}
		}
	}

	private static void AddMultiLineString(FieldDefinition field, string? str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return;
		}
		else if (!str.Contains('\n'))
		{
			DocumentationHandler.AddFieldDefinitionLine(field, str.Trim().EscapeXml());
		}
		else
		{
			foreach (string line in str.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
			{
				DocumentationHandler.AddFieldDefinitionLine(field, line.EscapeXml());
			}
		}
	}

	private static void AddMultiLineString(PropertyDefinition property, string? str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return;
		}
		else if (!str.Contains('\n'))
		{
			DocumentationHandler.AddPropertyDefinitionLine(property, str.Trim().EscapeXml());
		}
		else
		{
			foreach (string line in str.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
			{
				DocumentationHandler.AddPropertyDefinitionLine(property, line.EscapeXml());
			}
		}
	}

	private static IEnumerable<string?> GetContentFromList<T>(VersionedList<T> versionedList, string prefix)
	{
		if (versionedList.Count == 1)
		{
			yield return MaybePrefixString(versionedList[0].Value?.ToString(), prefix);
		}
		else
		{
			Dictionary<StringWrapper, LinkedList<UnityVersionRange>> summaries = new();
			for (int i = 0; i < versionedList.Count; i++)
			{
				string? summary = versionedList[i].Value?.ToString();
				if (!summaries.TryGetValue(summary, out LinkedList<UnityVersionRange>? rangeList))
				{
					rangeList = new();
					summaries.Add(summary, rangeList);
				}
				rangeList.AddLast(versionedList.GetRange(i));
			}
			if (summaries.Count == 0)
			{
			}
			else if (summaries.Count == 1)
			{
				yield return MaybePrefixString(summaries.Single().Key.String, prefix);
			}
			else
			{
				foreach ((StringWrapper summary, LinkedList<UnityVersionRange> list) in summaries)
				{
					yield return $"{prefix}{list.GetUnionedRanges().GetString(SharedState.Instance.MinVersion)}";
					yield return summary.String ?? "null";
				}
			}
		}
	}

	private static IEnumerable<string?> GetContentFromSet<T>(VersionedList<T> versionedList, string prefix)
	{
		if (versionedList.Count == 1)
		{
			yield return MaybePrefixString(versionedList[0].Value?.ToString(), prefix);
		}
		else
		{
			Dictionary<StringWrapper, LinkedList<UnityVersionRange>> summaries = new();
			for (int i = 0; i < versionedList.Count; i++)
			{
				string? summary = versionedList[i].Value?.ToString();
				if (!summaries.TryGetValue(summary, out LinkedList<UnityVersionRange>? rangeList))
				{
					rangeList = new();
					summaries.Add(summary, rangeList);
				}
				rangeList.AddLast(versionedList.GetRange(i));
			}
			if (summaries.Count == 0)
			{
			}
			else if (summaries.Count == 1)
			{
				yield return MaybePrefixString(summaries.Single().Key.String, prefix);
			}
			else
			{
				foreach ((StringWrapper summary, LinkedList<UnityVersionRange> list) in summaries)
				{
					yield return MaybePrefixString(summary.String, prefix);
				}
			}
		}
	}

	[return: NotNullIfNotNull(nameof(str))]
	private static string? MaybePrefixString(string? str, string prefix)
	{
		return str is null ? null : $"{prefix}{str}";
	}

	private readonly record struct StringWrapper(string? String)
	{
		public static implicit operator string?(StringWrapper wrapper) => wrapper.String;
		public static implicit operator StringWrapper(string? @string) => new StringWrapper(@string);
	}
}