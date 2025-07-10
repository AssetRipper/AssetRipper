namespace AssetRipper.Assets;

public sealed class EmptyAsset : UnityAssetBase
{
	public static EmptyAsset Instance { get; } = new EmptyAsset();
	private EmptyAsset()
	{
	}
}
