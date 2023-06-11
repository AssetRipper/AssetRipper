using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace AssetRipper.Primitives
{
	public readonly record struct UnityGUID
	{
		public UnityGUID(Guid guid)
		{
			Span<byte> guidData = stackalloc byte[16];
			bool success = guid.TryWriteBytes(guidData);
			Debug.Assert(success);
			ConvertSystemOrUnityBytes(guidData, guidData);
			Data0 = ReadUInt32LittleEndian(guidData, 0);
			Data1 = ReadUInt32LittleEndian(guidData, 1);
			Data2 = ReadUInt32LittleEndian(guidData, 2);
			Data3 = ReadUInt32LittleEndian(guidData, 3);
		}

		public UnityGUID(ReadOnlySpan<byte> guidData)
		{
			Data0 = ReadUInt32LittleEndian(guidData, 0);
			Data1 = ReadUInt32LittleEndian(guidData, 1);
			Data2 = ReadUInt32LittleEndian(guidData, 2);
			Data3 = ReadUInt32LittleEndian(guidData, 3);
		}

		public UnityGUID(uint data0, uint data1, uint data2, uint data3)
		{
			Data0 = data0;
			Data1 = data1;
			Data2 = data2;
			Data3 = data3;
		}

		public static UnityGUID NewGuid()
		{
			//This is not an acceptable way to convert between Unity and System Guids.
			//We only do it this way to efficiently get 16 random bytes.
			//We don't care about official Guid validity because Unity does not care either.
			Guid guid = Guid.NewGuid();
			ReadOnlySpan<Guid> guidSpan = new ReadOnlySpan<Guid>(in guid);
			ReadOnlySpan<byte> byteSpan = MemoryMarshal.AsBytes(guidSpan);
			return new UnityGUID(byteSpan);
		}

		public static explicit operator UnityGUID(Guid systemGuid) => new UnityGUID(systemGuid);

		public static explicit operator Guid(UnityGUID unityGuid)
		{
			Span<byte> span = stackalloc byte[16];
			unityGuid.Write(span);
			ConvertSystemOrUnityBytes(span, span);
			return new Guid(span);
		}

		private void Write(Span<byte> span)
		{
			BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(0 * sizeof(uint), sizeof(uint)), Data0);
			BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(1 * sizeof(uint), sizeof(uint)), Data1);
			BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(2 * sizeof(uint), sizeof(uint)), Data2);
			BinaryPrimitives.WriteUInt32LittleEndian(span.Slice(3 * sizeof(uint), sizeof(uint)), Data3);
		}

		public byte[] ToByteArray()
		{
			byte[] result = new byte[16];
			Write(result);
			return result;
		}

		public override string ToString()
		{
			Span<char> span = stackalloc char[32]
			{
				GetHexChar(Data0, 0),
				GetHexChar(Data0, 4),
				GetHexChar(Data0, 8),
				GetHexChar(Data0, 12),
				GetHexChar(Data0, 16),
				GetHexChar(Data0, 20),
				GetHexChar(Data0, 24),
				GetHexChar(Data0, 28),
				GetHexChar(Data1, 0),
				GetHexChar(Data1, 4),
				GetHexChar(Data1, 8),
				GetHexChar(Data1, 12),
				GetHexChar(Data1, 16),
				GetHexChar(Data1, 20),
				GetHexChar(Data1, 24),
				GetHexChar(Data1, 28),
				GetHexChar(Data2, 0),
				GetHexChar(Data2, 4),
				GetHexChar(Data2, 8),
				GetHexChar(Data2, 12),
				GetHexChar(Data2, 16),
				GetHexChar(Data2, 20),
				GetHexChar(Data2, 24),
				GetHexChar(Data2, 28),
				GetHexChar(Data3, 0),
				GetHexChar(Data3, 4),
				GetHexChar(Data3, 8),
				GetHexChar(Data3, 12),
				GetHexChar(Data3, 16),
				GetHexChar(Data3, 20),
				GetHexChar(Data3, 24),
				GetHexChar(Data3, 28),
			};

			return new string(span);

			static char GetHexChar(uint value, int offset)
			{
				int index = unchecked((int)((value >> offset) & 0xF));
				return "0123456789abcdef"[index];
			}
		}

		/// <summary>
		/// Read little-endian <see cref="uint"/> from <see cref="Span{byte}"/>
		/// </summary>
		/// <param name="byteSpan">A span of bytes.</param>
		/// <param name="index">The ith <see cref="uint"/> in <paramref name="byteSpan"/>.</param>
		/// <returns></returns>
		private static uint ReadUInt32LittleEndian(ReadOnlySpan<byte> byteSpan, int index)
		{
			return BinaryPrimitives.ReadUInt32LittleEndian(byteSpan.Slice(index * sizeof(uint), sizeof(uint)));
		}

		/// <summary>
		/// Converts system bytes to unity bytes, or the reverse
		/// </summary>
		/// <param name="input">A 16 byte input span</param>
		/// <returns>The same span: <paramref name="input"/></returns>
		/// <exception cref="ArgumentException">Span doesn't have 16 elements</exception>
		private static void ConvertSystemOrUnityBytes(scoped ReadOnlySpan<byte> input, scoped Span<byte> output)
		{
			if (input.Length != 16)
			{
				throw new ArgumentException($"Invalid length: {input.Length}", nameof(input));
			}
			if (output.Length != 16)
			{
				throw new ArgumentException($"Invalid length: {output.Length}", nameof(output));
			}

			//Unity Guid's are in big endian, so the bytes have to be flipped for multibyte fields
			(output[0], output[1], output[2], output[3]) = (input[3], input[2], input[1], input[0]);
			(output[4], output[5]) = (input[5], input[4]);
			(output[6], output[7]) = (input[7], input[6]);
			input.Slice(8).CopyTo(output.Slice(8));
			for (int i = 0; i < 16; i++)
			{
				//AB becomes BA
				uint value = output[i];
				output[i] = (byte)(unchecked(value << 4 & 0xF0) | unchecked(value >> 4 & 0xF));
			}
		}

		public static UnityGUID Parse(string guidString) => new UnityGUID(Guid.Parse(guidString));

		/// <summary>
		/// Make a guid by MD5 hashing a string
		/// </summary>
		/// <remarks>
		/// The returned guid is most likely not "valid" by official standards. However, Unity doesn't seem to care.
		/// </remarks>
		/// <param name="str">Input string. Can be any length</param>
		/// <returns>A stable guid corresponding to the input string</returns>
		public static UnityGUID Md5Hash(string str) => Md5Hash(Encoding.UTF8.GetBytes(str));

		/// <summary>
		/// Make a guid by MD5 hashing some input data
		/// </summary>
		/// <remarks>
		/// The returned guid is most likely not "valid" by official standards. However, Unity doesn't seem to care.
		/// </remarks>
		/// <param name="input">Input data. Can be any length</param>
		/// <returns>A stable guid corresponding to the <paramref name="input"/>.</returns>
		public static UnityGUID Md5Hash(scoped ReadOnlySpan<byte> input)
		{
			byte[] hashBytes = MD5.HashData(input);
			ConvertSystemOrUnityBytes(hashBytes, hashBytes);
			return new UnityGUID(hashBytes);
		}

		public static UnityGUID Md5Hash(scoped ReadOnlySpan<byte> assemblyName, scoped ReadOnlySpan<byte> @namespace, scoped ReadOnlySpan<byte> className)
		{
			int length = assemblyName.Length + @namespace.Length + className.Length;
			Span<byte> input = length < 1024 ? stackalloc byte[length] : GC.AllocateUninitializedArray<byte>(length);
			assemblyName.CopyTo(input);
			@namespace.CopyTo(input.Slice(assemblyName.Length));
			className.CopyTo(input.Slice(assemblyName.Length + @namespace.Length));
			return Md5Hash(input);
		}

		public bool IsZero => Data0 == 0 && Data1 == 0 && Data2 == 0 && Data3 == 0;

		public uint Data0 { get; }
		public uint Data1 { get; }
		public uint Data2 { get; }
		public uint Data3 { get; }

		/// <summary>
		/// 0x0000000DEADBEEF15DEADF00D0000000
		/// </summary>
		public static UnityGUID MissingReference { get; } = new UnityGUID(0xD0000000, 0x1FEEBDAE, 0x00FDAED5, 0x0000000D);

		public static UnityGUID Zero => default;

		[ThreadStatic]
		private static StringBuilder? s_sb;

		private static StringBuilder GetStringBuilder()
		{
			s_sb ??= new StringBuilder(32, 32);
			return s_sb;
		}
	}
}
