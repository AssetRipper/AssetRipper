using AssetRipper.Assets.Bundles;
using AssetRipper.Export.UnityProjects;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Processing;

namespace AssetRipper.GUI.Web;

public static class GameFileLoader
{
	private static GameData? GameData { get; set; }
	[MemberNotNullWhen(true, nameof(GameData))]
	public static bool IsLoaded => GameData is not null;
	public static GameBundle GameBundle => GameData!.GameBundle;
	public static IAssemblyManager AssemblyManager => GameData!.AssemblyManager;
	public static LibraryConfiguration Settings { get; } = LoadSettings();
	private static ExportHandler exportHandler = new(Settings);
	public static ExportHandler ExportHandler
	{
		private get
		{
			return exportHandler;
		}
		set
		{
			ArgumentNullException.ThrowIfNull(value);
			value.ThrowIfSettingsDontMatch(Settings);
			exportHandler = value;
		}
	}
	public static bool Premium => ExportHandler.GetType() != typeof(ExportHandler);

	public static void Reset()
	{
		if (GameData is not null)
		{
			GameData = null;
			Logger.Info(LogCategory.General, "Data was reset.");
		}
	}

	public static void LoadAndProcess(IReadOnlyList<string> paths)
	{
		Reset();
		Settings.LogConfigurationValues();
		GameData = ExportHandler.LoadAndProcess(paths);
	}

	public static void Export(string path)
	{
		if (IsLoaded)
		{
			Directory.Delete(path, true);
			Directory.CreateDirectory(path);
			ExportHandler.Export(GameData, path);
		}
	}

	private static LibraryConfiguration LoadSettings()
	{
		LibraryConfiguration settings = new();
		settings.LoadFromDefaultPath();
		return settings;
	}
}
