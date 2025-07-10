namespace AssetRipper.Assets.Metadata;

public sealed class NullPPtr : UnityAssetBase, IPPtr
{
	public static NullPPtr Instance { get; } = new();
	public int FileID => 0;
	public long PathID => 0;

	private NullPPtr()
	{
	}
}
