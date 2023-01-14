namespace AssetRipper.Export.UnityProjects.PathIdMapping;

internal sealed class SerializedFileInfo
{
	public string? Name { get; set; }
	public List<SerializedAssetInfo> Assets { get; set; } = new();
}
