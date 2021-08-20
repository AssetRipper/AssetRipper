using AssetRipper.Core.IO;

namespace AssetRipper.Core.Reading.Classes
{
	public class ChannelInfo
	{
		public byte stream;
		public byte offset;
		public byte format;
		public byte dimension;

		public ChannelInfo() { }

		public ChannelInfo(ObjectReader reader)
		{
			stream = reader.ReadByte();
			offset = reader.ReadByte();
			format = reader.ReadByte();
			dimension = (byte)(reader.ReadByte() & 0xF);
		}
	}
}
