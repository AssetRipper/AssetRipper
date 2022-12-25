using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.Assets.Interfaces;

public interface IDependencyProvider
{
	FileBase? FindDependency(FileIdentifier identifier);
}
