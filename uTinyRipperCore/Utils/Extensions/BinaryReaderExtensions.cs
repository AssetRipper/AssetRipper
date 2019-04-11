using System;
using System.IO;

namespace uTinyRipper
{
	public static class BinaryReaderExtensions
	{
		public static void ReadBuffer(this BinaryReader _this, byte[] buffer, int offset, int count)
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
	}
}
