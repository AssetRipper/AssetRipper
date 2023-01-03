using AssetRipper.Core.Logging;
using AssetRipper.Core.Structure.GameStructure.Platforms;
using System.IO;

namespace AssetRipper.Library.Exporters
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

				foreach (string directory in Directory.EnumerateDirectories(inputDirectory))
				{
					Directory.CreateDirectory(Path.Combine(outputDirectory, Path.GetRelativePath(inputDirectory, directory)));
				}

				foreach (string file in Directory.EnumerateFiles(inputDirectory))
				{
					string newFile = Path.Combine(outputDirectory, Path.GetRelativePath(inputDirectory, file));
					File.Copy(file, newFile, true);
				}
			}
		}
	}
}
