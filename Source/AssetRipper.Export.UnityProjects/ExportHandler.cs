using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Processing;

namespace AssetRipper.Export.UnityProjects;

public class ExportHandler
{
	protected LibraryConfiguration Settings { get; }

	public ExportHandler(LibraryConfiguration settings)
	{
		Settings = settings;
	}

	public GameData Load(IReadOnlyList<string> paths)
	{
		return Ripper.Load(paths, Settings);
	}

	public void Process(GameData gameData)
	{
		Ripper.Process(gameData, GetProcessors());
	}

	protected virtual IEnumerable<IAssetProcessor> GetProcessors()
	{
		return Ripper.GetDefaultProcessors(Settings);
	}

	public void Export(GameData gameData, string outputPath)
	{
		Ripper.ExportProject(gameData, Settings, outputPath, GetPostExporters(), GetBeforeExport());
	}

	protected virtual Action<ProjectExporter>? GetBeforeExport()
	{
		return null;
	}

	protected virtual IEnumerable<IPostExporter> GetPostExporters()
	{
		return Ripper.GetDefaultPostExporters();
	}

	public GameData LoadAndProcess(IReadOnlyList<string> paths)
	{
		GameData gameData = Load(paths);
		Process(gameData);
		return gameData;
	}

	public void LoadProcessAndExport(IReadOnlyList<string> inputPaths, string outputPath)
	{
		GameData gameData = LoadAndProcess(inputPaths);
		Export(gameData, outputPath);
	}

	public void ThrowIfSettingsDontMatch(LibraryConfiguration settings)
	{
		if (Settings != settings)
		{
			throw new ArgumentException("Settings don't match");
		}
	}
}
