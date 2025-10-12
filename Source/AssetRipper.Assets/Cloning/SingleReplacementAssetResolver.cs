namespace AssetRipper.Assets.Cloning;

public class SingleReplacementAssetResolver(IUnityObjectBase original, IUnityObjectBase replacement) : IAssetResolver
{
	public IUnityObjectBase Original { get; } = original;
	public IUnityObjectBase Replacement { get; } = replacement;

	public T? Resolve<T>(IUnityObjectBase? asset) where T : IUnityObjectBase
	{
		if (asset == Original)
		{
			return TryCast<T>(Replacement);
		}
		return TryCast<T>(asset);
	}

	private static T? TryCast<T>(IUnityObjectBase? asset) where T : IUnityObjectBase
	{
		return asset is T t ? t : default;
	}
}
