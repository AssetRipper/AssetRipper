using System.Text;

namespace AssetRipper.Import.IO.Extensions
{
	public static class BinaryReaderExtensions
	{
		public static void AlignStream(this BinaryReader reader) => reader.BaseStream.Align();
		public static void AlignStream(this BinaryReader reader, int alignment) => reader.BaseStream.Align(alignment);

		public static string ReadAlignedString(this BinaryReader reader)
		{
			int length = reader.ReadInt32();
			if (length > 0 && length <= reader.BaseStream.Length - reader.BaseStream.Position)
			{
				byte[] stringData = reader.ReadBytes(length);
				string result = Encoding.UTF8.GetString(stringData);
				reader.AlignStream(4);
				return result;
			}
			return "";
		}
	}
}
