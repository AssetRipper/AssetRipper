using System.Runtime.CompilerServices;

namespace Dxt
{
    public static class DxtDecoder
    {
		public unsafe static void DecompressDXT1(byte[] input, int width, int height, byte[] output)
		{
			fixed (byte* inputPtr = input)
			{
				fixed (byte* outputPtr = output)
				{
					DecompressDXT1(inputPtr, width, height, outputPtr);
				}
			}
		}

		public unsafe static void DecompressDXT1(byte* input, int width, int height, byte* output)
		{
			int bcw = (width + 3) / 4;
			int bch = (height + 3) / 4;
			int clen_last = (width + 3) % 4 + 1;
			uint* buf = stackalloc uint[16];
			ulong* d = (ulong*)input;
			for (int t = 0; t < bch; t++)
			{
				for (int s = 0; s < bcw; s++, d++)
				{
					DecodeDxt1Block(d, buf);
					int clen = s < bcw - 1 ? 4 : clen_last;
					uint* outputPtr = (uint*)(output + (t * 16 * width + s * 16));
					uint* bufPtr = buf;
					for (int i = 0, y = t * 4; i < 4 && y < height; i++, y++)
					{
						for (int j = 0; j < clen; j++) outputPtr[j] = bufPtr[j];
						outputPtr += width;
						bufPtr += 4;
					}
				}
			}
		}

		public unsafe static void DecompressDXT3(byte[] input, int width, int height, byte[] output)
		{
			fixed (byte* inputPtr = input)
			{
				fixed (byte* outputPtr = output)
				{
					DecompressDXT3(inputPtr, width, height, outputPtr);
				}
			}
		}

		public unsafe static void DecompressDXT3(byte* input, int width, int height, byte* output)
		{
			int bcw = (width + 3) / 4;
			int bch = (height + 3) / 4;
			int clen_last = (width + 3) % 4 + 1;
			uint* buf = stackalloc uint[16];
			ulong* inputPtr = (ulong*)input;
			for (int t = 0; t < bch; t++)
			{
				for (int s = 0; s < bcw; s++, inputPtr += 2)
				{
					DecodeDxt3Block(inputPtr, buf);
					int clen = s < bcw - 1 ? 4 : clen_last;
					uint* outputPtr = (uint*)(output + (t * 16 * width + s * 16));
					uint* bufPtr = buf;
					for (int i = 0, y = t * 4; i < 4 && y < height; i++, y++)
					{
						for (int j = 0; j < clen; j++) outputPtr[j] = bufPtr[j];
						outputPtr += width;
						bufPtr += 4;
					}
				}
			}
		}

		public unsafe static void DecompressDXT5(byte[] input, int width, int height, byte[] output)
		{
			fixed (byte* inputPtr = input)
			{
				fixed (byte* outputPtr = output)
				{
					DecompressDXT5(inputPtr, width, height, outputPtr);
				}
			}
		}

		public unsafe static void DecompressDXT5(byte* input, int width, int height, byte* output)
		{
			int bcw = (width + 3) / 4;
			int bch = (height + 3) / 4;
			int clen_last = (width + 3) % 4 + 1;
			uint* buf = stackalloc uint[16];
			ulong* d = (ulong*)input;
			for (int t = 0; t < bch; t++)
			{
				for (int s = 0; s < bcw; s++, d += 2)
				{
					DecodeDxt5Block(d, buf);
					int clen = s < bcw - 1 ? 4 : clen_last;
					uint* outputPtr = (uint*)(output + (t * 16 * width + s * 16));
					uint* bufPtr = buf;
					for (int i = 0, y = t * 4; i < 4 && y < height; i++, y++)
					{
						for (int j = 0; j < clen; j++) outputPtr[j] = bufPtr[j];
						outputPtr += width;
						bufPtr += 4;
					}
				}
			}
		}

		private unsafe static void DecodeDxt1Block(ulong* input, uint* output)
		{
			int r0, g0, b0, r1, g1, b1;
			ushort q0 = ((ushort*)input)[0];
			ushort q1 = ((ushort*)input)[1];

			r0 = (q0 & 0xf800) >> 8;
			g0 = (q0 & 0x07e0) >> 3;
			b0 = (q0 & 0x001f) << 3;
			r0 |= r0 >> 5;
			g0 |= g0 >> 6;
			b0 |= b0 >> 5;

			r1 = (q1 & 0xf800) >> 8;
			g1 = (q1 & 0x07e0) >> 3;
			b1 = (q1 & 0x001f) << 3;
			r1 |= r1 >> 5;
			g1 |= g1 >> 6;
			b1 |= b1 >> 5;

			uint* c = stackalloc uint[4];
			c[0] = ColorOpaque(r0, g0, b0);
			c[1] = ColorOpaque(r1, g1, b1);
			if (q0 > q1)
			{
				c[2] = ColorOpaque((r0 * 2 + r1) / 3, (g0 * 2 + g1) / 3, (b0 * 2 + b1) / 3);
				c[3] = ColorOpaque((r0 + r1 * 2) / 3, (g0 + g1 * 2) / 3, (b0 + b1 * 2) / 3);
			}
			else
			{
				c[2] = ColorOpaque((r0 + r1) / 2, (g0 + g1) / 2, (b0 + b1) / 2);
			}

			uint d = ((uint*)input)[1];
			output[0] = c[d & 3];
			output[1] = c[d >> 2 & 3];
			output[2] = c[d >> 4 & 3];
			output[3] = c[d >> 6 & 3];
			output[4] = c[d >> 8 & 3];
			output[5] = c[d >> 10 & 3];
			output[6] = c[d >> 12 & 3];
			output[7] = c[d >> 14 & 3];
			output[8] = c[d >> 16 & 3];
			output[9] = c[d >> 18 & 3];
			output[10] = c[d >> 20 & 3];
			output[11] = c[d >> 22 & 3];
			output[12] = c[d >> 24 & 3];
			output[13] = c[d >> 26 & 3];
			output[14] = c[d >> 28 & 3];
			output[15] = c[d >> 30 & 3];
		}

