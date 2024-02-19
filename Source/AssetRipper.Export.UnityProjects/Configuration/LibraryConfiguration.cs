using AssetRipper.Import.Configuration;
using AssetRipper.Mining.PredefinedAssets;

namespace AssetRipper.Export.UnityProjects.Configuration;

public class LibraryConfiguration : CoreConfiguration
{
	public ProcessingSettings ProcessingSettings
	{
		get => SingletonData.GetStoredValue<ProcessingSettings>(nameof(ProcessingSettings));
	}

	public ExportSettings ExportSettings
	{
		get => SingletonData.GetStoredValue<ExportSettings>(nameof(ExportSettings));
	}

	public LibraryConfiguration()
	{
		SingletonData.Add(nameof(ProcessingSettings), new JsonDataInstance<ProcessingSettings>(ProcessingExportSettingsContext.Default.ProcessingSettings));
		SingletonData.Add(nameof(ExportSettings), new JsonDataInstance<ExportSettings>(ProcessingExportSettingsContext.Default.ExportSettings));
		SingletonData.Add(nameof(EngineResourceData), new JsonDataInstance<EngineResourceData?>(EngineResourceDataContext.Default.NullableEngineResourceData));
	}

	public override void LogConfigurationValues()
	{
		base.LogConfigurationValues();
		ProcessingSettings.Log();
		ExportSettings.Log();
	}
}
