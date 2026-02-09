namespace AssetRipper.Assets.Cloning;

public class DefaultAssetResolver : IAssetResolver
{
	public static DefaultAssetResolver Shared { get; } = new();

	public virtual T? Resolve<T>(IUnityObjectBase? asset) where T : IUnityObjectBase
	{
		return asset is T t ? t : default;
	}
}
