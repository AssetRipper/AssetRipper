using System.Collections.Generic;

namespace AssetRipper.Library.Exporters.PathIdMapping;

internal sealed class SerializedFileInfo
{
	public string? Name { get; set; }
	public List<SerializedAssetInfo> Assets { get; set; } = new();
}
