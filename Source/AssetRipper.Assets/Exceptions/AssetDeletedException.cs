namespace AssetRipper.Assets.Exceptions;

public sealed class AssetDeletedException : Exception
{
	public AssetDeletedException() : base("The asset has been deleted and can no longer be referenced.")
	{
	}
}
