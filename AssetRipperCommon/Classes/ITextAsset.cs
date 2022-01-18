namespace AssetRipper.Core.Classes
{
	public interface ITextAsset : INamedObject
	{
		byte[] Script { get; }
	}
}
