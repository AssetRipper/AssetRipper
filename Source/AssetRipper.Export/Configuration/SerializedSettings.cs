using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Import.Configuration;
using AssetRipper.Import.Utils;
using AssetRipper.Processing.Configuration;
using System.Text.Json;

namespace AssetRipper.Export.Configuration;

public readonly record struct SerializedSettings(ImportSettings Import, ProcessingSettings Processing, ExportSettings Export)
{
	public const string DefaultFileName = "AssetRipper.Settings.json";

	public static string DefaultFilePath => ExecutingDirectory.Combine(DefaultFileName);

	public static SerializedSettings Load(string path)
	{
		return JsonSerializer.Deserialize(File.ReadAllText(path), SerializedSettingsContext.Default.SerializedSettings);
	}

	public static bool TryLoadFromDefaultPath(out SerializedSettings settings)
	{
		string path = DefaultFilePath;
		if (File.Exists(path))
		{
			settings = Load(path);
			return true;
		}
		settings = default;
		return false;
	}

	public void Save(string path)
	{
		using FileStream fileStream = File.Create(path);
		JsonSerializer.Serialize(fileStream, this, SerializedSettingsContext.Default.SerializedSettings);
	}

	public void SaveToDefaultPath()
	{
		Save(DefaultFilePath);
	}

	public static void DeleteDefaultPath()
	{
		string path = DefaultFilePath;
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}
}
