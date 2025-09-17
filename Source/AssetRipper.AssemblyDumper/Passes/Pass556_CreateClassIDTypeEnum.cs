using AssetRipper.AssemblyDumper.Documentation;
using AssetRipper.AssemblyDumper.Types;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass556_CreateClassIDTypeEnum
{
	public static Dictionary<FieldDefinition, ClassGroup> FieldGroupDictionary { get; } = new();
	public static TypeDefinition? ClassIdTypeDefintion { get; private set; }

	public static void DoPass()
	{
		Dictionary<int, ClassGroup> classIdDictionary = CreateClassIdDictionary();
		Dictionary<string, long> nameDictionary = CreateNameDictionary(classIdDictionary);
		ClassIdTypeDefintion = EnumCreator.CreateFromDictionary(SharedState.Instance, SharedState.RootNamespace, "ClassIDType", nameDictionary);

		List<KeyValuePair<string, long>> alphabeticalList = nameDictionary.ToList();
		alphabeticalList.Sort((a, b) => a.Key.CompareTo(b.Key));
		TypeDefinition alphabeticalEnum = EnumCreator.CreateFromDictionary(SharedState.Instance, SharedState.RootNamespace, "ClassIDTypeAlphabetical", alphabeticalList);
		alphabeticalEnum.IsPublic = false;

		foreach (FieldDefinition field in ClassIdTypeDefintion.Fields)
		{
			if (field.IsStatic)
			{
				int id = (int)nameDictionary[field.Name!];
				FieldGroupDictionary.Add(field, classIdDictionary[id]);
			}
		}
		foreach (FieldDefinition field in alphabeticalEnum.Fields)
		{
			if (field.IsStatic)
			{
				int id = (int)nameDictionary[field.Name!];
				FieldGroupDictionary.Add(field, classIdDictionary[id]);
			}
		}

		string documentationString = $"This enum is an identifier for the {nameDictionary.Count} Unity object types.";
		DocumentationHandler.AddTypeDefinitionLine(ClassIdTypeDefintion, documentationString);
		DocumentationHandler.AddTypeDefinitionLine(alphabeticalEnum, documentationString);

		Console.WriteLine($"\t{nameDictionary.Count} ClassIDType numbers.");
	}

	private static Dictionary<string, long> CreateNameDictionary(Dictionary<int, ClassGroup> classIdDictionary)
	{
		HashSet<string> duplicateNames = GetDuplicates(classIdDictionary.Values.Select(g => g.Name));
		Dictionary<string, long> result = new Dictionary<string, long>(classIdDictionary.Count);
		foreach ((int id, ClassGroup group) in classIdDictionary.OrderBy(pair => pair.Key))
		{
			string rawName = group.Name;
			if (duplicateNames.Contains(rawName))
			{
				result.Add($"{rawName}_{id}", id);
			}
			else
			{
				result.Add(rawName, id);
			}
		}
		return result;
	}

	private static Dictionary<int, ClassGroup> CreateClassIdDictionary()
	{
		Dictionary<int, ClassGroup> rawDictionary = SharedState.Instance.ClassGroups.ToDictionary(pair => pair.Key, pair => pair.Value);
		foreach ((int mainID, IReadOnlyList<int> idList) in Pass001_MergeMovedGroups.Changes)
		{
			ClassGroup group = SharedState.Instance.ClassGroups[mainID];
			foreach (int id in idList)
			{
				rawDictionary.Add(id, group);
			}
		}

		return rawDictionary;
	}

	private static HashSet<string> GetDuplicates(IEnumerable<string> rawStrings)
	{
		HashSet<string> uniqueStrings = new();
		HashSet<string> duplicates = new();
		foreach (string str in rawStrings)
		{
			if (uniqueStrings.Add(str))
			{
			}
			else
			{
				duplicates.Add(str);
			}
		}
		return duplicates;
	}
}
