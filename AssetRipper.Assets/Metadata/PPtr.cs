namespace AssetRipper.Assets.Metadata;

public readonly record struct PPtr(int FileID, long PathID)
{
	public PPtr(long PathID) : this(0, PathID) { }
}
