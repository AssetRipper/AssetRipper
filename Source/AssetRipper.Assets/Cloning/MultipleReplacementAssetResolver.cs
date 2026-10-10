using System.Collections.Frozen;

namespace AssetRipper.Assets.Cloning;

public class MultipleReplacementAssetResolver : IAssetResolver
{
	public FrozenDictionary<IUnityObjectBase, IUnityObjectBase> Replacements { get; }

	public MultipleReplacementAssetResolver(IReadOnlyDictionary<IUnityObjectBase, IUnityObjectBase> replacements)
	{
		Replacements = replacements.ToFrozenDictionary();
	}

	public T? Resolve<T>(IUnityObjectBase? asset) where T : IUnityObjectBase
	{
		if (asset != null && Replacements.TryGetValue(asset, out IUnityObjectBase? replacement))
		{
			return TryCast<T>(replacement);
		}
		return TryCast<T>(asset);
	}

	private static T? TryCast<T>(IUnityObjectBase? asset) where T : IUnityObjectBase
	{
		return asset is T t ? t : default;
	}
}
