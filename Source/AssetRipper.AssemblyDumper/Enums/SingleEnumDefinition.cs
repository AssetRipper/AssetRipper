using AssetRipper.DocExtraction.DataStructures;

namespace AssetRipper.AssemblyDumper.Enums;

internal sealed class SingleEnumDefinition : EnumDefinitionBase
{
	public SingleEnumDefinition(EnumHistory history)
	{
		History = history;
	}

	public EnumHistory History { get; }
	public override string Name => History.Name;
	public override IEnumerable<string> FullNames => new SingleEnumerable<string>(History.FullName);

	public override bool MatchesFullName(string fullName)
	{
		return History.FullName == fullName;
	}

	public override IOrderedEnumerable<KeyValuePair<string, long>> GetOrderedFields() => History.GetOrderedFields();

	public override bool IsFlagsEnum => History.IsFlagsEnum;

	public override ElementType ElementType => History.GetMergedElementType();

	public override UnityVersion MinimumVersion => History.MinimumVersion;
}
