using AssetRipper.IO.Files.ResourceFiles;

namespace AssetRipper.Assets.Interfaces;

public interface IResourceProvider
{
	ResourceFile? FindResource(string identifier);
}
