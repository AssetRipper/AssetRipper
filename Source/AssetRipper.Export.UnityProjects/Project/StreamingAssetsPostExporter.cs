using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Platforms;

namespace AssetRipper.Export.UnityProjects.Project
{
	public class StreamingAssetsPostExporter : IPostExporter
	{
		public void DoPostExport(Ripper ripper)
		{
			PlatformGameStructure? platform = ripper.GameStructure?.PlatformStructure;
			if (platform is not null && !string.IsNullOrEmpty(platform.StreamingAssetsPath) && Directory.Exists(platform.StreamingAssetsPath))
			{
				Logger.Info(LogCategory.Export, "Copying streaming assets...");
				string inputDirectory = platform.StreamingAssetsPath;
				string outputDirectory = Path.Combine(ripper.Settings.AssetsPath, "StreamingAssets");

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
