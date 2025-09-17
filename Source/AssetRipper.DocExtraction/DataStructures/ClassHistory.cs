using AssetRipper.AssemblyDumper.Utils;
using AssetRipper.DocExtraction.MetaData;
using AssetRipper.Primitives;

namespace AssetRipper.DocExtraction.DataStructures;

public sealed class ClassHistory : ComplexTypeHistory
{
	/// <summary>
	/// The full name for the base type of the class
	/// </summary>
	public VersionedList<FullNameRecord> BaseFullName { get; set; } = new();

	public override void Initialize(UnityVersion version, DocumentationBase first)
	{
		base.Initialize(version, first);
		ClassDocumentation @class = (ClassDocumentation)first;
		BaseFullName.Add(version, new FullNameRecord(@class.BaseFullName, @class.BaseName));
	}

	protected override void AddNotNull(UnityVersion version, DocumentationBase next)
	{
		base.AddNotNull(version, next);
		AddIfNotEqual(BaseFullName, version, ((ClassDocumentation)next).BaseFullNameRecord);
	}

	public static ClassHistory From(UnityVersion version, ClassDocumentation @class)
	{
		ClassHistory? classHistory = new();
		classHistory.Initialize(version, @class);
		return classHistory;
	}

	public bool TryGetBaseClass(UnityVersion version, HistoryFile historyFile, [NotNullWhen(true)] out ClassHistory? baseClass)
	{
		if (historyFile.Classes.TryGetValue(BaseFullName.GetItemForVersion(version).ToString(), out ClassHistory? potentialBaseClass))
		{
			if (potentialBaseClass.ExistsOnVersion(version))//Just to be safe
			{
				baseClass = potentialBaseClass;
				return true;
			}
		}
		baseClass = null;
		return false;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// Base class must be constant for this to succeed.
	/// </remarks>
	/// <param name="historyFile"></param>
	/// <param name="baseClass"></param>
	/// <returns></returns>
	public bool TryGetBaseClass(HistoryFile historyFile, [NotNullWhen(true)] out ClassHistory? baseClass)
	{
		if (BaseFullName.Count == 1 && historyFile.Classes.TryGetValue(BaseFullName[0].Value.ToString(), out ClassHistory? potentialBaseClass))
		{
			baseClass = potentialBaseClass;
			return true;
		}
		baseClass = null;
		return false;
	}

	public bool IsUnityEngineObject()
	{
		return Name == "Object" && Namespace == "UnityEngine";
	}

	public bool InheritsFromUnityEngineObject(UnityVersion version, HistoryFile historyFile)
	{
		ClassHistory? baseClass = this;
		while (true)
		{
			if (baseClass.IsUnityEngineObject())
			{
				return true;
			}
			else if (!baseClass.TryGetBaseClass(version, historyFile, out baseClass))
			{
				break;
			}
		}
		return false;
	}

	public override IReadOnlyDictionary<string, DataMemberHistory> GetAllMembers(UnityVersion version, HistoryFile historyFile)
	{
		Dictionary<string, DataMemberHistory> result = new();
		ClassHistory? baseClass = this;
		while (true)
		{
			foreach ((string name, DataMemberHistory member) in baseClass.Members)
			{
				if (member.ExistsOnVersion(version))
				{
					result[name] = member;//In the case of property overrides, the base property is usually more documented
				}
			}
			if (!baseClass.TryGetBaseClass(version, historyFile, out baseClass))
			{
				break;
			}
		}
		return result;
	}

	public override IReadOnlyDictionary<string, DataMemberHistory> GetAllMembers(HistoryFile historyFile)
	{
		Dictionary<string, DataMemberHistory> result = new();
		ClassHistory? baseClass = this;
		while (true)
		{
			foreach ((string name, DataMemberHistory member) in baseClass.Members)
			{
				result[name] = member;//In the case of property overrides, the base property is usually more documented
			}
			if (!baseClass.TryGetBaseClass(historyFile, out baseClass))
			{
				break;
			}
		}
		return result;
	}
}