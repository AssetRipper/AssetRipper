namespace AssetRipper.Assets.Cloning;

public interface IAssetResolver
{
	T? Resolve<T>(IUnityObjectBase? asset) where T : IUnityObjectBase;
}
