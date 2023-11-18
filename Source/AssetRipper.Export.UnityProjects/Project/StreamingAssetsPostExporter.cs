using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Platforms;
using AssetRipper.Processing;

namespace AssetRipper.Export.UnityProjects.Project
{
	public class StreamingAssetsPostExporter : IPostExporter
	{
		public void DoPostExport(GameData gameData, LibraryConfiguration settings)
		{
			PlatformGameStructure? platform = gameData.PlatformStructure;
			if (platform is not null && !string.IsNullOrEmpty(platform.StreamingAssetsPath) && Directory.Exists(platform.StreamingAssetsPath))
			{
				Logger.Info(LogCategory.Export, "Copying streaming assets...");
				string inputDirectory = platform.StreamingAssetsPath;
				string outputDirectory = Path.Combine(settings.AssetsPath, "StreamingAssets");

				Directory.CreateDirectory(outputDirectory);

				foreach (string directory in Directory.EnumerateDirectories(inputDirectory, "*", SearchOption.AllDirectories))
				{
					Directory.CreateDirectory(Path.Combine(outputDirectory, Path.GetRelativePath(inputDirectory, directory)));
				}

				foreach (string file in Directory.EnumerateFiles(inputDirectory, "*", SearchOption.AllDirectories))
				{
					string newFile = Path.Combine(outputDirectory, Path.GetRelativePath(inputDirectory, file));
					File.Copy(file, newFile, true);
				}
			}
		}
	}
}
