using AssetRipper.AssemblyDumper.Utils;
using AssetRipper.DocExtraction.MetaData;
using AssetRipper.Primitives;

namespace AssetRipper.DocExtraction.DataStructures;

public sealed class EnumMemberHistory : HistoryBase
{
	public VersionedList<long> Value { get; set; } = new();

	public override void Initialize(UnityVersion version, DocumentationBase first)
	{
		base.Initialize(version, first);
		Value.Add(version, ((EnumMemberDocumentation)first).Value);
	}

	protected override void AddNotNull(UnityVersion version, DocumentationBase next)
	{
		base.AddNotNull(version, next);
		AddIfNotEqual(Value, version, ((EnumMemberDocumentation)next).Value);
	}
}
