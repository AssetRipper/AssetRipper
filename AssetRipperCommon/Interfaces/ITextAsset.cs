namespace AssetRipper.Core.Interfaces
{
	public interface ITextAsset : IHasText
	{
		byte[] Script { get; }
	}
}
