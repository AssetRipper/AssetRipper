using AssetRipper.DocExtraction.DataStructures;
using AssetRipper.Numerics;

namespace AssetRipper.AssemblyDumper.Groups;

internal sealed class GeneratedClassInstance
{
	public string Name => Class.Name;
	public UniversalClass Class { get; }
	public TypeDefinition Type { get; set; }
	public Range<UnityVersion> VersionRange { get; set; }
	public GeneratedClassInstance? Base { get; set; }
	public List<GeneratedClassInstance> Derived { get; } = new();
	public ComplexTypeHistory? History { get; set; }
	public List<ClassProperty> Properties { get; } = new();
	public ClassGroupBase Group { get; }

	public GeneratedClassInstance(ClassGroupBase group, UniversalClass @class, TypeDefinition type, UnityVersion startVersion, UnityVersion endVersion)
	{
		Class = @class;
		Type = type;
		VersionRange = new Range<UnityVersion>(startVersion, endVersion);
		Group = group;
	}

	public override string ToString() => $"{Name} {VersionRange.Start} : {VersionRange.End}";

	public int GetSerializedVersion()
	{
		return Class.EditorRootNode is not null
			? Class.EditorRootNode.Version
			: Class.ReleaseRootNode is not null
				? Class.ReleaseRootNode.Version
				: 1;
	}

	public bool InheritsFromType(int id)
	{
		return Class.OriginalTypeID == id || (Base?.InheritsFromType(id) ?? false);
	}

	public bool InheritsFromAssetImporter() => InheritsFromType(1003);//The id for AssetImporter

	public void InitializeHistory(HistoryFile historyFile)
	{
		if (Group is SubclassGroup)
		{
			TryGetSubclass(Name, VersionRange.Start, historyFile, out ComplexTypeHistory? history);
			History = history;
		}
		else
		{
			TryGetClass(Name, VersionRange.Start, historyFile, out ClassHistory? classHistory);
			History = classHistory;
		}
	}

	private static bool TryGetClass(string name, UnityVersion version, HistoryFile historyFile, [NotNullWhen(true)] out ClassHistory? history)
	{
		if (TryGetClassFullName($"UnityEngine.{name}", version, historyFile, out history) && history.InheritsFromUnityEngineObject(version, historyFile))
		{
			return true;
		}
		else if (TryGetClassFullName($"UnityEditor.{name}", version, historyFile, out history) && history.InheritsFromUnityEngineObject(version, historyFile))
		{
			return true;
		}
		else
		{
			foreach ((_, ClassHistory classHistory) in historyFile.Classes)
			{
				if (MatchesNameAndExists(classHistory, name, version) && classHistory.InheritsFromUnityEngineObject(version, historyFile))
				{
					history = classHistory;
					return true;
				}
			}
			history = null;
			return false;
		}
	}

	private static bool TryGetSubclass(string name, UnityVersion version, HistoryFile historyFile, [NotNullWhen(true)] out ComplexTypeHistory? history)
	{
		if (TryGetClassFullName($"UnityEngine.{name}", version, historyFile, out ClassHistory? @class) && !@class.InheritsFromUnityEngineObject(version, historyFile))
		{
			history = @class;
			return true;
		}
		else if (TryGetClassFullName($"UnityEditor.{name}", version, historyFile, out @class) && !@class.InheritsFromUnityEngineObject(version, historyFile))
		{
			history = @class;
			return true;
		}
		if (TryGetStructFullName($"UnityEngine.{name}", version, historyFile, out StructHistory? @struct))
		{
			history = @struct;
			return true;
		}
		else if (TryGetStructFullName($"UnityEditor.{name}", version, historyFile, out @struct))
		{
			history = @struct;
			return true;
		}
		else
		{
			foreach ((_, ClassHistory classHistory) in historyFile.Classes)
			{
				if (MatchesNameAndExists(classHistory, name, version) && !classHistory.InheritsFromUnityEngineObject(version, historyFile))
				{
					history = classHistory;
					return true;
				}
			}
			foreach ((_, StructHistory structHistory) in historyFile.Structs)
			{
				if (MatchesNameAndExists(structHistory, name, version))
				{
					history = structHistory;
					return true;
				}
			}
			history = null;
			return false;
		}
	}

	private static bool MatchesNameAndExists(HistoryBase history, string name, UnityVersion version)
	{
		return (history.Name == name || history.NativeName.GetItemForVersion(version) == name)
			&& history.ExistsOnVersion(version);
	}

	private static bool TryGetClassFullName(string fullName, UnityVersion version, HistoryFile historyFile, [NotNullWhen(true)] out ClassHistory? history)
	{
		if (historyFile.Classes.TryGetValue(fullName, out ClassHistory? potentialHistory) && potentialHistory.ExistsOnVersion(version))
		{
			history = potentialHistory;
			return true;
		}
		else
		{
			history = null;
			return false;
		}
	}

	private static bool TryGetStructFullName(string fullName, UnityVersion version, HistoryFile historyFile, [NotNullWhen(true)] out StructHistory? history)
	{
		if (historyFile.Structs.TryGetValue(fullName, out StructHistory? potentialHistory) && potentialHistory.ExistsOnVersion(version))
		{
			history = potentialHistory;
			return true;
		}
		else
		{
			history = null;
			return false;
		}
	}
}
