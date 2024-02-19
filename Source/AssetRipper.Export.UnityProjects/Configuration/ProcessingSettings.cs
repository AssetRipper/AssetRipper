using AssetRipper.Import.Logging;

namespace AssetRipper.Export.UnityProjects.Configuration;

public sealed record class ProcessingSettings
{
	public bool EnablePrefabOutlining { get; set; }

	public void Log()
	{
		Logger.Info(LogCategory.General, $"{nameof(EnablePrefabOutlining)}: {EnablePrefabOutlining}");
	}
}
