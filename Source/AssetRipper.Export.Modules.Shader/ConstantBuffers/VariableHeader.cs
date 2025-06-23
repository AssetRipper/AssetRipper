namespace AssetRipper.Export.Modules.Shaders.ConstantBuffers;

internal class VariableHeader
{
	public uint NameOffset { get; init; }
	public uint StartOffset { get; init; }
	public uint TypeOffset { get; init; }
	public required Variable Variable { get; init; }
}
