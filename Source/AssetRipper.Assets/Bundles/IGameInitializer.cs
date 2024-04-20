using AssetRipper.Assets.IO;

namespace AssetRipper.Assets.Bundles;

public interface IGameInitializer
{
	IDependencyProvider? DependencyProvider => null;
	IResourceProvider? ResourceProvider => null;
	UnityVersion DefaultVersion => default;
	void OnCreated(GameBundle gameBundle, AssetFactoryBase assetFactory) { }
	void OnPathsLoaded(GameBundle gameBundle, AssetFactoryBase assetFactory) { }
	void OnDependenciesInitialized(GameBundle gameBundle, AssetFactoryBase assetFactory) { }
}
