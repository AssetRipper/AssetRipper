using AssetRipper.IO.Files.ResourceFiles;

namespace AssetRipper.Assets.Bundles;

public interface IResourceProvider
{
	ResourceFile? FindResource(string identifier);
}
