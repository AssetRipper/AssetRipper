namespace AssetRipper.Export.UnityProjects.Project;

public class PackageManifestPostExporter : IPostExporter
{
	public void DoPostExport(Ripper ripper)
	{
		string packagesDirectory = Path.Combine(ripper.Settings.ProjectRootPath, "Packages");
		Directory.CreateDirectory(packagesDirectory);
		CreateManifest(ripper.Settings.Version).Save(Path.Combine(packagesDirectory, "manifest.json"));
	}

	protected virtual PackageManifest CreateManifest(UnityVersion version)
	{
		return PackageManifest.CreateDefault(version);
	}
}
