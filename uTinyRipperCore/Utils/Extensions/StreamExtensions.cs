using System;
using System.IO;

namespace uTinyRipper
{
	public static class StreamExtensions
	{
		public static void ReadBuffer(this Stream _this, byte[] buffer, int offset, int count)
		{
			do
			{
				int read = _this.Read(buffer, offset, count);
				if (read == 0)
				{
					throw new Exception($"No data left");
				}
				offset += read;
				count -= read;
			}
			while (count > 0);
		}

		public static void CopyStream(this Stream _this, Stream dstStream)
		{
			byte[] buffer = new byte[BufferSize];
			while (true)
			{
				int offset = 0;
				int count = BufferSize;
				int toWrite = 0;

				int read = 0;
				do
				{
					read = _this.Read(buffer, offset, count);
					offset += read;
					count -= read;
					toWrite += read;
				} while (read != 0);

				dstStream.Write(buffer, 0, toWrite);
				if (toWrite != BufferSize)
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
				int offset = 0;
				int count = toRead;
				while (count > 0)
				{
					int read = _this.Read(buffer, offset, count);
					if (read == 0)
					{
						throw new Exception($"No data left");
					}
					offset += read;
					count -= read;
				}
				dstStream.Write(buffer, 0, toRead);
			}
		}

		private const int BufferSize = 81920;
	}
}
