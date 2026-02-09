using AssetRipper.DocExtraction.DataStructures;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass054_AssignPropertyHistories
{
	public static void DoPass()
	{
		foreach (ClassGroupBase group in SharedState.Instance.AllGroups)
		{
			HashSet<GeneratedClassInstance> instancesWithHistoriesNotFromBase = new();
			foreach (GeneratedClassInstance instance in group.Instances)
			{
				ComplexTypeHistory? history = TryGetHistoryInThisOrBase(instance, out bool usedBaseClass);
				if (history is null)
				{
					continue;
				}

				if (!usedBaseClass)
				{
					instancesWithHistoriesNotFromBase.Add(instance);
				}

				IReadOnlyDictionary<string, DataMemberHistory> members = history.GetAllMembers(instance.VersionRange.Start, SharedState.Instance.HistoryFile);
				foreach (ClassProperty classProperty in instance.Properties)
				{
					SetHistory(classProperty, members);
				}
			}
			foreach (InterfaceProperty interfaceProperty in group.InterfaceProperties)
			{
				interfaceProperty.History = interfaceProperty.DetermineHistoryFromImplementations(instancesWithHistoriesNotFromBase);
			}
		}
	}

	private static ComplexTypeHistory? TryGetHistoryInThisOrBase(GeneratedClassInstance instance, out bool usedBaseClass)
	{
		if (instance.History is not null)
		{
			usedBaseClass = false;
			return instance.History;
		}
		usedBaseClass = true;
		GeneratedClassInstance? current = instance.Base;
		while (current is not null)
		{
			if (current.History is not null)
			{
				return current.History;
			}
			current = current.Base;
		}
		return null;
	}

	private static void SetHistory(ClassProperty classProperty, IReadOnlyDictionary<string, DataMemberHistory> dictionary)
	{
		if (classProperty.OriginalFieldName is not null && TryGetHistoryFromOriginalName(classProperty.OriginalFieldName, dictionary, out DataMemberHistory? history))
		{
		}
		else if (classProperty.BackingField is not null)
		{
			if (dictionary.TryGetValue(classProperty.BackingField.Name!, out history))
			{
			}
			else
			{
				history = dictionary.FirstOrDefault(pair => HistoryIsApplicable(pair.Value, classProperty.BackingField)).Value;
			}
		}
		else
		{
			history = null;
		}
		classProperty.History = history;
	}

	private static bool TryGetHistoryFromOriginalName(string originalName, IReadOnlyDictionary<string, DataMemberHistory> dictionary, [NotNullWhen(true)] out DataMemberHistory? history)
	{
		if (dictionary.TryGetValue(originalName, out history))
		{
			return true;
		}
		else
		{
			history = dictionary.FirstOrDefault(pair => HistoryIsApplicable(pair.Value, originalName)).Value;
			return history is not null;
		}
	}

	private static bool HistoryIsApplicable(DataMemberHistory history, string originalName)
	{
		string historyNameNormalized = ValidNameGenerator.GetValidFieldName(history.Name).ToLowerInvariant();
		string fieldName = originalName.ToLowerInvariant();
		return historyNameNormalized == fieldName;
	}
	private static bool HistoryIsApplicable(DataMemberHistory history, FieldDefinition field)
	{
		string historyNameNormalized = ValidNameGenerator.GetValidFieldName(history.Name).ToLowerInvariant();
		string fieldName = field.Name!.ToString().ToLowerInvariant();
		if (historyNameNormalized == fieldName)
		{
			return true;
		}
		foreach (string? nativeName in history.NativeName.Values)
		{
			if (nativeName is null)
			{
			}
			else if (fieldName == nativeName.ToLowerInvariant()
				|| fieldName == ValidNameGenerator.GetValidFieldName(nativeName).ToLowerInvariant())
			{
				return true;
			}
		}
		return false;
	}

	private static DataMemberHistory? DetermineHistoryFromImplementations(this InterfaceProperty interfaceProperty, HashSet<GeneratedClassInstance> instancesWithHistoriesNotFromBase)
	{
		IEnumerable<ClassProperty> enumerable = instancesWithHistoriesNotFromBase.SelectMany(i => i.Properties).Where(c => c.Base == interfaceProperty);
		return TryGetUniqueHistory(enumerable, out DataMemberHistory? history) || TryGetUniqueHistory(interfaceProperty.Implementations, out history)
			? history
			: null;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="properties"></param>
	/// <param name="history">This will be null if there are conflicts or all the options are null.</param>
	/// <returns>True if a unique, possibly null, history has been decided.</returns>
	private static bool TryGetUniqueHistory(IEnumerable<ClassProperty> properties, out DataMemberHistory? history)
	{
		history = null;
		foreach (ClassProperty classProperty in properties)
		{
			if (history is null)
			{
				history = classProperty.History;
			}
			else if (classProperty.History is null)
			{
			}
			else if (history != classProperty.History)
			{
				history = null;
				return true;
			}
		}
		return history is not null;
	}
}
