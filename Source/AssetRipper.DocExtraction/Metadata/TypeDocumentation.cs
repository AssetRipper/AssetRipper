using AssetRipper.DocExtraction.Extensions;
using System.Text.Json.Serialization;

namespace AssetRipper.DocExtraction.MetaData;

public abstract record class TypeDocumentation<TMember> : DocumentationBase where TMember : DocumentationBase, new()
{
	public Dictionary<string, TMember> Members { get; set; } = new();
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
	public string FullName { get; set; } = "";
	public override string ToString() => FullName.ToString();
	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), FullName, Members.GetHashCodeByContent());
	}

	public virtual bool Equals(TypeDocumentation<TMember>? other)
	{
		return (object)this == other || (base.Equals(other) && FullName == other.FullName && Members.EqualByContent(other.Members));
	}
}
