using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.Assets.Interfaces;

public interface IDependencyProvider
{
	File? FindDependency(FileIdentifier identifier);
}
