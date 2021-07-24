using System.IO;

namespace AssetRipper.IO.Extensions
{
	public static class BinaryReaderExtensions
	{
		/// <summary>
		/// Reads the specified number of bytes from the stream, starting from a specified point in the byte array.
		/// </summary>
		/// <param name="_this">The binary reader to read from.</param>
		/// <param name="buffer">The buffer to read data into.</param>
		/// <param name="offset">The starting point in the buffer at which to begin reading into the buffer.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <exception cref="System.ArgumentException"></exception> 
		/// <exception cref="System.ArgumentNullException">buffer is null</exception> 
		/// <exception cref="System.ArgumentOutOfRangeException">index or count is negative</exception> 
		/// <exception cref="System.ObjectDisposedException">The stream is closed</exception> 
		/// <exception cref="System.IO.IOException">An I/O error occurred</exception> 
		public static void ReadBuffer(this BinaryReader _this, byte[] buffer, int offset, int count)
		{
			do
			{
				int read = _this.Read(buffer, offset, count);
				if (read == 0)
				{
					throw new IOException($"No data left");
				}
				offset += read;
				count -= read;
			}
			while (count > 0);
		}
	}
}
