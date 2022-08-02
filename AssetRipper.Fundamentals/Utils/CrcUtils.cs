using AssetRipper.Core.Classes;
using ICSharpCode.SharpZipLib.Checksum;
using System.Text;

namespace AssetRipper.Core.Utils
{
	public static class CrcUtils
	{
		public static uint CalculateDigest(byte[] data)
		{
			Crc32 crc = new Crc32();
			crc.Update(data);
			return (uint)crc.Value;
		}

		public static uint CalculateDigestAscii(string data)
		{
			Crc32 crc = new Crc32();
			for (int i = 0; i < data.Length; i++)
			{
				byte b = (byte)data[i];
				crc.Update(b);
			}

			return (uint)crc.Value;
		}

		public static uint CalculateDigestUTF8(string data)
		{
			return CalculateDigest(Encoding.UTF8.GetBytes(data));
		}

		public static uint CalculateDigestUTF8(Utf8StringBase data)
		{
			return CalculateDigest(data.Data);
		}

		public static uint CalculateDigest(byte[] data, int offset, int size)
		{
			Crc32 crc = new Crc32();
			ArraySegment<byte> segment = new ArraySegment<byte>(data, offset, size);
			crc.Update(segment);
			return (uint)crc.Value;
		}

		public static bool VerifyDigestUTF8(string data, uint digest)
		{
			return CalculateDigestUTF8(data) == digest;
		}

		public static bool VerifyDigestUTF8(Utf8StringBase data, uint digest)
		{
			return CalculateDigestUTF8(data) == digest;
		}

		public static bool Verify28DigestUTF8(string data, uint digest)
		{
			return (CalculateDigestUTF8(data) & 0xFFFFFFF) == digest;
		}

		public static bool Verify28DigestUTF8(Utf8StringBase data, uint digest)
		{
			return (CalculateDigestUTF8(data) & 0xFFFFFFF) == digest;
		}

		public static bool VerifyDigest(byte[] data, int offset, int size, uint digest)
		{
			return CalculateDigest(data, offset, size) == digest;
		}

