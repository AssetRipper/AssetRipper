using AssetRipper.Import.Configuration;
using AssetRipper.Mining.PredefinedAssets;

namespace AssetRipper.Export.UnityProjects.Configuration;

public class LibraryConfiguration : CoreConfiguration
{
	public ProcessingSettings ProcessingSettings
	{
		get => SingletonData.GetStoredValue<ProcessingSettings>(nameof(ProcessingSettings));
		set => SingletonData.GetValue<JsonDataInstance<ProcessingSettings>>(nameof(ProcessingSettings)).Value = value;
	}

	public ExportSettings ExportSettings
	{
		get => SingletonData.GetStoredValue<ExportSettings>(nameof(ExportSettings));
		set => SingletonData.GetValue<JsonDataInstance<ExportSettings>>(nameof(ExportSettings)).Value = value;
	}

	public bool SaveSettingsToDisk => ExportSettings.SaveSettingsToDisk;

	public LibraryConfiguration()
	{
		SingletonData.Add(nameof(ProcessingSettings), new JsonDataInstance<ProcessingSettings>(SerializedSettingsContext.Default.ProcessingSettings));
		SingletonData.Add(nameof(ExportSettings), new JsonDataInstance<ExportSettings>(SerializedSettingsContext.Default.ExportSettings));
		SingletonData.Add(nameof(EngineResourceData), new JsonDataInstance<EngineResourceData?>(EngineResourceDataContext.Default.NullableEngineResourceData));
	}

	public override void LogConfigurationValues()
	{
		base.LogConfigurationValues();
		ProcessingSettings.Log();
		ExportSettings.Log();
	}

	public void LoadFromDefaultPath()
	{
		if (SerializedSettings.TryLoadFromDefaultPath(out SerializedSettings settings))
		{
			ImportSettings = settings.Import;
			ProcessingSettings = settings.Processing;
			ExportSettings = settings.Export;
		}
	}

	public void SaveToDefaultPath()
	{
		new SerializedSettings(ImportSettings, ProcessingSettings, ExportSettings).SaveToDefaultPath();
	}
}
