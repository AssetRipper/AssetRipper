using AssetRipper.Configuration;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Import.Configuration;
using AssetRipper.Mining.PredefinedAssets;
using AssetRipper.Processing.Configuration;

namespace AssetRipper.Export.Configuration;

public class FullConfiguration : CoreConfiguration
{
	public ProcessingSettings ProcessingSettings
	{
		get => SingletonData.GetStoredValue<ProcessingSettings>(nameof(ProcessingSettings));
		set => SingletonData.SetStoredValue(nameof(ProcessingSettings), value);
	}

	public ExportSettings ExportSettings
	{
		get => SingletonData.GetStoredValue<ExportSettings>(nameof(ExportSettings));
		set => SingletonData.SetStoredValue(nameof(ExportSettings), value);
	}

	public bool SaveSettingsToDisk => ExportSettings.SaveSettingsToDisk;

	/// <summary>
	/// Transient runtime state: detected Unity packages (not serialized).
	/// Set during export when PackageDetectionMode is Auto.
	/// </summary>
	public Dictionary<string, string>? DetectedPackages { get; set; }

	/// <summary>
	/// Transient runtime state: assembly GUIDs extracted from Unity package tarballs (not serialized).
	/// Maps assembly names to their .asmdef GUID hex strings for correct script references.
	/// </summary>
	public Dictionary<string, string>? DetectedAssemblyGuids { get; set; }

	public string? LanguageCode
	{
		get => ExportSettings.LanguageCode;
		set => ExportSettings.LanguageCode = value;
	}

	public FullConfiguration()
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

	public void MaybeSaveToDefaultPath()
	{
		if (SaveSettingsToDisk)
		{
			SaveToDefaultPath();
		}
	}
}
