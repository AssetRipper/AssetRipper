namespace AssetRipper.DocExtraction.MetaData;

public sealed record class EnumMemberDocumentation : DocumentationBase
{
	public long Value { get; set; }
}
