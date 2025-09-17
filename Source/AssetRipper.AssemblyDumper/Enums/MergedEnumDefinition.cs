using AssetRipper.DocExtraction.DataStructures;
using AssetRipper.DocExtraction.Extensions;

namespace AssetRipper.AssemblyDumper.Enums;

internal sealed class MergedEnumDefinition : EnumDefinitionBase
{
	internal MergedEnumDefinition(List<EnumHistory> histories)
	{
		Histories = histories;
	}

	public IEnumerable<EnumHistory> Histories { get; }

	public override string Name => Histories.First().Name;

	public override IEnumerable<string> FullNames => Histories.Select(history => history.FullName);

	public override bool IsFlagsEnum => Histories.Any(v => v.IsFlagsEnum);

	public override ElementType ElementType
	{
		get
		{
			bool first = true;
			ElementType type = default;
			foreach (EnumHistory history in Histories)
			{
				if (first)
				{
					if (history.TryGetMergedElementType(out ElementType otherType))
					{
						type = otherType;
						first = false;
					}
					else
					{
						return ElementType.I8;
					}
				}
				else
				{
					if (history.TryGetMergedElementType(out ElementType otherType))
					{
						type = type.Merge(otherType);
					}
					else
					{
						return ElementType.I8;
					}
				}
			}
			return type;
		}
	}

	public override UnityVersion MinimumVersion => Histories.Min(v => v.MinimumVersion);

	public override bool MatchesFullName(string fullName)
	{
		return FullNames.Contains(fullName);
	}

	public override IOrderedEnumerable<KeyValuePair<string, long>> GetOrderedFields()
	{
		return Histories.First().GetOrderedFields();
	}
}