		public static string ReverseCrc32(int crc)
		{
			var b = crc ^ CalculateDigestAscii("HHHHHHH");

			var x0 = -((b >> 2) & 1);
			b ^= x0 & -0x5988f44c;
			var x1 = -((b >> 0) & 1);
			b ^= x1 & -0x6860eed7;
			var x2 = -((b >> 1) & 1);
			b ^= x2 & 0x63d0353a;
			var x3 = -((b >> 7) & 1);
			b ^= x3 & -0x3c513c80;
			var x4 = -((b >> 3) & 1);
			b ^= x4 & 0x69ca3228;
			var x5 = -((b >> 4) & 1);
			b ^= x5 & -0x6c8104f0;
			var x6 = -((b >> 5) & 1);
			b ^= x6 & 0x43334c20;
			var x7 = -((b >> 9) & 1);
			b ^= x7 & 0x241f3a00;
			var x8 = -((b >> 6) & 1);
			b ^= x8 & -0xb76b2c0;
			var x9 = -((b >> 8) & 1);
			b ^= x9 & -0x4eecf00;
			var x10 = -((b >> 10) & 1);
			b ^= x10 & -0x2a6d9400;
			var x11 = -((b >> 11) & 1);
			b ^= x11 & -0x169a9800;
			var x12 = -((b >> 14) & 1);
			b ^= x12 & 0x2bc24000;
			var x13 = -((b >> 12) & 1);
			b ^= x13 & -0xffdd000;
			var x14 = -((b >> 13) & 1);
			b ^= x14 & -0x506ae000;
			var x15 = -((b >> 19) & 1);
			b ^= x15 & 0x60d80000;
			var x16 = -((b >> 15) & 1);
			b ^= x16 & 0x5fe78000;
			var x17 = -((b >> 18) & 1);
			b ^= x17 & 0x6ec40000;
			var x18 = -((b >> 17) & 1);
			b ^= x18 & -0x464e0000;
			var x19 = -((b >> 20) & 1);
			b ^= x19 & 0x23900000;
			var x20 = -((b >> 21) & 1);
			b ^= x20 & -0x73e00000;
			var x21 = -((b >> 24) & 1);
			b ^= x21 & 0x53000000;
			var x22 = -((b >> 30) & 1);
			b ^= x22 & 0x40000000;
			var x23 = -((b >> 27) & 1);
			b ^= x23 & -0x68000000;
			var x24 = -((b >> 16) & 1);
			b ^= x24 & -0x4bff0000;
			var x25 = -((b >> 25) & 1);
			b ^= x25 & 0x22000000;
			var x26 = -((b >> 22) & 1);
			b ^= x26 & 0x4400000;
			var x27 = -((b >> 28) & 1);
			b ^= x27 & 0x10000000;
			var x28 = -((b >> 23) & 1);
			b ^= x28 & 0x20800000;
			var x29 = -((b >> 26) & 1);
			b ^= x29 & -0x5c000000;
			var x31 = -((b >> 29) & 1);
			b ^= x31 & -0x60000000;
			var x32 = -((b >> 31) & 1);

			x29 ^= x31;
			x28 ^= x29;
			x27 ^= x31;
			x26 ^= x29;
			x25 ^= x26 ^ x27 ^ x28 ^ x32;
			x24 ^= x25 ^ x26 ^ x29 ^ x32;
			x23 ^= x24 ^ x25 ^ x27 ^ x28 ^ x29 ^ x31;
			x22 ^= x23 ^ x24 ^ x26 ^ x28 ^ x29 ^ x31 ^ x32;
			x21 ^= x22 ^ x25 ^ x32;
			x20 ^= x22 ^ x28 ^ x32;
			x19 ^= x21 ^ x24 ^ x26 ^ x29 ^ x32;
			x18 ^= x19 ^ x22 ^ x25 ^ x26 ^ x29 ^ x32;
			x17 ^= x20 ^ x23 ^ x26 ^ x28 ^ x32;
			x16 ^= x17 ^ x18 ^ x19 ^ x22 ^ x24 ^ x27;
			x15 ^= x18 ^ x20 ^ x22 ^ x24 ^ x25 ^ x26 ^ x28;
			x14 ^= x16 ^ x19 ^ x20 ^ x23 ^ x25 ^ x28;
			x13 ^= x14 ^ x16 ^ x17 ^ x18 ^ x19 ^ x22 ^ x23 ^ x25 ^ x28 ^ x29 ^ x32;
			x12 ^= x13 ^ x14 ^ x19 ^ x21 ^ x23 ^ x25 ^ x31;
			x11 ^= x12 ^ x13 ^ x18 ^ x19 ^ x21 ^ x23 ^ x24 ^ x25 ^ x28 ^ x32;
			x10 ^= x13 ^ x18 ^ x19 ^ x20 ^ x22 ^ x24 ^ x25 ^ x26 ^ x29 ^ x31;
			x9 ^= x10 ^ x17 ^ x18 ^ x19 ^ x20 ^ x25 ^ x26 ^ x28 ^ x29;
			x8 ^= x11 ^ x12 ^ x13 ^ x15 ^ x17 ^ x18 ^ x21 ^ x22 ^ x23 ^ x24 ^ x25 ^ x28;
			x7 ^= x8 ^ x11 ^ x13 ^ x15 ^ x16 ^ x23 ^ x25 ^ x27 ^ x28 ^ x29 ^ x32;
			x6 ^= x7 ^ x10 ^ x11 ^ x14 ^ x16 ^ x17 ^ x18 ^ x19 ^ x21 ^ x23 ^ x24 ^ x27 ^ x28 ^ x29 ^ x31 ^ x32;
			x5 ^= x6 ^ x7 ^ x8 ^ x9 ^ x10 ^ x13 ^ x14 ^ x15 ^ x16 ^ x17 ^ x18 ^ x19 ^ x20 ^ x26 ^ x27 ^ x28 ^ x29 ^
			      x31 ^ x32;
			x4 ^= x5 ^ x6 ^ x7 ^ x8 ^ x9 ^ x15 ^ x16 ^ x22 ^ x23 ^ x25 ^ x26 ^ x29 ^ x31;
			x3 ^= x7 ^ x9 ^ x10 ^ x12 ^ x14 ^ x15 ^ x16 ^ x17 ^ x19 ^ x20 ^ x21 ^ x23 ^ x24 ^ x26 ^ x27 ^ x29 ^ x31;
			x2 ^= x6 ^ x7 ^ x8 ^ x9 ^ x13 ^ x16 ^ x17 ^ x19 ^ x20 ^ x21 ^ x26 ^ x29;
			x1 ^= x2 ^ x3 ^ x5 ^ x6 ^ x13 ^ x15 ^ x16 ^ x17 ^ x20 ^ x25 ^ x28 ^ x32;
			x0 ^= x4 ^ x5 ^ x7 ^ x8 ^ x9 ^ x14 ^ x15 ^ x17 ^ x19 ^ x20 ^ x21 ^ x22 ^ x27 ^ x31;

			return new string(new[]
			{
				(char)(72 ^ (1 & x0) ^ (2 & x1) ^ (4 & x2) ^ (24 & x3) ^ (32 & x4)),
				(char)(72 ^ (1 & x5) ^ (2 & x6) ^ (4 & x7) ^ (24 & x8) ^ (32 & x9)),
				(char)(72 ^ (1 & x10) ^ (2 & x11) ^ (4 & x12) ^ (24 & x13) ^ (32 & x14)),
				(char)(72 ^ (1 & x15) ^ (2 & x16) ^ (4 & x17) ^ (24 & x18) ^ (32 & x19)),
				(char)(72 ^ (1 & x20) ^ (2 & x21) ^ (4 & x22) ^ (24 & x23) ^ (32 & x24)),
				(char)(72 ^ (1 & x25) ^ (2 & x26) ^ (4 & x27) ^ (24 & x28) ^ (32 & x29)),
				(char)(72 ^ (2 & x31) ^ (4 & x32)),
			});
		}
	}
}
