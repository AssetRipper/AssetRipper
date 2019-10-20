using System;
using System.IO;

namespace uTinyRipper
{
	public static class ByteArrayExtensions
	{
		public static byte[] SwapBytes(this byte[] _this, AlignType align)
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
							if (align == AlignType.Align2)
							{
								for (int i = 0; i < _this.Length; i += 2)
								{
									writer.Write(reader.ReadUInt16());
								}
							}
							else if (align == AlignType.Align4)
							{
								for (int i = 0; i < _this.Length; i += 4)
								{
									writer.Write(reader.ReadUInt32());
								}
							}
							else
							{
								throw new ArgumentException(align.ToString(), nameof(align));
							}
						}
					}
				}
			}
			return buffer;
		}
	}
}
