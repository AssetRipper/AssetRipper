using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Processing;

namespace AssetRipper.Export.UnityProjects;

public interface IPostExporter
{
	void DoPostExport(GameData gameData, LibraryConfiguration settings);
}
