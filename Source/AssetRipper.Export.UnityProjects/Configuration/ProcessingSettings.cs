using AssetRipper.Import.Logging;

namespace AssetRipper.Export.UnityProjects.Configuration;

public sealed record class ProcessingSettings
{
	public bool EnablePrefabOutlining { get; set; }
	public bool EnableStaticMeshSeparation { get; set; } = true;
	public bool EnableAssetDeduplication { get; set; }

	public void Log()
	{
		Logger.Info(LogCategory.General, $"{nameof(EnablePrefabOutlining)}: {EnablePrefabOutlining}");
		Logger.Info(LogCategory.General, $"{nameof(EnableStaticMeshSeparation)}: {EnableStaticMeshSeparation}");
		Logger.Info(LogCategory.General, $"{nameof(EnableAssetDeduplication)}: {EnableAssetDeduplication}");
	}
}
