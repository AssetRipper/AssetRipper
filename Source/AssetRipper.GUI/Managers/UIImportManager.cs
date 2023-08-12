using AssetRipper.Export.UnityProjects;
using AssetRipper.GUI.Exceptions;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure;
using AssetRipper.IO.Files.Utils;

namespace AssetRipper.GUI.Managers
{
	public static class UIImportManager
	{
		public static void ImportFromPath(Ripper ripper, string[] paths, Action<GameStructure> onComplete, Action<Exception> onError) => new Thread(() => ImportFromPathInternal(ripper, paths, onComplete, onError))
		{
			Name = "Background Game Load Thread",
			IsBackground = true,
		}.Start();

		private static void ImportFromPathInternal(Ripper ripper, string[] paths, Action<GameStructure> onComplete, Action<Exception> onError)
		{
			try
			{
				GameStructure gameStructure = ripper.Load(paths);

				if (!gameStructure.IsValid)
				{
					onError(new GameNotFoundException());
					return;
				}

				gameStructure.CheckVersionsAreAllTheSame();
				onComplete(gameStructure);
			}
			catch (Exception e)
			{
				onError(e);
			}
		}

		private static void CheckVersionsAreAllTheSame(this GameStructure structure)
		{
			UnityVersion[] versions = structure.FileCollection
				.FetchAssetCollections()
				.Where(c => !FilenameUtils.IsEngineResource(c.Name) && !FilenameUtils.IsBuiltinExtra(c.Name))//It's common for engine resources to have a slightly lower version.
				.Select(t => t.Version)
				.Distinct()
				.ToArray();

			if (versions.Length <= 1)
			{
				return;
			}

			Logger.Log(LogType.Warning, LogCategory.Import, $"Asset collection has versions probably incompatible with each other. Here they are:");
			foreach (UnityVersion version in versions)
			{
				Logger.Log(LogType.Warning, LogCategory.Import, version.ToString());
			}
		}
	}
}
