using System.Buffers.Binary;
using System.Text;

namespace Smolv;

public static class SmolvDecoder
{
	public static int GetDecodedBufferSize(ReadOnlySpan<byte> data)
	{
		if (!CheckSmolHeader(data))
		{
			return 0;
		}

		int size = BinaryPrimitives.ReadInt32LittleEndian(data[(5 * sizeof(uint))..]);
		return size;
	}

	public static int GetDecodedBufferSize([NotNull] Stream stream)
	{
		ArgumentNullException.ThrowIfNull(stream);
		if (!stream.CanSeek)
		{
			throw new ArgumentException(null, nameof(stream));
		}
		if (stream.Position + HeaderSize > stream.Length)
		{
			return 0;
		}

		long initPosition = stream.Position;
		stream.Position += HeaderSize - sizeof(uint);
		int size = stream.ReadByte() | (stream.ReadByte() << 8) | (stream.ReadByte() << 16) | (stream.ReadByte() << 24);
		stream.Position = initPosition;
		return size;
	}

	public static byte[]? Decode(byte[] data)
	{
		int bufferSize = GetDecodedBufferSize(data);
		if (bufferSize == 0)
		{
			// invalid SMOL-V
			return null;
		}

		byte[] output = new byte[bufferSize];
		if (Decode(data, output))
		{
			return output;
		}

		return null;
	}

	public static bool Decode(byte[] data, [NotNull] byte[] output)
	{
		ArgumentNullException.ThrowIfNull(output);

		int bufferSize = GetDecodedBufferSize(data);
		if (bufferSize > output.Length)
		{
			return false;
		}

		using MemoryStream outputStream = new MemoryStream(output);
		return Decode(data, outputStream);
	}

	public static bool Decode([NotNull] byte[] data, Stream outputStream)
	{
		ArgumentNullException.ThrowIfNull(data);
		using MemoryStream inputStream = new MemoryStream(data);
		return Decode(inputStream, data.Length, outputStream);
	}