		private unsafe static void DecodeDxt3Block(ulong* input, uint* output)
		{
			int* a = stackalloc int[16];
			byte* inputPtr = (byte*)input;
			a[0] = ((*inputPtr & 0xF) * 0x11) << 24;
			a[1] = ((*inputPtr >> 4) * 0x11) << 24;
			a[2] = ((*(inputPtr + 1) & 0xF) * 0x11) << 24;
			a[3] = ((*(inputPtr + 1) >> 4) * 0x11) << 24;
			a[4] = ((*(inputPtr + 2) & 0xF) * 0x11) << 24;
			a[5] = ((*(inputPtr + 2) >> 4) * 0x11) << 24;
			a[6] = ((*(inputPtr + 3) & 0xF) * 0x11) << 24;
			a[7] = ((*(inputPtr + 3) >> 4) * 0x11) << 24;
			a[8] = ((*(inputPtr + 4) & 0xF) * 0x11) << 24;
			a[9] = ((*(inputPtr + 4) >> 4) * 0x11) << 24;
			a[10] = ((*(inputPtr + 5) & 0xF) * 0x11) << 24;
			a[11] = ((*(inputPtr + 5) >> 4) * 0x11) << 24;
			a[12] = ((*(inputPtr + 6) & 0xF) * 0x11) << 24;
			a[13] = ((*(inputPtr + 6) >> 4) * 0x11) << 24;
			a[14] = ((*(inputPtr + 7) & 0xF) * 0x11) << 24;
			a[15] = ((*(inputPtr + 7) >> 4) * 0x11) << 24;

			int r0, g0, b0, r1, g1, b1;
			ushort q0 = ((ushort*)input)[4];
			ushort q1 = ((ushort*)input)[5];

			r0 = (q0 & 0xf800) >> 8;
			g0 = (q0 & 0x07e0) >> 3;
			b0 = (q0 & 0x001f) << 3;
			r0 |= r0 >> 5;
			g0 |= g0 >> 6;
			b0 |= b0 >> 5;

			r1 = (q1 & 0xf800) >> 8;
			g1 = (q1 & 0x07e0) >> 3;
			b1 = (q1 & 0x001f) << 3;
			r1 |= r1 >> 5;
			g1 |= g1 >> 6;
			b1 |= b1 >> 5;

			int* c = stackalloc int[4];
			c[0] = Color(r0, g0, b0);
			c[1] = Color(r1, g1, b1);
			if (q0 > q1)
			{
				c[2] = Color((r0 * 2 + r1) / 3, (g0 * 2 + g1) / 3, (b0 * 2 + b1) / 3);
				c[3] = Color((r0 + r1 * 2) / 3, (g0 + g1 * 2) / 3, (b0 + b1 * 2) / 3);
			}
			else
			{
				c[2] = Color((r0 + r1) / 2, (g0 + g1) / 2, (b0 + b1) / 2);
			}
			uint d = *((uint*)input + 3);
			output[0] = unchecked((uint)(c[d & 3] | a[0]));
			output[1] = unchecked((uint)(c[d >> 2 & 3] | a[1]));
			output[2] = unchecked((uint)(c[d >> 4 & 3] | a[2]));
			output[3] = unchecked((uint)(c[d >> 6 & 3] | a[3]));
			output[4] = unchecked((uint)(c[d >> 8 & 3] | a[4]));
			output[5] = unchecked((uint)(c[d >> 10 & 3] | a[5]));
			output[6] = unchecked((uint)(c[d >> 12 & 3] | a[6]));
			output[7] = unchecked((uint)(c[d >> 14 & 3] | a[7]));
			output[8] = unchecked((uint)(c[d >> 16 & 3] | a[8]));
			output[9] = unchecked((uint)(c[d >> 18 & 3] | a[9]));
			output[10] = unchecked((uint)(c[d >> 20 & 3] | a[10]));
			output[11] = unchecked((uint)(c[d >> 22 & 3] | a[11]));
			output[12] = unchecked((uint)(c[d >> 24 & 3] | a[12]));
			output[13] = unchecked((uint)(c[d >> 26 & 3] | a[13]));
			output[14] = unchecked((uint)(c[d >> 28 & 3] | a[14]));
			output[15] = unchecked((uint)(c[d >> 30 & 3] | a[15]));
		}

