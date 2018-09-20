using System;
using System.IO;

namespace UtinyRipper
{
	public static class ByteArrayExtensions
	{
		public static byte[] SwapBytes(this byte[] _this, AlignType align)
		{
			byte[] destination = new byte[_this.Length];
			using (MemoryStream dstream = new MemoryStream(destination))
			{
				using (BinaryWriter writer = new BinaryWriter(dstream))
				{
					using (MemoryStream sstream = new MemoryStream(_this))
					{
						using (EndianReader reader = new EndianReader(sstream, EndianType.BigEndian))
						{
							switch (align)
							{
								case AlignType.Align2:
									for (int i = 0; i < _this.Length; i += 2)
									{
										writer.Write(reader.ReadUInt16());
									}
									break;

								case AlignType.Align4:
									for (int i = 0; i < _this.Length; i += 4)
									{
										writer.Write(reader.ReadUInt32());
									}
									break;

								default:
									throw new NotSupportedException(align.ToString());
							}
						}
					}
				}
			}
			return destination;
		}
	}
}
