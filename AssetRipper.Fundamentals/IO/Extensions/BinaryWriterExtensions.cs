using System.IO;
using System.Text;

namespace AssetRipper.Core.IO.Extensions
{
	public static class BinaryWriterExtensions
	{
		public static void AlignStream(this BinaryWriter writer, int alignment)
		{
			long pos = writer.BaseStream.Position;
			long mod = pos % alignment;
			if (mod != 0)
			{
				writer.Write(new byte[alignment - mod]);
			}
		}

		public static void WriteAlignedString(this BinaryWriter writer, string str)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(str);
			writer.Write(bytes.Length);
			writer.Write(bytes);
			writer.AlignStream(4);
		}
	}
}
