using AssetRipper.Import.Logging;
using AssetRipper.Import.Utils;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using System.Diagnostics;

namespace AssetRipper.Import.Configuration;

public class CoreConfiguration
{
	#region Import Settings
	/// <summary>
	/// Disabling scripts can allow some games to export when they previously did not.
	/// </summary>
	public bool DisableScriptImport => ImportSettings.ScriptContentLevel == ScriptContentLevel.Level0;

	public ImportSettings ImportSettings
	{
		get => SingletonData.GetStoredValue<ImportSettings>(nameof(ImportSettings));
		set => SingletonData.GetValue<JsonDataInstance<ImportSettings>>(nameof(ImportSettings)).Value = value;
	}

	#endregion

	#region Export Settings
	/// <summary>
	/// The root path to export to
	/// </summary>
	public string ExportRootPath { get; set; } = "";
	/// <summary>
	/// The path to create a new unity project in
	/// </summary>
	public string ProjectRootPath => Path.Combine(ExportRootPath, "ExportedProject");
	public string AssetsPath => Path.Combine(ProjectRootPath, "Assets");
	public string ProjectSettingsPath => Path.Combine(ProjectRootPath, "ProjectSettings");
	public string AuxiliaryFilesPath => Path.Combine(ExportRootPath, "AuxiliaryFiles");
	#endregion

	#region Project Settings
	public UnityVersion Version { get; private set; }
	public BuildTarget Platform { get; private set; }
	public TransferInstructionFlags Flags { get; private set; }
	#endregion

	public SingletonDataStorage SingletonData { get; } = new();
	public ListDataStorage ListData { get; } = new();

	public CoreConfiguration()
	{
		ResetToDefaultValues();
		AddDebugData();
		SingletonData.Add(nameof(ImportSettings), new JsonDataInstance<ImportSettings>(ImportSettingsContext.Default.ImportSettings));
	}

	public void SetProjectSettings(UnityVersion version, BuildTarget platform, TransferInstructionFlags flags)
	{
		Version = version;
		Platform = platform;
		Flags = flags;
	}

	public virtual void ResetToDefaultValues()
	{
		ExportRootPath = ExecutingDirectory.Combine("Ripped");
		SingletonData.Clear();
		ListData.Clear();
	}

	public virtual void LogConfigurationValues()
	{
		Logger.Info(LogCategory.General, $"Configuration Settings:");
		Logger.Info(LogCategory.General, $"{nameof(ExportRootPath)}: {ExportRootPath}");
		ImportSettings.Log();
	}

	[Conditional("DEBUG")]
	private void AddDebugData()
	{
		SingletonData.Add("README", "This is a singleton entry. It is used to store information that can be contained in a single file.");
		ListData.Add("README", ["This is a list entry. It is used to store information that might be contained in multiple files."]);
		ListData.Add("Fibonacci", [1, 1, 2, 3, 5, 8, 13, 21, 34, 55]);
		ListData.Add("Unused Key", []);
	}
}
