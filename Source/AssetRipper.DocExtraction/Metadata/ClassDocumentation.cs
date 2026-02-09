using System.Text.Json.Serialization;

namespace AssetRipper.DocExtraction.MetaData;

public sealed record class ClassDocumentation : ComplexTypeDocumentation
{
	[JsonIgnore]
	public string? BaseNamespace => BaseFullNameRecord.Namespace;
	public string BaseName { get; set; } = "";
	public string BaseFullName { get; set; } = "";
	[JsonIgnore]
	public FullNameRecord BaseFullNameRecord => new FullNameRecord(BaseFullName, BaseName);
}
