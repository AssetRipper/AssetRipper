namespace AssetRipper.Core.IO.Endian
{
	public interface IEndianWritable
	{
		void Write(EndianWriter writer);
	}
}
