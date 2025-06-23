using AssetRipper.IO.Endian;
using System.Text;

namespace AssetRipper.SourceGenerated.Extensions;

public static class ByteArrayExtensions
{
	public static byte[] SwapBytes(this byte[] _this, int size)
	{
		byte[] buffer = new byte[_this.Length];
		using (MemoryStream dst = new MemoryStream(buffer))
		{
			using BinaryWriter writer = new BinaryWriter(dst);
			using MemoryStream src = new MemoryStream(_this);
			using EndianReader reader = new EndianReader(src, EndianType.BigEndian);
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
		return buffer;
	}

	public static string ToFormattedHex(this byte[] _this)
	{
		StringBuilder sb = new StringBuilder();
		int count = 0;
		foreach (byte b in _this)
		{
			sb.Append(b.ToString("X2"));
			count++;
			if (count >= 16)
			{
				sb.AppendLine();
				count = 0;
			}
			else if (count % 4 == 0)
			{
				sb.Append('\t');
			}
			else
			{
				sb.Append(' ');
			}
		}
		return sb.ToString();
	}
}
