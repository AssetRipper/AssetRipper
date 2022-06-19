using System.IO;

namespace AssetRipper.Core.IO.Extensions
{
	public static class StreamExtensions
	{
		private const int BufferSize = 81920;

		public static void Align(this Stream _this) => Align(_this, 4);
		public static void Align(this Stream _this, int alignment)
		{
			long pos = _this.Position;
			long mod = pos % alignment;
			if (mod != 0)
			{
				_this.Position += alignment - mod;
			}
		}

		public static void CopyTo(this Stream source, Stream destination, long size)
		{
			byte[] buffer = new byte[BufferSize];
			for (long left = size; left > 0; left -= BufferSize)
			{
				int toRead = BufferSize < left ? BufferSize : (int)left;
				int read = source.Read(buffer, 0, toRead);
				destination.Write(buffer, 0, read);
				if (read != toRead)
				{
					return;
				}
			}
		}
	}
}
