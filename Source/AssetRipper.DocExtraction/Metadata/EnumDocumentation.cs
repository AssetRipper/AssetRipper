using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AssetRipper.DocExtraction.MetaData;

public sealed record class EnumDocumentation : TypeDocumentation<EnumMemberDocumentation>
{
	public ElementType ElementType { get; set; }
	public bool IsFlagsEnum { get; set; }
}
