using System.Runtime.CompilerServices;

namespace Etc
{
	public static class EtcDecoder
	{
		public unsafe static void DecompressETC(byte[] input, int width, int height, byte[] output)
		{
			fixed (byte* inputPtr = input)
			{
				fixed (byte* outputPtr = output)
				{
					DecompressETC(inputPtr, width, height, outputPtr);
				}
			}
		}

		public unsafe static void DecompressETC(byte* input, int width, int height, byte* output)
		{
			int bcw = (width + 3) / 4;
			int bch = (height + 3) / 4;
			int clen_last = (width + 3) % 4 + 1;
			uint* buf = stackalloc uint[16];
			for (int t = 0; t < bch; t++)
			{
				for (int s = 0; s < bcw; s++, input += 8)
				{
					DecodeEtc1Block(input, buf);
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

		public unsafe static void DecompressETC2(byte[] input, int width, int height, byte[] output)
		{
			fixed (byte* inputPtr = input)
			{
				fixed (byte* outputPtr = output)
				{
					DecompressETC2(inputPtr, width, height, outputPtr);
				}
			}
		}

		public unsafe static void DecompressETC2(byte* input, int width, int height, byte* output)
		{
			int bcw = (width + 3) / 4;
			int bch = (height + 3) / 4;
			int clen_last = (width + 3) % 4 + 1;
			uint* buf = stackalloc uint[16];
			for (int t = 0; t < bch; t++)
			{
				for (int s = 0; s < bcw; s++, input += 8)
				{
					DecodeEtc2Block(input, buf);
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

		public unsafe static void DecompressETC2A1(byte[] input, int width, int height, byte[] output)
		{
			fixed (byte* inputPtr = input)
			{
				fixed (byte* outputPtr = output)
				{
					DecompressETC2A1(inputPtr, width, height, outputPtr);
				}
			}
		}

		public unsafe static void DecompressETC2A1(byte* input, int width, int height, byte* output)
		{
			int width4 = width * 4;
			int bcw = (width + 3) / 4;
			int bch = (height + 3) / 4;
			int clen_last = (width + 3) % 4 + 1;
			uint* buf = stackalloc uint[16];
			for (int t = 0; t < bch; t++)
			{
				for (int s = 0; s < bcw; s++, input += 8)
				{
					DecodeEtc2a1Block(input, buf);
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

		public unsafe static void DecompressETC2A8(byte[] input, int width, int height, byte[] output)
		{
			fixed (byte* inputPtr = input)
			{
				fixed (byte* outputPtr = output)
				{
					DecompressETC2A8(inputPtr, width, height, outputPtr);
				}
			}
		}

		public unsafe static void DecompressETC2A8(byte* input, int width, int height, byte* output)
		{
			int bcw = (width + 3) / 4;
			int bch = (height + 3) / 4;
			int clen_last = (width + 3) % 4 + 1;
			uint* buf = stackalloc uint[16];
			for (int t = 0; t < bch; t++)
			{
				for (int s = 0; s < bcw; s++, input += 16)
				{
					DecodeEtc2Block(input + 8, buf);
					DecodeEtc2a8Block(input, buf);
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

		private unsafe static void DecodeEtc1Block(byte* input, uint* output)
		{
			byte i3 = input[3];
			int* code = stackalloc int[2];
			code[0] = (i3 >> 5) * 4;
			code[1] = (i3 >> 2 & 7) * 4;
			byte* c = stackalloc byte[6];
			int ti = (i3 & 1) * 16;
			if ((i3 & 2) != 0)
			{
				unchecked
				{
					c[0] = (byte)(input[0] & 0xf8);
					c[1] = (byte)(input[1] & 0xf8);
					c[2] = (byte)(input[2] & 0xf8);
					c[3] = (byte)(c[0] + (input[0] << 3 & 0x18) - (input[0] << 3 & 0x20));
					c[4] = (byte)(c[1] + (input[1] << 3 & 0x18) - (input[1] << 3 & 0x20));
					c[5] = (byte)(c[2] + (input[2] << 3 & 0x18) - (input[2] << 3 & 0x20));
					c[0] |= (byte)(c[0] >> 5);
					c[1] |= (byte)(c[1] >> 5);
					c[2] |= (byte)(c[2] >> 5);
					c[3] |= (byte)(c[3] >> 5);
					c[4] |= (byte)(c[4] >> 5);
					c[5] |= (byte)(c[5] >> 5);
				}
			}
			else
			{
				unchecked
				{
					c[0] = (byte)(input[0] & 0xf0 | input[0] >> 4);
					c[3] = (byte)(input[0] & 0x0f | input[0] << 4);
					c[1] = (byte)(input[1] & 0xf0 | input[1] >> 4);
					c[4] = (byte)(input[1] & 0x0f | input[1] << 4);
					c[2] = (byte)(input[2] & 0xf0 | input[2] >> 4);
					c[5] = (byte)(input[2] & 0x0f | input[2] << 4);
				}
			}

			int j = input[6] << 8 | input[7];
			int k = input[4] << 8 | input[5];
			for (int i = 0; i < 16; i++, j >>= 1, k >>= 1)
			{
				int s = Etc1SubblockTable[ti + i];
				int index = k << 1 & 2 | j & 1;
				int cd = code[s];
				int m = Etc1ModifierTable[cd + index];
				uint color = ApplicateColor(c, s, m);
				int oi = WriteOrderTable[i];
				output[oi] = color;
			}
		}

		private unsafe static void DecodeEtc2Block(byte* input, uint* output)
		{
			int j = input[6] << 8 | input[7];
			int k = input[4] << 8 | input[5];
			byte* c = stackalloc byte[3 * 3];

			if ((input[3] & 2) != 0)
			{
				int r = input[0] & 0xf8;
				int dr = (input[0] << 3 & 0x18) - (input[0] << 3 & 0x20);
				if (r + dr < 0 || r + dr > 255)
				{
					// T
					unchecked
					{
						c[0] = (byte)(input[0] << 3 & 0xc0 | input[0] << 4 & 0x30 | input[0] >> 1 & 0xc | input[0] & 3);
						c[1] = (byte)(input[1] & 0xf0 | input[1] >> 4);
						c[2] = (byte)(input[1] & 0x0f | input[1] << 4);
						c[3] = (byte)(input[2] & 0xf0 | input[2] >> 4);
						c[4] = (byte)(input[2] & 0x0f | input[2] << 4);
						c[5] = (byte)(input[3] & 0xf0 | input[3] >> 4);
					}
					int ti = input[3] >> 1 & 6 | input[3] & 1;
					byte d = Etc2DistanceTable[ti];
					uint* color_set = stackalloc uint[]
					{
						ApplicateColorRaw(c, 0),
						ApplicateColor(c, 1, d),
						ApplicateColorRaw(c, 1),
						ApplicateColor(c, 1, -d)
					};
					
					for (int i = 0; i < 16; i++, j >>= 1, k >>= 1)
					{
						int index = k << 1 & 2 | j & 1;
						uint color = color_set[index];
						int oi = WriteOrderTable[i];
						output[oi] = color;
					}
				}
				else
				{
					int g = input[1] & 0xf8;
					int dg = (input[1] << 3 & 0x18) - (input[1] << 3 & 0x20);
					if (g + dg < 0 || g + dg > 255)
					{
						// H
						unchecked
						{
							c[0] = (byte)(input[0] << 1 & 0xf0 | input[0] >> 3 & 0xf);
							c[1] = (byte)(input[0] << 5 & 0xe0 | input[1] & 0x10);
							c[1] |= (byte)(c[0 * 3 + 1] >> 4);
							c[2] = (byte)(input[1] & 8 | input[1] << 1 & 6 | input[2] >> 7);
							c[2] |= (byte)(c[0 * 3 + 2] << 4);
							c[3] = (byte)(input[2] << 1 & 0xf0 | input[2] >> 3 & 0xf);
							c[4] = (byte)(input[2] << 5 & 0xe0 | input[3] >> 3 & 0x10);
							c[4] |= (byte)(c[1 * 3 + 1] >> 4);
							c[5] = (byte)(input[3] << 1 & 0xf0 | input[3] >> 3 & 0xf);
						}
						int di = input[3] & 4 | input[3] << 1 & 2;
						if (c[0] > c[3] || (c[0] == c[3] && (c[1] > c[4] || (c[1] == c[4] && c[2] >= c[5]))))
						{
							++di;
						}
						byte d = Etc2DistanceTable[di];
						uint* color_set = stackalloc uint[]
						{
							ApplicateColor(c, 0, d),
							ApplicateColor(c, 0, -d),
							ApplicateColor(c, 1, d),
							ApplicateColor(c, 1, -d)
						};
						for (int i = 0; i < 16; i++, j >>= 1, k >>= 1)
						{
							int index = k << 1 & 2 | j & 1;
							uint color = color_set[index];
							int oi = WriteOrderTable[i];
							output[oi] = color;
						}
					}
					else
					{
						int b = input[2] & 0xf8;
						int db = (input[2] << 3 & 0x18) - (input[2] << 3 & 0x20);
						if (b + db < 0 || b + db > 255)
						{
							// planar
							unchecked
							{
								c[0] = (byte)(input[0] << 1 & 0xfc | input[0] >> 5 & 3);
								c[1] = (byte)(input[0] << 7 & 0x80 | input[1] & 0x7e | input[0] & 1);
								c[2] = (byte)(input[1] << 7 & 0x80 | input[2] << 2 & 0x60 | input[2] << 3 & 0x18 | input[3] >> 5 & 4);
								c[2] |= (byte)(c[0 * 3 + 2] >> 6);
								c[3] = (byte)(input[3] << 1 & 0xf8 | input[3] << 2 & 4 | input[3] >> 5 & 3);
								c[4] = (byte)(input[4] & 0xfe | input[4] >> 7);
								c[5] = (byte)(input[4] << 7 & 0x80 | input[5] >> 1 & 0x7c);
								c[5] |= (byte)(c[1 * 3 + 2] >> 6);
								c[6] = (byte)(input[5] << 5 & 0xe0 | input[6] >> 3 & 0x1c | input[5] >> 1 & 3);
								c[7] = (byte)(input[6] << 3 & 0xf8 | input[7] >> 5 & 0x6 | input[6] >> 4 & 1);
								c[8] = (byte)(input[7] << 2 | input[7] >> 4 & 3);
							}
							for (int y = 0, i = 0; y < 4; y++)
							{
								for (int x = 0; x < 4; x++, i++)
								{
									int ri = Clamp((x * (c[1 * 3 + 0] - c[0 * 3 + 0]) + y * (c[2 * 3 + 0] - c[0 * 3 + 0]) + 4 * c[0 * 3 + 0] + 2) >> 2);
									int gi = Clamp((x * (c[1 * 3 + 1] - c[0 * 3 + 1]) + y * (c[2 * 3 + 1] - c[0 * 3 + 1]) + 4 * c[0 * 3 + 1] + 2) >> 2);
									int bi = Clamp((x * (c[1 * 3 + 2] - c[0 * 3 + 2]) + y * (c[2 * 3 + 2] - c[0 * 3 + 2]) + 4 * c[0 * 3 + 2] + 2) >> 2);
									output[i] = Color(ri, gi, bi, 255);
								}
							}
						}
						else
						{
							// differential
							int* code = stackalloc int[2];
							code[0] = (input[3] >> 5) * 4;
							code[1] = (input[3] >> 2 & 7) * 4;
							int ti = (input[3] & 1) * 16;
							unchecked
							{
								c[0] = (byte)(r | r >> 5);
								c[1] = (byte)(g | g >> 5);
								c[2] = (byte)(b | b >> 5);
								c[3] = (byte)(r + dr);
								c[4] = (byte)(g + dg);
								c[5] = (byte)(b + db);
								c[3] |= (byte)(c[1 * 3 + 0] >> 5);
								c[4] |= (byte)(c[1 * 3 + 1] >> 5);
								c[5] |= (byte)(c[1 * 3 + 2] >> 5);
							}
							for (int i = 0; i < 16; i++, j >>= 1, k >>= 1)
							{
								int s = Etc1SubblockTable[ti + i];
								int index = k << 1 & 2 | j & 1;
								int ci = code[s];
								int m = Etc1ModifierTable[ci + index];
								uint color = ApplicateColor(c, s, m);
								int oi = WriteOrderTable[i];
								output[oi] = color;
							}
						}
					}
				}
			}
			else
			{
				// individual
				int* code = stackalloc int[2];
				code[0] = (input[3] >> 5) * 4;
				code[1] = (input[3] >> 2 & 7) * 4;
				int ti = (input[3] & 1) * 16;
				unchecked
				{
					c[0] = (byte)(input[0] & 0xf0 | input[0] >> 4);
					c[3] = (byte)(input[0] & 0x0f | input[0] << 4);
					c[1] = (byte)(input[1] & 0xf0 | input[1] >> 4);
					c[4] = (byte)(input[1] & 0x0f | input[1] << 4);
					c[2] = (byte)(input[2] & 0xf0 | input[2] >> 4);
					c[5] = (byte)(input[2] & 0x0f | input[2] << 4);
				}
				for (int i = 0; i < 16; i++, j >>= 1, k >>= 1)
				{
					int s = Etc1SubblockTable[ti + i];
					int index = k << 1 & 2 | j & 1;
					int ci = code[s];
					int m = Etc1ModifierTable[ci + index];
					uint color = ApplicateColor(c, s, m);
					int oi = WriteOrderTable[i];
					output[oi] = color;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe static void DecodeEtc2a1Block(byte* input, uint* output)
		{
			if ((input[3] & 2) != 0)
			{
				// Opaque
				DecodeEtc2Block(input, output);
			}
			else
			{
				DecodeEtc2PunchThrowBlock(input, output);
			}
		}

		private unsafe static void DecodeEtc2PunchThrowBlock(byte* input, uint* output)
		{
			int j = input[6] << 8 | input[7];
			int k = input[4] << 8 | input[5];
			byte* c = stackalloc byte[3 * 3];
			int r = input[0] & 0xf8;
			int dr = (input[0] << 3 & 0x18) - (input[0] << 3 & 0x20);
			if (r + dr < 0 || r + dr > 255)
			{
				// T (Etc2Block + mask for color)
				unchecked
				{
					c[0] = (byte)(input[0] << 3 & 0xc0 | input[0] << 4 & 0x30 | input[0] >> 1 & 0xc | input[0] & 3);
					c[1] = (byte)(input[1] & 0xf0 | input[1] >> 4);
					c[2] = (byte)(input[1] & 0x0f | input[1] << 4);
					c[3] = (byte)(input[2] & 0xf0 | input[2] >> 4);
					c[4] = (byte)(input[2] & 0x0f | input[2] << 4);
					c[5] = (byte)(input[3] & 0xf0 | input[3] >> 4);
				}
				int ti = input[3] >> 1 & 6 | input[3] & 1;
				byte d = Etc2DistanceTable[ti];
				uint* color_set = stackalloc uint[]
				{
					ApplicateColorRaw(c, 0),
					ApplicateColor(c, 1, d),
					ApplicateColorRaw(c, 1),
					ApplicateColor(c, 1, -d)
				};

				for (int i = 0; i < 16; i++, j >>= 1, k >>= 1)
				{
					int index = k << 1 & 2 | j & 1;
					uint color = color_set[index];
					uint mask = PunchthroughMaskTable[index];
					int oi = WriteOrderTable[i];
					output[oi] = color & mask;
				}
			}
			else
			{
				int g = input[1] & 0xf8;
				int dg = (input[1] << 3 & 0x18) - (input[1] << 3 & 0x20);
				if (g + dg < 0 || g + dg > 255)
				{
					// H (Etc2Block + mask for color)
					unchecked
					{
						c[0] = (byte)(input[0] << 1 & 0xf0 | input[0] >> 3 & 0xf);
						c[1] = (byte)(input[0] << 5 & 0xe0 | input[1] & 0x10);
						c[1] |= (byte)(c[0 * 3 + 1] >> 4);
						c[2] = (byte)(input[1] & 8 | input[1] << 1 & 6 | input[2] >> 7);
						c[2] |= (byte)(c[0 * 3 + 2] << 4);
						c[3] = (byte)(input[2] << 1 & 0xf0 | input[2] >> 3 & 0xf);
						c[4] = (byte)(input[2] << 5 & 0xe0 | input[3] >> 3 & 0x10);
						c[4] |= (byte)(c[1 * 3 + 1] >> 4);
						c[5] = (byte)(input[3] << 1 & 0xf0 | input[3] >> 3 & 0xf);
					}
					int di = input[3] & 4 | input[3] << 1 & 2;
					if (c[0] > c[3] || (c[0] == c[3] && (c[1] > c[4] || (c[1] == c[4] && c[2] >= c[5]))))
					{
						++di;
					}
					byte d = Etc2DistanceTable[di];
					uint* color_set = stackalloc uint[]
					{
						ApplicateColor(c, 0, d),
						ApplicateColor(c, 0, -d),
						ApplicateColor(c, 1, d),
						ApplicateColor(c, 1, -d)
					};
					for (int i = 0; i < 16; i++, j >>= 1, k >>= 1)
					{
						int index = k << 1 & 2 | j & 1;
						uint color = color_set[index];
						uint mask = PunchthroughMaskTable[index];
						int oi = WriteOrderTable[i];
						output[oi] = color & mask;
					}
				}
				else
				{
					int b = input[2] & 0xf8;
					int db = (input[2] << 3 & 0x18) - (input[2] << 3 & 0x20);
					if (b + db < 0 || b + db > 255)
					{
						// planar (same as Etc2Block)
						unchecked
						{
							c[0] = (byte)(input[0] << 1 & 0xfc | input[0] >> 5 & 3);
							c[1] = (byte)(input[0] << 7 & 0x80 | input[1] & 0x7e | input[0] & 1);
							c[2] = (byte)(input[1] << 7 & 0x80 | input[2] << 2 & 0x60 | input[2] << 3 & 0x18 | input[3] >> 5 & 4);
							c[2] |= (byte)(c[0 * 3 + 2] >> 6);
							c[3] = (byte)(input[3] << 1 & 0xf8 | input[3] << 2 & 4 | input[3] >> 5 & 3);
							c[4] = (byte)(input[4] & 0xfe | input[4] >> 7);
							c[5] = (byte)(input[4] << 7 & 0x80 | input[5] >> 1 & 0x7c);
							c[5] |= (byte)(c[1 * 3 + 2] >> 6);
							c[6] = (byte)(input[5] << 5 & 0xe0 | input[6] >> 3 & 0x1c | input[5] >> 1 & 3);
							c[7] = (byte)(input[6] << 3 & 0xf8 | input[7] >> 5 & 0x6 | input[6] >> 4 & 1);
							c[8] = (byte)(input[7] << 2 | input[7] >> 4 & 3);
						}
						for (int y = 0, i = 0; y < 4; y++)
						{
							for (int x = 0; x < 4; x++, i++)
							{
								int ri = Clamp((x * (c[1 * 3 + 0] - c[0 * 3 + 0]) + y * (c[2 * 3 + 0] - c[0 * 3 + 0]) + 4 * c[0 * 3 + 0] + 2) >> 2);
								int gi = Clamp((x * (c[1 * 3 + 1] - c[0 * 3 + 1]) + y * (c[2 * 3 + 1] - c[0 * 3 + 1]) + 4 * c[0 * 3 + 1] + 2) >> 2);
								int bi = Clamp((x * (c[1 * 3 + 2] - c[0 * 3 + 2]) + y * (c[2 * 3 + 2] - c[0 * 3 + 2]) + 4 * c[0 * 3 + 2] + 2) >> 2);
								output[i] = Color(ri, gi, bi, 255);
							}
						}
					}
					else
					{
						// differential (Etc1Block + mask + specific mod table)
						int* code = stackalloc int[2];
						code[0] = (input[3] >> 5) * 4;
						code[1] = (input[3] >> 2 & 7) * 4;
						int ti = (input[3] & 1) * 16;
						unchecked
						{
							c[0] = (byte)(r | r >> 5);
							c[1] = (byte)(g | g >> 5);
							c[2] = (byte)(b | b >> 5);
							c[3] = (byte)(r + dr);
							c[4] = (byte)(g + dg);
							c[5] = (byte)(b + db);
							c[3] |= (byte)(c[1 * 3 + 0] >> 5);
							c[4] |= (byte)(c[1 * 3 + 1] >> 5);
							c[5] |= (byte)(c[1 * 3 + 2] >> 5);
						}
						for (int i = 0; i < 16; i++, j >>= 1, k >>= 1)
						{
							int s = Etc1SubblockTable[ti + i];
							int index = k << 1 & 2 | j & 1;
							int ci = code[s];
							int m = PunchthroughModifierTable[ci + index];
							uint color = ApplicateColor(c, s, m);
							uint mask = PunchthroughMaskTable[index];
							int oi = WriteOrderTable[i];
							output[oi] = color & mask;
						}
					}
				}
			}
		}

		private unsafe static void DecodeEtc2a8Block(byte* input, uint* output)
		{
			if ((input[1] & 0xf0) != 0)
			{
				int mult = input[1] >> 4;
				int ti = (input[1] & 0xf) * 8;
				ulong l =
					input[7] | (uint)input[6] << 8 |
					(uint)input[5] << 16 | (uint)input[4] << 24 |
					(ulong)input[3] << 32 | (ulong)input[2] << 40;
				for (int i = 0; i < 16; i++, l >>= 3)
				{
					int oi = WriteOrderTableRev[i];
					int ai = Etc2AlphaModTable[ti + (int)(l & 7)];
					byte c = (byte)Clamp(input[0] + mult * ai);
					((byte*)(output + oi))[3] = c;
				}
			}
			else
			{
				for (int i = 0; i < 16; i++)
				{
					int oi = WriteOrderTableRev[i];
					byte c = input[0];
					((byte*)(output + oi))[3] = c;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static uint Color(int r, int g, int b, int a)
		{
			return unchecked((uint)(r << 16 | g << 8 | b | a << 24));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int Clamp(int n)
		{
			return n < 0 ? 0 : n > 255 ? 255 : n;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe static uint ApplicateColor(byte* c, int o, int m)
		{
			return Color(Clamp(c[o * 3 + 0] + m), Clamp(c[o * 3 + 1] + m), Clamp(c[o * 3 + 2] + m), 255);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe static uint ApplicateColor(int* c, int o, int m)
		{
			return Color(Clamp(c[o * 3 + 0] + m), Clamp(c[o * 3 + 1] + m), Clamp(c[o * 3 + 2] + m), 255);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe static uint ApplicateColorRaw(byte* c, int o)
		{
			return Color(c[o * 3 + 0], c[o * 3 + 1], c[o * 3 + 2], 255);
		}

		private static readonly byte[] WriteOrderTable = { 0, 4, 8, 12, 1, 5, 9, 13, 2, 6, 10, 14, 3, 7, 11, 15 };
		private static readonly byte[] WriteOrderTableRev = { 15, 11, 7, 3, 14, 10, 6, 2, 13, 9, 5, 1, 12, 8, 4, 0 };
		private static readonly int[] Etc1SubblockTable =
		{
			0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1,
			0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1,
		};
		private static readonly int[] Etc1ModifierTable =
		{
			2, 8, -2, -8,
			5, 17, -5, -17,
			9, 29, -9, -29,
			13, 42, -13, -42,
			18, 60, -18, -60,
			24, 80, -24, -80,
			33, 106, -33, -106,
			47, 183, -47, -183,
		};
		private static readonly int[] PunchthroughModifierTable =
		{
			0, 8, 0, -8,
			0, 17, 0, -17,
			0, 29, 0, -29,
			0, 42, 0, -42,
			0, 60, 0, -60,
			0, 80, 0, -80,
			0, 106, 0, -106,
			0, 183, 0, -183,
		};
		private static readonly byte[] Etc2DistanceTable = { 3, 6, 11, 16, 23, 32, 41, 64 };
		private static readonly sbyte[] Etc2AlphaModTable =
		{
			-3, -6,  -9, -15, 2, 5, 8, 14,
			-3, -7, -10, -13, 2, 6, 9, 12,
			-2, -5,  -8, -13, 1, 4, 7, 12,
			-2, -4,  -6, -13, 1, 3, 5, 12,
			-3, -6,  -8, -12, 2, 5, 7, 11,
			-3, -7,  -9, -11, 2, 6, 8, 10,
			-4, -7,  -8, -11, 3, 6, 7, 10,
			-3, -5,  -8, -11, 2, 4, 7, 10,
			-2, -6,  -8, -10, 1, 5, 7,  9,
			-2, -5,  -8, -10, 1, 4, 7,  9,
			-2, -4,  -8, -10, 1, 3, 7,  9,
			-2, -5,  -7, -10, 1, 4, 6,  9,
			-3, -4,  -7, -10, 2, 3, 6,  9,
			-1, -2,  -3, -10, 0, 1, 2,  9,
			-4, -6,  -8,  -9, 3, 5, 7,  8,
			-3, -5,  -7,  -9, 2, 4, 6,  8,
		};
		private static readonly uint[] PunchthroughMaskTable = { 0xFFFFFFFF, 0xFFFFFFFF, 0x00000000, 0xFFFFFFFF };
	}
}
