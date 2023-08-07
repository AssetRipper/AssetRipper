using System.Runtime.CompilerServices;

namespace AssetRipper.Checksum;

static partial class Crc32Algorithm
{
	/// <summary>
	/// Find a conflicting Ascii string with Gaussian Elimination
	/// </summary>
	/// <remarks>
	/// The output is one-to-one and matches [H-Wh-w]{6}[HJLN] <br />
	/// Based on <see href="https://gitlab.com/-/snippets/2369762">work</see> by <see href="https://gitlab.com/lox9973">lox9973</see> licensed under MIT.
	/// </remarks>
	/// <param name="hash">A CRC32 checksum</param>
	/// <returns>An Ascii string of length 7 with that <paramref name="hash"/></returns>
	public static string ReverseAscii(uint hash)
	{
		return ReverseAscii(hash, default, defaultInitialMask);
	}

	/// <summary>
	/// Find a conflicting Ascii string with Gaussian Elimination
	/// </summary>
	/// <remarks>
	/// The output is one-to-one and starts with <paramref name="prefix"/>. The suffix matches [H-Wh-w]{6}[HJLN] <br />
	/// Based on <see href="https://gitlab.com/-/snippets/2369762">work</see> by <see href="https://gitlab.com/lox9973">lox9973</see> licensed under MIT.
	/// </remarks>
	/// <param name="hash">A CRC32 checksum</param>
	/// <param name="prefix">A span of characters that will be prepended to the output</param>
	/// <returns>An Ascii string of length <paramref name="prefix"/>.Length + 7 with that <paramref name="hash"/></returns>
	public static string ReverseAscii(uint hash, ReadOnlySpan<char> prefix)
	{
		uint reverseInitialMask = HashAscii(prefix, HHHHHHH);
		return ReverseAscii(hash, prefix, reverseInitialMask);
	}

