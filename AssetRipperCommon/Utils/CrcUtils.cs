using AssetRipper.Core.Classes;
using ICSharpCode.SharpZipLib.Checksum;
using System;
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
			var segment = new ArraySegment<byte>(data, offset, size);
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
	}
}
