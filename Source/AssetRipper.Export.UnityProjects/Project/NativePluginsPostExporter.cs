using AssetRipper.Export.Configuration;
using AssetRipper.Export.UnityProjects.Scripts.UnityPackages;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Platforms;
using AssetRipper.Processing;

namespace AssetRipper.Export.UnityProjects.Project;

/// <summary>
/// Copies the game's native plugins, such as platform SDK libraries, into the project's Plugins folder.
/// </summary>
/// <remarks>
/// Managed code that calls into a native library fails with <see cref="DllNotFoundException"/> without it.
/// Unity generates the import settings, choosing the platform and CPU from the folder names.
/// These are left out:
/// <list type="bullet">
/// <item>Code that Burst compiled for the build, which the editor generates itself.</item>
/// <item>Libraries that a referenced Unity package already contains, since Unity rejects two plugins with the same name.</item>
/// </list>
/// </remarks>
public class NativePluginsPostExporter : IPostExporter
{
	public void DoPostExport(GameData gameData, FullConfiguration settings, FileSystem fileSystem)
	{
		PlatformGameStructure? platform = gameData.PlatformStructure;
		if (platform?.GameDataPath is not { } dataPath)
		{
			return;
		}
		string inputDirectory = platform.FileSystem.Path.Join(dataPath, "Plugins");
		if (!platform.FileSystem.Directory.Exists(inputDirectory))
		{
			return;
		}

		HashSet<string> referencedPackages = [];
		UnityPackageIndex? packageIndex = null;
		if (settings.ExportSettings.ReferenceUnityPackages && gameData.AssemblyManager.IsSet)
		{
			packageIndex = UnityPackageIndex.GetForProject(settings.Version, settings.ExportSettings.UnityEditorPath);
			if (packageIndex is not null)
			{
				referencedPackages = packageIndex.GetReferencedPackageNames(gameData.AssemblyManager.GetAssemblies());
			}
		}

		Logger.Info(LogCategory.Export, "Copying native plugins...");
		string outputDirectory = fileSystem.Path.Join(settings.AssetsPath, "Plugins");
		int count = 0;
		foreach (string file in platform.FileSystem.Directory.EnumerateFiles(inputDirectory, "*", SearchOption.AllDirectories))
		{
			string fileName = platform.FileSystem.Path.GetFileName(file);
			if (!IsNativeLibrary(fileName) || IsBurstOutput(fileName))
			{
				continue;
			}
			if (packageIndex is not null && packageIndex.HasPluginFile(fileName, referencedPackages))
			{
				Logger.Info(LogCategory.Export, $"Not copying {fileName}, because a referenced package contains it");
				continue;
			}

			string relativePath = platform.FileSystem.Path.GetRelativePath(inputDirectory, file);
			string newFile = fileSystem.Path.Join(outputDirectory, relativePath);
			fileSystem.Directory.Create(fileSystem.Path.GetDirectoryName(newFile)!);
			using Stream readStream = platform.FileSystem.File.OpenRead(file);
			using Stream writeStream = fileSystem.File.Create(newFile);
			readStream.CopyTo(writeStream);
			count++;
		}
		Logger.Info(LogCategory.Export, $"Copied {count} native plugins");
	}

	private static bool IsNativeLibrary(string fileName)
	{
		return Path.GetExtension(fileName).ToLowerInvariant() is ".dll" or ".so" or ".dylib";
	}

	private static bool IsBurstOutput(string fileName)
	{
		return fileName.StartsWith("lib_burst_generated", StringComparison.OrdinalIgnoreCase);
	}
}