	private static string ReverseAscii(uint hash, ReadOnlySpan<char> prefix, uint reverseInitialMask)
	{
		uint b = hash ^ reverseInitialMask;

		uint x00 = GetMaskFromBit(b, 2);
		b ^= x00 & 0xa6770bb4;
		uint x01 = GetMaskFromBit(b, 0);
		b ^= x01 & 0x979f1129;
		uint x02 = GetMaskFromBit(b, 1);
		b ^= x02 & 0x63d0353a;
		uint x03 = GetMaskFromBit(b, 7);
		b ^= x03 & 0xc3aec380;
		uint x04 = GetMaskFromBit(b, 3);
		b ^= x04 & 0x69ca3228;
		uint x05 = GetMaskFromBit(b, 4);
		b ^= x05 & 0x937efb10;
		uint x06 = GetMaskFromBit(b, 5);
		b ^= x06 & 0x43334c20;
		uint x07 = GetMaskFromBit(b, 9);
		b ^= x07 & 0x241f3a00;
		uint x08 = GetMaskFromBit(b, 6);
		b ^= x08 & 0xf4894d40;
		uint x09 = GetMaskFromBit(b, 8);
		b ^= x09 & 0xfb113100;
		uint x10 = GetMaskFromBit(b, 10);
		b ^= x10 & 0xd5926c00;
		uint x11 = GetMaskFromBit(b, 11);
		b ^= x11 & 0xe9656800;
		uint x12 = GetMaskFromBit(b, 14);
		b ^= x12 & 0x2bc24000;
		uint x13 = GetMaskFromBit(b, 12);
		b ^= x13 & 0xf0023000;
		uint x14 = GetMaskFromBit(b, 13);
		b ^= x14 & 0xaf952000;
		uint x15 = GetMaskFromBit(b, 19);
		b ^= x15 & 0x60d80000;
		uint x16 = GetMaskFromBit(b, 15);
		b ^= x16 & 0x5fe78000;
		uint x17 = GetMaskFromBit(b, 18);
		b ^= x17 & 0x6ec40000;
		uint x18 = GetMaskFromBit(b, 17);
		b ^= x18 & 0xb9b20000;
		uint x19 = GetMaskFromBit(b, 20);
		b ^= x19 & 0x23900000;
		uint x20 = GetMaskFromBit(b, 21);
		b ^= x20 & 0x8c200000;
		uint x21 = GetMaskFromBit(b, 24);
		b ^= x21 & 0x53000000;
		uint x22 = GetMaskFromBit(b, 30);
		b ^= x22 & 0x40000000;
		uint x23 = GetMaskFromBit(b, 27);
		b ^= x23 & 0x98000000;
		uint x24 = GetMaskFromBit(b, 16);
		b ^= x24 & 0xb4010000;
		uint x25 = GetMaskFromBit(b, 25);
		b ^= x25 & 0x22000000;
		uint x26 = GetMaskFromBit(b, 22);
		b ^= x26 & 0x4400000;
		uint x27 = GetMaskFromBit(b, 28);
		b ^= x27 & 0x10000000;
		uint x28 = GetMaskFromBit(b, 23);
		b ^= x28 & 0x20800000;
		uint x29 = GetMaskFromBit(b, 26);
		b ^= x29 & 0xa4000000;
		uint x31 = GetMaskFromBit(b, 29);
		b ^= x31 & 0xa0000000;
		uint x32 = GetMaskFromBit(b, 31);

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
		x09 ^= x10 ^ x17 ^ x18 ^ x19 ^ x20 ^ x25 ^ x26 ^ x28 ^ x29;
		x08 ^= x11 ^ x12 ^ x13 ^ x15 ^ x17 ^ x18 ^ x21 ^ x22 ^ x23 ^ x24 ^ x25 ^ x28;
		x07 ^= x08 ^ x11 ^ x13 ^ x15 ^ x16 ^ x23 ^ x25 ^ x27 ^ x28 ^ x29 ^ x32;
		x06 ^= x07 ^ x10 ^ x11 ^ x14 ^ x16 ^ x17 ^ x18 ^ x19 ^ x21 ^ x23 ^ x24 ^ x27 ^ x28 ^ x29 ^ x31 ^ x32;
		x05 ^= x06 ^ x07 ^ x08 ^ x09 ^ x10 ^ x13 ^ x14 ^ x15 ^ x16 ^ x17 ^ x18 ^ x19 ^ x20 ^ x26 ^ x27 ^ x28 ^ x29 ^ x31 ^ x32;
		x04 ^= x05 ^ x06 ^ x07 ^ x08 ^ x09 ^ x15 ^ x16 ^ x22 ^ x23 ^ x25 ^ x26 ^ x29 ^ x31;
		x03 ^= x07 ^ x09 ^ x10 ^ x12 ^ x14 ^ x15 ^ x16 ^ x17 ^ x19 ^ x20 ^ x21 ^ x23 ^ x24 ^ x26 ^ x27 ^ x29 ^ x31;
		x02 ^= x06 ^ x07 ^ x08 ^ x09 ^ x13 ^ x16 ^ x17 ^ x19 ^ x20 ^ x21 ^ x26 ^ x29;
		x01 ^= x02 ^ x03 ^ x05 ^ x06 ^ x13 ^ x15 ^ x16 ^ x17 ^ x20 ^ x25 ^ x28 ^ x32;
		x00 ^= x04 ^ x05 ^ x07 ^ x08 ^ x09 ^ x14 ^ x15 ^ x17 ^ x19 ^ x20 ^ x21 ^ x22 ^ x27 ^ x31;

		ReadOnlySpan<char> reverseBuffer = stackalloc char[7]
		{
			(char)(72 ^ 1 & x00 ^ 2 & x01 ^ 4 & x02 ^ 24 & x03 ^ 32 & x04),
			(char)(72 ^ 1 & x05 ^ 2 & x06 ^ 4 & x07 ^ 24 & x08 ^ 32 & x09),
			(char)(72 ^ 1 & x10 ^ 2 & x11 ^ 4 & x12 ^ 24 & x13 ^ 32 & x14),
			(char)(72 ^ 1 & x15 ^ 2 & x16 ^ 4 & x17 ^ 24 & x18 ^ 32 & x19),
			(char)(72 ^ 1 & x20 ^ 2 & x21 ^ 4 & x22 ^ 24 & x23 ^ 32 & x24),
			(char)(72 ^ 1 & x25 ^ 2 & x26 ^ 4 & x27 ^ 24 & x28 ^ 32 & x29),
			(char)(72 ^ 2 & x31 ^ 4 & x32)
		};

		return prefix.Length is 0 ? new string(reverseBuffer) : string.Concat(prefix, reverseBuffer);
	}

	/// <summary>
	/// Get the mask for a bit without using branches.
	/// </summary>
	/// <param name="value">The value containing the bit.</param>
	/// <param name="bitOffset">The location of the bit in <paramref name="value"/> by right shifting.</param>
	/// <returns><see cref="uint.MaxValue"/> if the bit is true and <see cref="uint.MinValue"/> if the bit is false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static uint GetMaskFromBit(uint value, int bitOffset)
	{
		return unchecked(~(value >> bitOffset & 1u) + 1u);
	}

	private const string HHHHHHH = "HHHHHHH";

	/// <summary>
	/// Ascii digest of <see cref="HHHHHHH"/>.
	/// </summary>
	private static readonly uint defaultInitialMask = HashAscii(HHHHHHH);
}
