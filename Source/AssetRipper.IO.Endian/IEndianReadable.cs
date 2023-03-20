namespace AssetRipper.IO.Endian
{
	public interface IEndianReadable<TSelf> where TSelf : IEndianReadable<TSelf>
	{
		static abstract TSelf Read(EndianReader reader);
	}
}
