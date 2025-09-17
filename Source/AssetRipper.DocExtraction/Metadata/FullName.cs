using System.Text.Json.Serialization;

namespace AssetRipper.DocExtraction.MetaData;

public record struct FullNameRecord(string FullName, string Name)
{
	public override string ToString() => FullName;
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
}