		private unsafe static void DecodeDxt5Block(ulong* input, uint* output)
		{
			int* a = stackalloc int[8];
			a[0] = ((byte*)input)[0];
			a[1] = ((byte*)input)[1];
			if (a[0] > a[1])
			{
				a[2] = ((a[0] * 6 + a[1]) / 7) << 24;
				a[3] = ((a[0] * 5 + a[1] * 2) / 7) << 24;
				a[4] = ((a[0] * 4 + a[1] * 3) / 7) << 24;
				a[5] = ((a[0] * 3 + a[1] * 4) / 7) << 24;
				a[6] = ((a[0] * 2 + a[1] * 5) / 7) << 24;
				a[7] = ((a[0] + a[1] * 6) / 7) << 24;
			}
			else
			{
				a[2] = ((a[0] * 4 + a[1]) / 5) << 24;
				a[3] = ((a[0] * 3 + a[1] * 2) / 5) << 24;
				a[4] = ((a[0] * 2 + a[1] * 3) / 5) << 24;
				a[5] = ((a[0] + a[1] * 4) / 5) << 24;
				a[7] = 255 << 24;
			}
			a[0] <<= 24;
			a[1] <<= 24;

			int r0, g0, b0, r1, g1, b1;
			int q0 = ((ushort*)(input + 1))[0];
			int q1 = ((ushort*)(input + 1))[1];

			r0 = (q0 & 0xf800) >> 8;
			g0 = (q0 & 0x07e0) >> 3;
			b0 = (q0 & 0x001f) << 3;
			r0 |= r0 >> 5;
			g0 |= g0 >> 6;
			b0 |= b0 >> 5;

			r1 = (q1 & 0xf800) >> 8;
			g1 = (q1 & 0x07e0) >> 3;
			b1 = (q1 & 0x001f) << 3;
			r1 |= r1 >> 5;
			g1 |= g1 >> 6;
			b1 |= b1 >> 5;

			int* c = stackalloc int[4];
			c[0] = Color(r0, g0, b0);
			c[1] = Color(r1, g1, b1);
			if (q0 > q1)
			{
				c[2] = Color((r0* 2 + r1) / 3, (g0* 2 + g1) / 3, (b0* 2 + b1) / 3);
				c[3] = Color((r0 + r1* 2) / 3, (g0 + g1* 2) / 3, (b0 + b1* 2) / 3);
			}
			else
			{
				c[2] = Color((r0 + r1) / 2, (g0 + g1) / 2, (b0 + b1) / 2);
			}

			ulong da = *input >> 16;
			uint dc = (uint)(*(input + 1) >> 32);
			output[0] = unchecked((uint)(a[da & 7] | c[dc & 3]));
			output[1] = unchecked((uint)(a[da >> 3 & 7] | c[dc >> 2 & 3]));
			output[2] = unchecked((uint)(a[da >> 6 & 7] | c[dc >> 4 & 3]));
			output[3] = unchecked((uint)(a[da >> 9 & 7] | c[dc >> 6 & 3]));
			output[4] = unchecked((uint)(a[da >> 12 & 7] | c[dc >> 8 & 3]));
			output[5] = unchecked((uint)(a[da >> 15 & 7] | c[dc >> 10 & 3]));
			output[6] = unchecked((uint)(a[da >> 18 & 7] | c[dc >> 12 & 3]));
			output[7] = unchecked((uint)(a[da >> 21 & 7] | c[dc >> 14 & 3]));
			output[8] = unchecked((uint)(a[da >> 24 & 7] | c[dc >> 16 & 3]));
			output[9] = unchecked((uint)(a[da >> 27 & 7] | c[dc >> 18 & 3]));
			output[10] = unchecked((uint)(a[da >> 30 & 7] | c[dc >> 20 & 3]));
			output[11] = unchecked((uint)(a[da >> 33 & 7] | c[dc >> 22 & 3]));
			output[12] = unchecked((uint)(a[da >> 36 & 7] | c[dc >> 24 & 3]));
			output[13] = unchecked((uint)(a[da >> 39 & 7] | c[dc >> 26 & 3]));
			output[14] = unchecked((uint)(a[da >> 42 & 7] | c[dc >> 28 & 3]));
			output[15] = unchecked((uint)(a[da >> 45 & 7] | c[dc >> 30 & 3]));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int Color(int r, int g, int b)
		{
			return r << 16 | g << 8 | b;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static uint ColorOpaque(int r, int g, int b)
		{
			return unchecked((uint)(r << 16 | g << 8 | b | 255 << 24));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Rgb565(int c, out int r, out int g, out int b)
		{
			r = (c & 0xf800) >> 8;
			g = (c & 0x07e0) >> 3;
			b = (c & 0x001f) << 3;
			r |= r >> 5;
			g |= g >> 6;
			b |= b >> 5;
		}
	}
}
