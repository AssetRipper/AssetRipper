using AssetRipper.Import.Logging;

namespace AssetRipper.Processing.Configuration;

public sealed record class ProcessingSettings
{
	public bool EnablePrefabOutlining { get; set; } = false;
	public bool EnableStaticMeshSeparation { get; set; } = true;
	public bool EnableAssetDeduplication { get; set; } = false;
	public bool RemoveNullableAttributes { get; set; } = false;
	public bool PublicizeAssemblies { get; set; } = false;
	public BundledAssetsExportMode BundledAssetsExportMode { get; set; } = BundledAssetsExportMode.DirectExport;

	public void Log()
	{
		Logger.Info(LogCategory.General, $"{nameof(EnablePrefabOutlining)}: {EnablePrefabOutlining}");
		Logger.Info(LogCategory.General, $"{nameof(EnableStaticMeshSeparation)}: {EnableStaticMeshSeparation}");
		Logger.Info(LogCategory.General, $"{nameof(EnableAssetDeduplication)}: {EnableAssetDeduplication}");
		Logger.Info(LogCategory.General, $"{nameof(BundledAssetsExportMode)}: {BundledAssetsExportMode}");
	}
}
