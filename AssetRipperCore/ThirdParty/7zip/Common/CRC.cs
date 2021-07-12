// Common/CRC.cs

namespace SevenZip
{
	public readonly struct CRC
	{
		public CRC(bool _)
		{
			_value = 0xFFFFFFFF;
		}

		public CRC(byte[] data):
			this(true)
		{
			_value = Update(data, 0, data.Length)._value;
		}

		public CRC(byte[] data, int offset, int size) :
			this(true)
		{
			_value = Update(data, offset, size)._value;
		}

		private CRC(uint value)
		{
			_value = value;
		}

		static CRC()
		{
			Table = new uint[256];
			const uint kPoly = 0xEDB88320;
			for (uint i = 0; i < 256; i++)
			{
				uint r = i;
				for (int j = 0; j < 8; j++)
				{
					if ((r & 1) != 0)
					{
						r = (r >> 1) ^ kPoly;
					}
					else
					{
						r >>= 1;
					}
				}
				Table[i] = r;
			}
		}

		public static uint CalculateDigest(byte[] data)
		{
			return new CRC(data).Digest;
		}

		public static uint CalculateDigestAscii(string data)
		{
			CRC crc = new CRC(true);
			for (int i = 0; i < data.Length; i++)
			{
				byte c = (byte)data[i];
				crc = crc.Update(c);
			}
			return crc.Digest;
		}

		public static uint CalculateDigestUTF8(string data)
		{
			byte[] stringData = System.Text.Encoding.UTF8.GetBytes(data);
			return new CRC(stringData).Digest;
		}

		public static uint CalculateDigest(byte[] data, int offset, int size)
		{
			return new CRC(data, offset, size).Digest;
		}

		public static bool VerifyDigestUTF8(string data, uint digest)
		{
			return CalculateDigestUTF8(data) == digest;
		}

		public static bool Verify28DigestUTF8(string data, uint digest)
		{
			return (CalculateDigestUTF8(data) & 0xFFFFFFF) == digest;
		}

		public static bool VerifyDigest(byte[] data, int offset, int size, uint digest)
		{
			return CalculateDigest(data, offset, size) == digest;
		}

		public CRC Update(byte b)
		{
			uint value = Table[unchecked((byte)(_value)) ^ b] ^ (_value >> 8);
			return new CRC(value);
		}

		public CRC Update(byte[] data, int offset, int size)
		{
			uint value = _value;
			for (int i = 0; i < size; i++)
			{
				value = Table[unchecked((byte)(value)) ^ data[offset + i]] ^ (value >> 8);
			}
			return new CRC(value);
		}

		public uint Digest => _value ^ 0xFFFFFFFF;

		public static readonly uint[] Table;

		private readonly uint _value;
	}
}
