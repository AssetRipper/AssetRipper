using AssetRipper.DocExtraction.MetaData;
using AssetRipper.Primitives;
using System.Text.Json.Serialization;

namespace AssetRipper.DocExtraction.DataStructures;

public abstract class TypeHistory<TMember, TMemberDocumentation> : HistoryBase
	where TMember : HistoryBase, new()
	where TMemberDocumentation : DocumentationBase, new()
{
	public Dictionary<string, TMember> Members { get; set; } = new();
	public string FullName { get; set; } = "";
	[JsonIgnore]
	public string? Namespace
	{
		get
		{
			if (Name.Length == 0)
			{
				return FullName;
			}
			else if (FullName.Length <= Name.Length + 1)
			{
				return null;
			}
			else
			{
				return FullName.Substring(0, FullName.Length - Name.Length - 1);
			}
		}
	}
	public override string ToString() => FullName;

	public override void Initialize(UnityVersion version, DocumentationBase first)
	{
		base.Initialize(version, first);
		TypeDocumentation<TMemberDocumentation> type = (TypeDocumentation<TMemberDocumentation>)first;
		FullName = type.FullName;
		foreach ((string memberName, TMemberDocumentation memberInstance) in type.Members)
		{
			TMember memberHistory = new();
			memberHistory.Initialize(version, memberInstance);
			Members.Add(memberName, memberHistory);
		}
	}

	protected override void AddNotNull(UnityVersion version, DocumentationBase next)
	{
		base.AddNotNull(version, next);
		TypeDocumentation<TMemberDocumentation> type = (TypeDocumentation<TMemberDocumentation>)next;
		HashSet<string> processedNames = new();
		foreach ((string memberName, TMember memberHistory) in Members)
		{
			if (type.Members.TryGetValue(memberName, out TMemberDocumentation? memberInstance))
			{
				memberHistory.Add(version, memberInstance);
			}
			else
			{
				memberHistory.Add(version, null);
			}
			processedNames.Add(memberName);
		}
		foreach ((string memberName, TMemberDocumentation memberInstance) in type.Members)
		{
			if (!processedNames.Contains(memberName))
			{
				TMember memberHistory = new();
				memberHistory.Initialize(version, memberInstance);
				Members.Add(memberName, memberHistory);
			}
		}
	}
}