	public static bool Decode([NotNull] Stream inputStream, int inputSize, [NotNull] Stream outputStream)
	{
		ArgumentNullException.ThrowIfNull(inputStream);
		ArgumentNullException.ThrowIfNull(outputStream);
		if (inputStream.Length < HeaderSize)
		{
			return false;
		}

		using BinaryReader input = new BinaryReader(inputStream, Encoding.UTF8, true);
		using BinaryWriter output = new BinaryWriter(outputStream, Encoding.UTF8, true);
		long inputEndPosition = input.BaseStream.Position + inputSize;
		long outputStartPosition = output.BaseStream.Position;

		// Header
		output.Write(SpirVHeaderMagic);
		input.BaseStream.Position += sizeof(uint);
		uint version = input.ReadUInt32();
		output.Write(version & 0x00FFFFFF);
		uint generator = input.ReadUInt32();
		output.Write(generator);
		int bound = input.ReadInt32();
		output.Write(bound);
		uint schema = input.ReadUInt32();
		output.Write(schema);
		int decodedSize = input.ReadInt32();

		// Body
		int prevResult = 0;
		int prevDecorate = 0;
		while (input.BaseStream.Position < inputEndPosition)
		{
			// read length + opcode
			if (!ReadLengthOp(input, out uint instrLen, out SpvOp op))
			{
				return false;
			}

			bool wasSwizzle = op == SpvOp.VectorShuffleCompact;
			if (wasSwizzle)
			{
				op = SpvOp.VectorShuffle;
			}
			output.Write((instrLen << 16) | (uint)op);

			uint ioffs = 1;
			// read type as varint, if we have it
			if (op.OpHasType())
			{
				if (!ReadVarint(input, out uint value))
				{
					return false;
				}

				output.Write(value);
				ioffs++;
			}

			// read result as delta+varint, if we have it
			if (op.OpHasResult())
			{
				if (!ReadVarint(input, out uint value))
				{
					return false;
				}

				int zds = prevResult + ZigDecode(value);
				output.Write(zds);
				prevResult = zds;
				ioffs++;
			}

			// Decorate: IDs relative to previous decorate
			if (op == SpvOp.Decorate || op == SpvOp.MemberDecorate)
			{
				if (!ReadVarint(input, out uint value))
				{
					return false;
				}

				int zds = prevDecorate + unchecked((int)value);
				output.Write(zds);
				prevDecorate = zds;
				ioffs++;
			}

			bool isNewSmolV = false;
			if (op == SpvOp.MemberDecorate && isNewSmolV)
			{
				if (input.BaseStream.Position >= inputEndPosition)
				{
					return false; //Broken input
				}

				long count = input.BaseStream.Position + 1;
				uint prevIndex = 0;
				uint prevOffset = 0;
				for (int m = 0; m < count; m++)
				{
					//read member index
					if (!ReadVarint(input, out uint memberIndex))
					{
						return false;
					}

					memberIndex += prevIndex;
					prevIndex = memberIndex;

					//decoration (and length if not common/known)
					if (!ReadVarint(input, out uint memberDec))
					{
						return false;
					}

					int knownExtraOps = DecorationExtraOps((int)memberDec);
					uint memberLen;
					if (knownExtraOps == -1)
					{
						if (!ReadVarint(input, out memberLen))
						{
							return false;
						}

						memberLen += 4;
					}
					else
					{
						memberLen = (uint)(4 + knownExtraOps);
					}

					// write SPIR-V op+length (unless it's first member decoration, in which case it was written before)
					if (m != 0)
					{
						output.Write((memberLen << 16) | (uint)op);
						output.Write(prevDecorate);
					}
					output.Write(memberIndex);
					output.Write(memberDec);

					if (memberDec == 35) // Offset
					{
						if (memberLen != 5)
						{
							return false;
						}

						if (!ReadVarint(input, out uint val))
						{
							return false;
						}

						val += prevOffset;
						output.Write(val);
						prevOffset = val;
					}
					else
					{
						for (uint i = 4; i < memberLen; ++i)
						{
							if (!ReadVarint(input, out uint val))
							{
								return false;
							}

							output.Write(val);
						}
					}
				}
				continue;
			}

			// Read this many IDs, that are relative to result ID
			int relativeCount = op.OpDeltaFromResult();
			bool inverted = false;
			if (relativeCount < 0)
			{
				inverted = true;
				relativeCount = -relativeCount;
			}
			for (int i = 0; i < relativeCount && ioffs < instrLen; ++i, ++ioffs)
			{
				if (!ReadVarint(input, out uint value))
				{
					return false;
				}

				int zd = inverted ? ZigDecode(value) : unchecked((int)value);
				output.Write(prevResult - zd);
			}

			if (wasSwizzle && instrLen <= 9)
			{
				uint swizzle = input.ReadByte();
				if (instrLen > 5)
				{
					output.Write(swizzle >> 6);
				}

				if (instrLen > 6)
				{
					output.Write((swizzle >> 4) & 3);
				}

				if (instrLen > 7)
				{
					output.Write((swizzle >> 2) & 3);
				}

				if (instrLen > 8)
				{
					output.Write(swizzle & 3);
				}
			}
			else if (op.OpVarRest())
			{
				// read rest of words with variable encoding
				for (; ioffs < instrLen; ++ioffs)
				{
					if (!ReadVarint(input, out uint value))
					{
						return false;
					}
					output.Write(value);
				}
			}
			else
			{
				// read rest of words without any encoding
				for (; ioffs < instrLen; ++ioffs)
				{
					if (input.BaseStream.Position + 4 > input.BaseStream.Length)
					{
						return false;
					}
					uint val = input.ReadUInt32();
					output.Write(val);
				}
			}
		}

		if (output.BaseStream.Position != outputStartPosition + decodedSize)
		{
			// something went wrong during decoding? we should have decoded to exact output size
			return false;
		}

		return true;
	}

	private static bool CheckSmolHeader(ReadOnlySpan<byte> data)
	{
		return CheckGenericHeader(data, SmolHeaderMagic);
	}

	private static bool CheckGenericHeader(ReadOnlySpan<byte> data, uint expectedMagic)
	{
		if (data.Length < HeaderSize)
		{
			return false;
		}

		uint headerMagic = BinaryPrimitives.ReadUInt32LittleEndian(data);
		if (headerMagic != expectedMagic)
		{
			return false;
		}

		uint headerVersion = BinaryPrimitives.ReadUInt32LittleEndian(data[sizeof(uint)..]);
		if (headerVersion < 0x00010000 || headerVersion > 0x00010300)
		{
			// only support 1.0 through 1.3
			return false;
		}

		return true;
	}

