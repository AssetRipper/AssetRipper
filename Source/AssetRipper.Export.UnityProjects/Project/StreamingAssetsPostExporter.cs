using AssetRipper.Export.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Platforms;
using AssetRipper.Processing;

namespace AssetRipper.Export.UnityProjects.Project;

public class StreamingAssetsPostExporter : IPostExporter
{
	public void DoPostExport(GameData gameData, FullConfiguration settings, FileSystem fileSystem)
	{
		PlatformGameStructure? platform = gameData.PlatformStructure;
		if (platform is null)
		{
			return;
		}

		string? inputDirectory = platform.StreamingAssetsPath;
		if (!string.IsNullOrEmpty(inputDirectory) && platform.FileSystem.Directory.Exists(inputDirectory))
		{
			Logger.Info(LogCategory.Export, "Copying streaming assets...");
			string outputDirectory = fileSystem.Path.Join(settings.AssetsPath, "StreamingAssets");

			fileSystem.Directory.Create(outputDirectory);

			foreach (string directory in platform.FileSystem.Directory.EnumerateDirectories(inputDirectory, "*", SearchOption.AllDirectories))
			{
				string relativePath = platform.FileSystem.Path.GetRelativePath(inputDirectory, directory);
				fileSystem.Directory.Create(fileSystem.Path.Join(outputDirectory, relativePath));
			}

			foreach (string file in platform.FileSystem.Directory.EnumerateFiles(inputDirectory, "*", SearchOption.AllDirectories))
			{
				string relativePath = platform.FileSystem.Path.GetRelativePath(inputDirectory, file);
				string newFile = fileSystem.Path.Join(outputDirectory, relativePath);

				using Stream readStream = platform.FileSystem.File.OpenRead(file);
				using Stream writeStream = fileSystem.File.Create(newFile);
				readStream.CopyTo(writeStream);
			}
		}
	}
}
