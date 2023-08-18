using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.Assets.Bundles;

public interface IDependencyProvider
{
	FileBase? FindDependency(FileIdentifier identifier);
	void ReportMissingDependency(FileIdentifier identifier);
}
