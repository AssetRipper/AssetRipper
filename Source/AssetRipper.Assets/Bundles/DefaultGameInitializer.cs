using AssetRipper.Assets.IO;

namespace AssetRipper.Assets.Bundles;

public record class DefaultGameInitializer(IDependencyProvider? DependencyProvider = null, IResourceProvider? ResourceProvider = null, UnityVersion DefaultVersion = default) : IGameInitializer
{
	public virtual void OnCreated(GameBundle gameBundle, AssetFactoryBase assetFactory) { }
	public virtual void OnPathsLoaded(GameBundle gameBundle, AssetFactoryBase assetFactory) { }
	public virtual void OnDependenciesInitialized(GameBundle gameBundle, AssetFactoryBase assetFactory) { }
}
