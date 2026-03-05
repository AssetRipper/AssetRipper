namespace AssetRipper.Export.Modules.Shaders.ShaderBlob.Parameters;

public sealed record class UAVParameter
{
	public UAVParameter() { }

	public UAVParameter(string name, int index, int originalIndex)
	{
		Name = name;
		NameIndex = -1;
		Index = index;
		OriginalIndex = originalIndex;
	}

	public string Name { get; set; } = string.Empty;
	public int NameIndex { get; set; }
	public int Index { get; set; }
	public int OriginalIndex { get; set; }
}