	private static bool ReadVarint(BinaryReader input, out uint value)
	{
		uint v = 0;
		int shift = 0;
		while (input.BaseStream.Position < input.BaseStream.Length)
		{
			byte b = input.ReadByte();
			v |= unchecked((uint)(b & 127) << shift);
			shift += 7;
			if ((b & 128) == 0)
			{
				break;
			}
		}

		value = v;
		// @TODO: report failures
		return true;
	}

	private static bool ReadLengthOp(BinaryReader input, out uint len, out SpvOp op)
	{
		len = default;
		op = default;
		if (!ReadVarint(input, out uint value))
		{
			return false;
		}
		len = ((value >> 20) << 4) | ((value >> 4) & 0xF);
		op = (SpvOp)(((value >> 4) & 0xFFF0) | (value & 0xF));

		op = RemapOp(op);
		len = DecodeLen(op, len);
		return true;
	}

	/// <summary>
	/// Remap most common Op codes (Load, Store, Decorate, VectorShuffle etc.) to be in &lt; 16 range, for 
	/// more compact varint encoding. This basically swaps rarely used op values that are &lt; 16 with the
	/// ones that are common.
	/// </summary>
	private static SpvOp RemapOp(SpvOp op)
	{
		return op switch
		{
			// 0: 24%
			SpvOp.Decorate => SpvOp.Nop,
			SpvOp.Nop => SpvOp.Decorate,

			// 1: 17%
			SpvOp.Load => SpvOp.Undef,
			SpvOp.Undef => SpvOp.Load,

			// 2: 9%
			SpvOp.Store => SpvOp.SourceContinued,
			SpvOp.SourceContinued => SpvOp.Store,

			// 3: 7.2%
			SpvOp.AccessChain => SpvOp.Source,
			SpvOp.Source => SpvOp.AccessChain,

			// 4: 5.0%
			// Name - already small enum value - 5: 4.4%
			// MemberName - already small enum value - 6: 2.9% 
			SpvOp.VectorShuffle => SpvOp.SourceExtension,
			SpvOp.SourceExtension => SpvOp.VectorShuffle,

			// 7: 4.0%
			SpvOp.MemberDecorate => SpvOp.String,
			SpvOp.String => SpvOp.MemberDecorate,

			// 8: 0.9%
			SpvOp.Label => SpvOp.Line,
			SpvOp.Line => SpvOp.Label,

			// 9: 3.9%
			SpvOp.Variable => (SpvOp)9,
			(SpvOp)9 => SpvOp.Variable,

			// 10: 3.9%
			SpvOp.FMul => SpvOp.Extension,
			SpvOp.Extension => SpvOp.FMul,

			// 11: 2.5%
			// ExtInst - already small enum value - 12: 1.2%
			// VectorShuffleCompact - already small enum value - used for compact shuffle encoding
			SpvOp.FAdd => SpvOp.ExtInstImport,
			SpvOp.ExtInstImport => SpvOp.FAdd,

			// 14: 2.2%
			SpvOp.TypePointer => SpvOp.MemoryModel,
			SpvOp.MemoryModel => SpvOp.TypePointer,

			// 15: 1.1%
			SpvOp.FNegate => SpvOp.EntryPoint,
			SpvOp.EntryPoint => SpvOp.FNegate,

			_ => op,
		};
	}

	private static uint DecodeLen(SpvOp op, uint len)
	{
		return len + op switch
		{
			SpvOp.VectorShuffle => 4 + 1,
			SpvOp.VectorShuffleCompact => 4 + 1,
			SpvOp.Decorate => 2 + 1,
			SpvOp.Load => 3 + 1,
			SpvOp.AccessChain => 3 + 1,
			_ => (uint)1,
		};
	}

	private static int DecorationExtraOps(int dec)
	{
		// RelaxedPrecision, Block..ColMajor
		if (dec == 0 || (dec >= 2 && dec <= 5))
		{
			return 0;
		}
		// Stream..XfbStride
		if (dec >= 29 && dec <= 37)
		{
			return 1;
		}

		// unknown, encode length
		return -1;
	}

	private static int ZigDecode(uint u)
	{
		return (u & 1) != 0 ? unchecked((int)~(u >> 1)) : unchecked((int)(u >> 1));
	}

	public const uint SpirVHeaderMagic = 0x07230203;
	/// <summary>
	/// 'SMOL' ascii
	/// </summary>
	public const uint SmolHeaderMagic = 0x534D4F4C;

	private const int HeaderSize = 6 * sizeof(uint);
}
