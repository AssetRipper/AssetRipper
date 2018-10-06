using System;
using System.IO;

namespace uTinyRipper
{
	public static class StreamExtensions
	{
		public static void CopyStream(this Stream _this, Stream dstStream)
		{
			byte[] buffer = new byte[BufferSize];
			while (true)
			{
				int read = _this.Read(buffer, 0, BufferSize);
				dstStream.Write(buffer, 0, read);
				if (read != BufferSize)
				{
					return;
				}
			}
		}

		public static void CopyStream(this Stream _this, Stream dstStream, long size)
		{
			byte[] buffer = new byte[BufferSize];
			for (long left = size; left > 0; left -= BufferSize)
			{
				int toRead = BufferSize < left ? BufferSize : (int)left;
				int read = _this.Read(buffer, 0, toRead);
				if(read != toRead)
				{
					throw new Exception($"Read {read} but expected {toRead}");
				}
				dstStream.Write(buffer, 0, read);
			}
		}

		private const int BufferSize = 4096;
	}
}
