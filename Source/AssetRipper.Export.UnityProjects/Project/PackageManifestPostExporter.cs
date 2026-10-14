using AssetRipper.Export.Configuration;
using AssetRipper.Export.UnityProjects.Scripts.UnityPackages;
using AssetRipper.Import.Logging;
using AssetRipper.Processing;

namespace AssetRipper.Export.UnityProjects.Project;

public class PackageManifestPostExporter : IPostExporter
{
	public void DoPostExport(GameData gameData, FullConfiguration settings, FileSystem fileSystem)
	{
		string packagesDirectory = fileSystem.Path.Join(settings.ProjectRootPath, "Packages");
		fileSystem.Directory.Create(packagesDirectory);
		string path = fileSystem.Path.Join(packagesDirectory, "manifest.json");
		PackageManifest manifest = CreateManifest(settings.Version);
		AddReferencedPackages(manifest, gameData, settings);
		using Stream stream = fileSystem.File.Create(path);
		manifest.Save(stream);
	}

	protected virtual PackageManifest CreateManifest(UnityVersion version)
	{
		return PackageManifest.CreateDefault(version);
	}

	/// <summary>
	/// Add the packages whose assemblies were replaced with package references.
	/// </summary>
	private static void AddReferencedPackages(PackageManifest manifest, GameData gameData, FullConfiguration settings)
	{
		if (!settings.ExportSettings.ReferenceUnityPackages || !gameData.AssemblyManager.IsSet)
		{
			return;
		}
		UnityPackageIndex? index = UnityPackageIndex.GetForProject(settings.Version, settings.ExportSettings.UnityEditorPath);
		if (index is null)
		{
			return;
		}
		foreach (string assemblyName in gameData.AssemblyManager.GetAssemblies().Select(a => a.Name?.ToString()).OfType<string>().Order())
		{
			if (index.TryGetAssembly(assemblyName, out PackageAssembly? assembly) && manifest.Dependencies.TryAdd(assembly.PackageName, assembly.PackageVersion))
			{
				Logger.Info(LogCategory.Export, $"Referencing package {assembly.PackageName} {assembly.PackageVersion}");
			}
		}
		Logger.Info(LogCategory.Export, $"The package versions match Unity {index.EditorVersion}. Open the project with that version.");
	}
}
