using System.Collections.Generic;

namespace AssetRipper.Library.Exporters.PathIdMapping;

internal sealed class SerializedGameInfo
{
	public List<SerializedFileInfo> Files { get; set; } = new();
}
