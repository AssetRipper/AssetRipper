using AssetRipper.AssemblyDumper.Enums;
using AssetRipper.AssemblyDumper.Methods;
using AssetRipper.AssemblyDumper.Types;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass040_AddEnums
{
	private static readonly HashSet<string?> namespaceBlacklist = new()
	{
		"UnityEngine.Yoga",
	};
	private static readonly HashSet<string> fullNameBlackList = new()
	{
		"UnityEditor.PackageManager.LogLevel",
		"UnityEngine.LogOption",
		"UnityEngine.LogType",
	};

	private static readonly Dictionary<string, (TypeDefinition, EnumDefinitionBase)> enumDictionary = new();
	internal static IReadOnlyDictionary<string, (TypeDefinition, EnumDefinitionBase)> EnumDictionary => enumDictionary;
	public static void DoPass()
	{
		IMethodDefOrRef flagsConstructor = SharedState.Instance.Importer.ImportDefaultConstructor<FlagsAttribute>();
		int total = 0;
		int merged = 0;
		Dictionary<string, IReadOnlyList<EnumDefinitionBase>> definitionDictionary = EnumDefinitionBase.Create(SharedState.Instance.HistoryFile.Enums.Values.Where(h =>
		{
			return !fullNameBlackList.Contains(h.FullName) && !namespaceBlacklist.Contains(h.Namespace);
		}));
		foreach ((string name, IReadOnlyList<EnumDefinitionBase> definitionList) in definitionDictionary)
		{
			for (int i = 0; i < definitionList.Count; i++)
			{
				EnumDefinitionBase definition = definitionList[i];

				string enumName = definitionList.Count > 1 ? $"{definition.Name}_{i}" : definition.Name;

				TypeDefinition type = EnumCreator.CreateFromDictionary(
					SharedState.Instance,
					SharedState.EnumsNamespace,
					enumName,
					definition.GetOrderedFields(),
					definition.ElementType.ToEnumUnderlyingType());

				if (definition.IsFlagsEnum)
				{
					type.AddCustomAttribute(flagsConstructor);
				}

				foreach (string fullName in definition.FullNames)
				{
					enumDictionary.Add(fullName, (type, definition));
				}

				if (definition is MergedEnumDefinition)
				{
					merged++;
				}

				total++;
			}
		}
		Console.WriteLine($"\t{total} generated enums, of which {merged} were created from multiple sources.");
	}
}
