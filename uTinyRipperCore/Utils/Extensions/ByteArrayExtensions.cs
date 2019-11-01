using System;
using System.IO;

namespace uTinyRipper
{
	public static class ByteArrayExtensions
	{
		public static byte[] SwapBytes(this byte[] _this, int size)
		{
			byte[] buffer = new byte[_this.Length];
			using (MemoryStream dst = new MemoryStream(buffer))
			{
				using (BinaryWriter writer = new BinaryWriter(dst))
				{
					using (MemoryStream src = new MemoryStream(_this))
					{
						using (EndianReader reader = new EndianReader(src, EndianType.BigEndian))
						{
							if (size == 2)
							{
								for (int i = 0; i < _this.Length; i += 2)
								{
									writer.Write(reader.ReadUInt16());
								}
							}
							else if (size == 4)
							{
								for (int i = 0; i < _this.Length; i += 4)
								{
									writer.Write(reader.ReadUInt32());
								}
							}
							else
							{
								throw new ArgumentException(size.ToString(), nameof(size));
							}
						}
					}
				}
			}
			return buffer;
		}
	}
}
