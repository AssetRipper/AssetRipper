using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Processing;

namespace AssetRipper.Export.UnityProjects;

public class CsprojModifierExporter : IPostExporter
{
	public void DoPostExport(GameData gameData, LibraryConfiguration libraryConfiguration)
	{
		// export the csharp project modifier folder to the ExportedProject/Packages folder

	}
}
