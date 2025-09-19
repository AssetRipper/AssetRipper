using AssetRipper.Export.Configuration;
using AssetRipper.Processing;

namespace AssetRipper.Export.UnityProjects;

public interface IPostExporter
{
	void DoPostExport(GameData gameData, FullConfiguration settings, FileSystem fileSystem);
}
