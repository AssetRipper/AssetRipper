﻿using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Processing;

namespace AssetRipper.Export.UnityProjects.Project;

public class PackageManifestPostExporter : IPostExporter
{
	public void DoPostExport(GameData gameData, LibraryConfiguration settings)
	{
		string packagesDirectory = Path.Combine(settings.ProjectRootPath, "Packages");
		Directory.CreateDirectory(packagesDirectory);
		CreateManifest(settings.Version).Save(Path.Combine(packagesDirectory, "manifest.json"));
	}

	protected virtual PackageManifest CreateManifest(UnityVersion version)
	{
		return PackageManifest.CreateDefault(version);
	}
}
