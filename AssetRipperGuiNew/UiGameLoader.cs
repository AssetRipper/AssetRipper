using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Structure.GameStructure;
using AssetRipperGuiNew.Exceptions;
using System;
using System.Linq;
using System.Threading;

namespace AssetRipperGuiNew
{
	public static class UiGameLoader
	{
		public static void LoadFromPath(string[] paths, Action<GameStructure> onComplete, Action<Exception> onError) => new Thread(() => LoadFromPathInternal(paths, onComplete, onError))
		{
			Name = "Background Game Load Thread",
			IsBackground = true,
		}.Start();

		private static void LoadFromPathInternal(string[] paths, Action<GameStructure> onComplete, Action<Exception> onError)
		{
			try
			{
				GameStructure gameStructure = GameStructure.Load(paths);

				if (!gameStructure.IsValid)
				{
					onError(new NewUiGameNotFoundException());
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
				.GameFiles
				.Values
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