using System.Runtime.CompilerServices;

namespace Astc
{
	public static class AstcDecoder
	{
		private unsafe struct BlockData
		{
			public int bw;
			public int bh;
			public int width;
			public int height;
			public int part_num;
			public int dual_plane;
			public int plane_selector;
			public int weight_range;
			public int weight_num; // max: 120
			public fixed int cem[4];
			public int cem_range;
			public int endpoint_value_num; // max: 32
			public fixed int endpoints[4 * 8];
			public fixed int weights[144 * 2];
			public fixed int partition[144];
		}

		private struct IntSeqData
		{
			public int bits;
			public int nonbits;
		}

		public static byte[] DecodeASTC(byte[] input, int width, int height, int blockWidth, int blockHeight)
		{
			byte[] output = new byte[width * height * 4];
			DecodeASTC(input, width, height, blockWidth, blockHeight, output);
			return output;
		}

		public unsafe static void DecodeASTC(byte[] input, int width, int height, int blockWidth, int blockHeight, byte[] output)
		{
			fixed (byte* inputPtr = input)
			{
				fixed (byte* outputPtr = output)
				{
					DecodeASTC(inputPtr, width, height, blockWidth, blockHeight, outputPtr);
				}
			}
		}

		public unsafe static void DecodeASTC(byte* input, int width, int height, int blockWidth, int blockHeight, byte* output)
		{
			int bcw = (width + blockWidth - 1) / blockWidth;
			int bch = (height + blockHeight - 1) / blockHeight;
			int clen_last = (width + blockWidth - 1) % blockWidth + 1;
			uint* buf = stackalloc uint[blockWidth * blockHeight];
			for (int t = 0; t < bch; t++)
			{
				for (int s = 0; s < bcw; s++, input += 16)
				{
					DecodeBlock(input, blockWidth, blockHeight, buf);
					int clen = s < bcw - 1 ? blockWidth : clen_last;
					uint* outputPtr = (uint*)(output + (t * blockHeight * 4 * width + s * 4 * blockWidth));
					uint* bufPtr = buf;
					for (int i = 0, y = t * blockHeight; i < blockHeight && y < height; i++, y++)
					{
						for (int j = 0; j < clen; j++) outputPtr[j] = bufPtr[j];
						outputPtr += width;
						bufPtr += blockWidth;
					}
				}
			}
		}

		private unsafe static void DecodeBlock(byte* input, int blockWidth, int blockHeight, uint* output)
		{
			if (input[0] == 0xfc && (input[1] & 1) == 1)
			{
				uint c = Color(input[9], input[11], input[13], input[15]);
				for (int i = 0; i < blockWidth * blockHeight; i++)
				{
					output[i] = c;
				}
			}
			else
			{
				BlockData blockData = new BlockData();
				blockData.bw = blockWidth;
				blockData.bh = blockHeight;
				BlockData* blockPtr = &blockData;
				DecodeBlockParameters(input, blockPtr);
				DecodeEndpoints(input, blockPtr);
				DecodeWeights(input, blockPtr);
				if (blockData.part_num > 1)
				{
					SelectPartition(input, blockPtr);
				}
				ApplicateColor(blockPtr, output);
			}
		}

		private unsafe static void DecodeBlockParameters(byte* input, BlockData* pBlock)
		{
			pBlock->dual_plane = (input[1] & 4) >> 2;
			pBlock->weight_range = (input[0] >> 4 & 1) | (input[1] << 2 & 8);

			if ((input[0] & 3) != 0)
			{
				pBlock->weight_range |= input[0] << 1 & 6;
				switch (input[0] & 0xc)
				{
					case 0:
						pBlock->width = (*((int*)input) >> 7 & 3) + 4;
						pBlock->height = (input[0] >> 5 & 3) + 2;
						break;
					case 4:
						pBlock->width = (*((int*)input) >> 7 & 3) + 8;
						pBlock->height = (input[0] >> 5 & 3) + 2;
						break;
					case 8:
						pBlock->width = (input[0] >> 5 & 3) + 2;
						pBlock->height = (*((int*)input) >> 7 & 3) + 8;
						break;
					case 12:
						if ((input[1] & 1) != 0)
						{
							pBlock->width = (input[0] >> 7 & 1) + 2;
							pBlock->height = (input[0] >> 5 & 3) + 2;
						}
						else
						{
							pBlock->width = (input[0] >> 5 & 3) + 2;
							pBlock->height = (input[0] >> 7 & 1) + 6;
						}
						break;
				}
			}
			else
			{
				pBlock->weight_range |= input[0] >> 1 & 6;
				switch ((*((int*)input)) & 0x180)
				{
					case 0:
						pBlock->width = 12;
						pBlock->height = (input[0] >> 5 & 3) + 2;
						break;
					case 0x80:
						pBlock->width = (input[0] >> 5 & 3) + 2;
						pBlock->height = 12;
						break;
					case 0x100:
						pBlock->width = (input[0] >> 5 & 3) + 6;
						pBlock->height = (input[1] >> 1 & 3) + 6;
						pBlock->dual_plane = 0;
						pBlock->weight_range &= 7;
						break;
					case 0x180:
						pBlock->width = (input[0] & 0x20) != 0 ? 10 : 6;
						pBlock->height = (input[0] & 0x20) != 0 ? 6 : 10;
						break;
				}
			}

			pBlock->part_num = (input[1] >> 3 & 3) + 1;

			pBlock->weight_num = pBlock->width * pBlock->height;
			if (pBlock->dual_plane != 0)
				pBlock->weight_num *= 2;

			int weight_bits, config_bits, cem_base = 0;

			switch (WeightPrecTableA[pBlock->weight_range])
			{
				case 3:
					weight_bits = pBlock->weight_num * WeightPrecTableB[pBlock->weight_range] + (pBlock->weight_num * 8 + 4) / 5;
					break;
				case 5:
					weight_bits = pBlock->weight_num * WeightPrecTableB[pBlock->weight_range] + (pBlock->weight_num * 7 + 2) / 3;
					break;
				default:
					weight_bits = pBlock->weight_num * WeightPrecTableB[pBlock->weight_range];
					break;
			}

			if (pBlock->part_num == 1)
			{
				pBlock->cem[0] = *((int*)(input + 1)) >> 5 & 0xf;
				config_bits = 17;
			}
			else
			{
				cem_base = *((int*)(input + 2)) >> 7 & 3;
				if (cem_base == 0)
				{
					int cem = input[3] >> 1 & 0xf;
					for (int i = 0; i < pBlock->part_num; i++)
					{
						pBlock->cem[i] = cem;
					}
					config_bits = 29;
				}
				else
				{
					for (int i = 0; i < pBlock->part_num; i++)
					{
						pBlock->cem[i] = ((input[3] >> (i + 1) & 1) + cem_base - 1) << 2;
					}
					switch (pBlock->part_num)
					{
						case 2:
							pBlock->cem[0] |= input[3] >> 3 & 3;
							pBlock->cem[1] |= GetBits(input, 126 - weight_bits, 2);
							break;
						case 3:
							pBlock->cem[0] |= input[3] >> 4 & 1;
							pBlock->cem[0] |= GetBits(input, 122 - weight_bits, 2) & 2;
							pBlock->cem[1] |= GetBits(input, 124 - weight_bits, 2);
							pBlock->cem[2] |= GetBits(input, 126 - weight_bits, 2);
							break;
						case 4:
							for (int i = 0; i < 4; i++)
							{
								pBlock->cem[i] |= GetBits(input, 120 + i * 2 - weight_bits, 2);
							}
							break;
					}
					config_bits = 25 + pBlock->part_num * 3;
				}
			}

			if (pBlock->dual_plane != 0)
			{
				config_bits += 2;
				pBlock->plane_selector = GetBits(input, cem_base != 0 ? 130 - weight_bits - pBlock->part_num * 3 : 126 - weight_bits, 2);
			}

			int remain_bits = 128 - config_bits - weight_bits;

			pBlock->endpoint_value_num = 0;
			for (int i = 0; i < pBlock->part_num; i++)
			{
				pBlock->endpoint_value_num += (pBlock->cem[i] >> 1 & 6) + 2;
			}

			for (int i = 0, endpoint_bits; i < CemTableA.Length; i++)
			{
				switch (CemTableA[i])
				{
					case 3:
						endpoint_bits = pBlock->endpoint_value_num * CemTableB[i] + (pBlock->endpoint_value_num * 8 + 4) / 5;
						break;
					case 5:
						endpoint_bits = pBlock->endpoint_value_num * CemTableB[i] + (pBlock->endpoint_value_num * 7 + 2) / 3;
						break;
					default:
						endpoint_bits = pBlock->endpoint_value_num * CemTableB[i];
						break;
				}

				if (endpoint_bits <= remain_bits)
				{
					pBlock->cem_range = i;
					break;
				}
			}
		}

		private unsafe static void DecodeEndpoints(byte* input, BlockData* pBlock)
		{
			IntSeqData* epSeq = stackalloc IntSeqData[32];
			DecodeIntseq(input, pBlock->part_num == 1 ? 17 : 29, CemTableA[pBlock->cem_range], CemTableB[pBlock->cem_range], pBlock->endpoint_value_num, false, epSeq);

			int* ev = stackalloc int[32];
			switch (CemTableA[pBlock->cem_range])
			{
				case 3:
					for (int i = 0, b = 0, c = DETritsTable[CemTableB[pBlock->cem_range]]; i < pBlock->endpoint_value_num; i++)
					{
						int a = (epSeq[i].bits & 1) * 0x1ff;
						int x = epSeq[i].bits >> 1;
						switch (CemTableB[pBlock->cem_range])
						{
							case 1:
								b = 0;
								break;
							case 2:
								b = 0b100010110 * x;
								break;
							case 3:
								b = x << 7 | x << 2 | x;
								break;
							case 4:
								b = x << 6 | x;
								break;
							case 5:
								b = x << 5 | x >> 2;
								break;
							case 6:
								b = x << 4 | x >> 4;
								break;
						}
						ev[i] = (a & 0x80) | ((epSeq[i].nonbits * c + b) ^ a) >> 2;
					}
					break;

				case 5:
					for (int i = 0, b = 0, c = DEQuintsTable[CemTableB[pBlock->cem_range]]; i < pBlock->endpoint_value_num; i++)
					{
						int a = (epSeq[i].bits & 1) * 0x1ff;
						int x = epSeq[i].bits >> 1;
						switch (CemTableB[pBlock->cem_range])
						{
							case 1:
								b = 0;
								break;
							case 2:
								b = 0b100001100 * x;
								break;
							case 3:
								b = x << 7 | x << 1 | x >> 1;
								break;
							case 4:
								b = x << 6 | x >> 1;
								break;
							case 5:
								b = x << 5 | x >> 3;
								break;
						}
						ev[i] = (a & 0x80) | ((epSeq[i].nonbits * c + b) ^ a) >> 2;
					}
					break;

				default:
					switch (CemTableB[pBlock->cem_range])
					{
						case 1:
							for (int i = 0; i < pBlock->endpoint_value_num; i++)
								ev[i] = epSeq[i].bits * 0xff;
							break;
						case 2:
							for (int i = 0; i < pBlock->endpoint_value_num; i++)
								ev[i] = epSeq[i].bits * 0x55;
							break;
						case 3:
							for (int i = 0; i < pBlock->endpoint_value_num; i++)
								ev[i] = epSeq[i].bits << 5 | epSeq[i].bits << 2 | epSeq[i].bits >> 1;
							break;
						case 4:
							for (int i = 0; i < pBlock->endpoint_value_num; i++)
								ev[i] = epSeq[i].bits << 4 | epSeq[i].bits;
							break;
						case 5:
							for (int i = 0; i < pBlock->endpoint_value_num; i++)
								ev[i] = epSeq[i].bits << 3 | epSeq[i].bits >> 2;
							break;
						case 6:
							for (int i = 0; i < pBlock->endpoint_value_num; i++)
								ev[i] = epSeq[i].bits << 2 | epSeq[i].bits >> 4;
							break;
						case 7:
							for (int i = 0; i < pBlock->endpoint_value_num; i++)
								ev[i] = epSeq[i].bits << 1 | epSeq[i].bits >> 6;
							break;
						case 8:
							for (int i = 0; i < pBlock->endpoint_value_num; i++)
								ev[i] = epSeq[i].bits;
							break;
					}
					break;
			}

			int* v = ev;
			for (int cem = 0, cemOff = 0; cem < pBlock->part_num; v += (pBlock->cem[cem] / 4 + 1) * 2, cem++, cemOff += 8)
			{				
				switch (pBlock->cem[cem])
				{
					case 0:
						SetEndpoint(&pBlock->endpoints[cemOff], v[0], v[0], v[0], 255, v[1], v[1], v[1], 255);
						break;
					case 1:
						{
							int l0 = (v[0] >> 2) | (v[1] & 0xc0);
							int l1 = Clamp(l0 + (v[1] & 0x3f));
							SetEndpoint(&pBlock->endpoints[cemOff], l0, l0, l0, 255, l1, l1, l1, 255);
						}
						break;
					case 4:
						SetEndpoint(&pBlock->endpoints[cemOff], v[0], v[0], v[0], v[2], v[1], v[1], v[1], v[3]);
						break;
					case 5:
						BitTransferSigned(&v[1], &v[0]);
						BitTransferSigned(&v[3], &v[2]);
						v[1] += v[0];
						SetEndpointClamp(&pBlock->endpoints[cemOff], v[0], v[0], v[0], v[2], v[1], v[1], v[1], v[2] + v[3]);
						break;
					case 6:
						SetEndpoint(&pBlock->endpoints[cemOff], v[0] * v[3] >> 8, v[1] * v[3] >> 8, v[2] * v[3] >> 8, 255, v[0], v[1], v[2], 255);
						break;
					case 8:
						if (v[0] + v[2] + v[4] <= v[1] + v[3] + v[5])
							SetEndpoint(&pBlock->endpoints[cemOff], v[0], v[2], v[4], 255, v[1], v[3], v[5], 255);
						else
							SetEndpointBlue(&pBlock->endpoints[cemOff], v[1], v[3], v[5], 255, v[0], v[2], v[4], 255);
						break;
					case 9:
						BitTransferSigned(&v[1], &v[0]);
						BitTransferSigned(&v[3], &v[2]);
						BitTransferSigned(&v[5], &v[4]);
						if (v[1] + v[3] + v[5] >= 0)
							SetEndpointClamp(&pBlock->endpoints[cemOff], v[0], v[2], v[4], 255, v[0] + v[1], v[2] + v[3], v[4] + v[5], 255);
						else
							SetEndpointBlueClamp(&pBlock->endpoints[cemOff], v[0] + v[1], v[2] + v[3], v[4] + v[5], 255, v[0], v[2], v[4], 255);
						break;
					case 10:
						SetEndpoint(&pBlock->endpoints[cemOff], v[0] * v[3] >> 8, v[1] * v[3] >> 8, v[2] * v[3] >> 8, v[4], v[0], v[1], v[2], v[5]);
						break;
					case 12:
						if (v[0] + v[2] + v[4] <= v[1] + v[3] + v[5])
							SetEndpoint(&pBlock->endpoints[cemOff], v[0], v[2], v[4], v[6], v[1], v[3], v[5], v[7]);
						else
							SetEndpointBlue(&pBlock->endpoints[cemOff], v[1], v[3], v[5], v[7], v[0], v[2], v[4], v[6]);
						break;
					case 13:
						BitTransferSigned(&v[1], &v[0]);
						BitTransferSigned(&v[3], &v[2]);
						BitTransferSigned(&v[5], &v[4]);
						BitTransferSigned(&v[7], &v[6]);
						if (v[1] + v[3] + v[5] >= 0)
							SetEndpointClamp(&pBlock->endpoints[cemOff], v[0], v[2], v[4], v[6], v[0] + v[1], v[2] + v[3], v[4] + v[5], v[6] + v[7]);
						else
							SetEndpointBlueClamp(&pBlock->endpoints[cemOff], v[0] + v[1], v[2] + v[3], v[4] + v[5], v[6] + v[7], v[0], v[2], v[4], v[6]);
						break;
				}
			}
		}

		private unsafe static void DecodeWeights(byte* input, BlockData* block)
		{
			IntSeqData* wSeq = stackalloc IntSeqData[128];
			DecodeIntseq(input, 128, WeightPrecTableA[block->weight_range], WeightPrecTableB[block->weight_range], block->weight_num, true, wSeq);
			
			int* wv = stackalloc int[128];
			if (WeightPrecTableA[block->weight_range] == 0)
			{
				switch (WeightPrecTableB[block->weight_range])
				{
					case 1:
						for (int i = 0; i < block->weight_num; i++)
							wv[i] = wSeq[i].bits != 0 ? 63 : 0;
						break;
					case 2:
						for (int i = 0; i < block->weight_num; i++)
							wv[i] = wSeq[i].bits << 4 | wSeq[i].bits << 2 | wSeq[i].bits;
						break;
					case 3:
						for (int i = 0; i < block->weight_num; i++)
							wv[i] = wSeq[i].bits << 3 | wSeq[i].bits;
						break;
					case 4:
						for (int i = 0; i < block->weight_num; i++)
							wv[i] = wSeq[i].bits << 2 | wSeq[i].bits >> 2;
						break;
					case 5:
						for (int i = 0; i < block->weight_num; i++)
							wv[i] = wSeq[i].bits << 1 | wSeq[i].bits >> 4;
						break;
				}
				for (int i = 0; i < block->weight_num; i++)
				{
					if (wv[i] > 32)
					{
						++wv[i];
					}
				}
			}
			else if (WeightPrecTableB[block->weight_range] == 0)
			{
				int s = WeightPrecTableA[block->weight_range] == 3 ? 32 : 16;
				for (int i = 0; i < block->weight_num; i++)
				{
					wv[i] = wSeq[i].nonbits * s;
				}
			}
			else
			{
				if (WeightPrecTableA[block->weight_range] == 3)
				{
					switch (WeightPrecTableB[block->weight_range])
					{
						case 1:
							for (int i = 0; i < block->weight_num; i++)
							{
								wv[i] = wSeq[i].nonbits * 50;
							}
							break;
						case 2:
							for (int i = 0; i < block->weight_num; i++)
							{
								wv[i] = wSeq[i].nonbits * 23;
								if ((wSeq[i].bits & 2) != 0)
								{
									wv[i] += 0b1000101;
								}
							}
							break;
						case 3:
							for (int i = 0; i < block->weight_num; i++)
							{
								wv[i] = wSeq[i].nonbits * 11 + ((wSeq[i].bits << 4 | wSeq[i].bits >> 1) & 0b1100011);
							}
							break;
					}
				}
				else if (WeightPrecTableA[block->weight_range] == 5)
				{
					switch (WeightPrecTableB[block->weight_range])
					{
						case 1:
							for (int i = 0; i < block->weight_num; i++)
								wv[i] = wSeq[i].nonbits * 28;
							break;
						case 2:
							for (int i = 0; i < block->weight_num; i++)
							{
								wv[i] = wSeq[i].nonbits * 13;
								if ((wSeq[i].bits & 2) != 0)
								{
									wv[i] += 0b1000010;
								}
							}
							break;
					}
				}
				for (int i = 0; i < block->weight_num; i++)
				{
					int a = (wSeq[i].bits & 1) * 0x7f;
					wv[i] = (a & 0x20) | ((wv[i] ^ a) >> 2);
					if (wv[i] > 32)
					{
						++wv[i];
					}
				}
			}

			int ds = (1024 + block->bw / 2) / (block->bw - 1);
			int dt = (1024 + block->bh / 2) / (block->bh - 1);
			int pn = block->dual_plane != 0 ? 2 : 1;

			for (int t = 0, i = 0; t < block->bh; t++)
			{
				for (int s = 0; s < block->bw; s++, i++)
				{
					int gs = (ds * s * (block->width - 1) + 32) >> 6;
					int gt = (dt * t * (block->height - 1) + 32) >> 6;
					int fs = gs & 0xf;
					int ft = gt & 0xf;
					int v = (gs >> 4) + (gt >> 4) * block->width;
					int w11 = (fs * ft + 8) >> 4;
					int w10 = ft - w11;
					int w01 = fs - w11;
					int w00 = 16 - fs - ft + w11;

					for (int p = 0; p < pn; p++)
					{
						int p00 = wv[v * pn + p];
						int p01 = wv[(v + 1) * pn + p];
						int p10 = wv[(v + block->width) * pn + p];
						int p11 = wv[(v + block->width + 1) * pn + p];
						block->weights[i * 2 + p] = (p00 * w00 + p01 * w01 + p10 * w10 + p11 * w11 + 8) >> 4;
					}
				}
			}
		}

		private unsafe static void SelectPartition(byte* input, BlockData* block)
		{
			bool small_block = block->bw * block->bh < 31;
			int seed = (*((int*)input) >> 13 & 0x3ff) | (block->part_num - 1) << 10;

			uint rnum;
			unchecked
			{
				rnum = (uint)seed;
				rnum ^= rnum >> 15;
				rnum -= rnum << 17;
				rnum += rnum << 7;
				rnum += rnum << 4;
				rnum ^= rnum >> 5;
				rnum += rnum << 16;
				rnum ^= rnum >> 7;
				rnum ^= rnum >> 3;
				rnum ^= rnum << 6;
				rnum ^= rnum >> 17;
			}
			
			int* seeds = stackalloc int[8];
			for (int i = 0; i < 8; i++)
			{
				seeds[i] = (int)((rnum >> (i * 4)) & 0xF);
				seeds[i] *= seeds[i];
			}


			int* sh = stackalloc int[2];
			sh[0] = (seed & 2) != 0 ? 4 : 5;
			sh[1] = block->part_num == 3 ? 6 : 5;

			if ((seed & 1) != 0)
			{
				for (int i = 0; i < 8; i++)
				{
					seeds[i] >>= sh[i % 2];
				}
			}
			else
			{
				for (int i = 0; i < 8; i++)
				{
					seeds[i] >>= sh[1 - i % 2];
				}
			}

			if (small_block)
			{
				for (int t = 0, i = 0; t < block->bh; t++)
				{
					for (int s = 0; s < block->bw; s++, i++)
					{
						int x = s << 1;
						int y = t << 1;
						int a = (int)((seeds[0] * x + seeds[1] * y + (rnum >> 14)) & 0x3f);
						int b = (int)((seeds[2] * x + seeds[3] * y + (rnum >> 10)) & 0x3f);
						int c = (int)(block->part_num < 3 ? 0 : (seeds[4] * x + seeds[5] * y + (rnum >> 6)) & 0x3f);
						int d = (int)(block->part_num < 4 ? 0 : (seeds[6] * x + seeds[7] * y + (rnum >> 2)) & 0x3f);
						block->partition[i] = (a >= b && a >= c && a >= d) ? 0 : (b >= c && b >= d) ? 1 : (c >= d) ? 2 : 3;
					}
				}
			}
			else
			{
				for (int y = 0, i = 0; y < block->bh; y++)
				{
					for (int x = 0; x < block->bw; x++, i++)
					{
						int a = (int)((seeds[0] * x + seeds[1] * y + (rnum >> 14)) & 0x3f);
						int b = (int)((seeds[2] * x + seeds[3] * y + (rnum >> 10)) & 0x3f);
						int c = (int)(block->part_num < 3 ? 0 : (seeds[4] * x + seeds[5] * y + (rnum >> 6)) & 0x3f);
						int d = (int)(block->part_num < 4 ? 0 : (seeds[6] * x + seeds[7] * y + (rnum >> 2)) & 0x3f);
						block->partition[i] = (a >= b && a >= c && a >= d) ? 0 : (b >= c && b >= d) ? 1 : (c >= d) ? 2 : 3;
					}
				}
			}
		}

		private unsafe static void ApplicateColor(BlockData* block, uint* output)
		{
			if (block->dual_plane != 0)
			{
				int* ps = stackalloc int[] { 0, 0, 0, 0 };
				ps[block->plane_selector] = 1;
				if (block->part_num > 1)
				{
					for (int i = 0; i < block->bw * block->bh; i++)
					{
						int p = block->partition[i];
						byte r = SelectColor(block->endpoints[p * 8 + 0], block->endpoints[p * 8 + 4], block->weights[i * 2 + ps[0]]);
						byte g = SelectColor(block->endpoints[p * 8 + 1], block->endpoints[p * 8 + 5], block->weights[i * 2 + ps[1]]);
						byte b = SelectColor(block->endpoints[p * 8 + 2], block->endpoints[p * 8 + 6], block->weights[i * 2 + ps[2]]);
						byte a = SelectColor(block->endpoints[p * 8 + 3], block->endpoints[p * 8 + 7], block->weights[i * 2 + ps[3]]);
						output[i] = Color(r, g, b, a);
					}
				}
				else
				{
					for (int i = 0; i < block->bw * block->bh; i++)
					{
						byte r = SelectColor(block->endpoints[0], block->endpoints[4], block->weights[i * 2 + ps[0]]);
						byte g = SelectColor(block->endpoints[1], block->endpoints[5], block->weights[i * 2 + ps[1]]);
						byte b = SelectColor(block->endpoints[2], block->endpoints[6], block->weights[i * 2 + ps[2]]);
						byte a = SelectColor(block->endpoints[3], block->endpoints[7], block->weights[i * 2 + ps[3]]);
						output[i] = Color(r, g, b, a);
					}
				}
			}
			else if (block->part_num > 1)
			{
				for (int i = 0; i < block->bw * block->bh; i++)
				{
					int p = block->partition[i];
					byte r = SelectColor(block->endpoints[p * 8 + 0], block->endpoints[p * 8 + 4], block->weights[i * 2]);
					byte g = SelectColor(block->endpoints[p * 8 + 1], block->endpoints[p * 8 + 5], block->weights[i * 2]);
					byte b = SelectColor(block->endpoints[p * 8 + 2], block->endpoints[p * 8 + 6], block->weights[i * 2]);
					byte a = SelectColor(block->endpoints[p * 8 + 3], block->endpoints[p * 8 + 7], block->weights[i * 2]);
					output[i] = Color(r, g, b, a);
				}
			}
			else
			{
				for (int i = 0; i < block->bw * block->bh; i++)
				{
					byte r = SelectColor(block->endpoints[0], block->endpoints[4], block->weights[i * 2]);
					byte g = SelectColor(block->endpoints[1], block->endpoints[5], block->weights[i * 2]);
					byte b = SelectColor(block->endpoints[2], block->endpoints[6], block->weights[i * 2]);
					byte a = SelectColor(block->endpoints[3], block->endpoints[7], block->weights[i * 2]);
					output[i] = Color(r, g, b, a);
				}
			}
		}

		private unsafe static void DecodeIntseq(byte* input, int offset, int a, int b, int count, bool reverse, IntSeqData* _out)
		{
			if (count <= 0)
				return;

			int n = 0;

			if (a == 3)
			{
				int mask = (1 << b) - 1;
				int block_count = (count + 4) / 5;
				int last_block_count = (count + 4) % 5 + 1;
				int block_size = 8 + 5 * b;
				int last_block_size = (block_size * last_block_count + 4) / 5;

				if (reverse)
				{
					for (int i = 0, p = offset; i < block_count; i++, p -= block_size)
					{
						int now_size = (i < block_count - 1) ? block_size : last_block_size;
						ulong d = BitReverseU64(GetBits64(input, p - now_size, now_size), now_size);
						int x = (int)((d >> b & 3) | (d >> b * 2 & 0xc) | (d >> b * 3 & 0x10) | (d >> b * 4 & 0x60) | (d >> b * 5 & 0x80));
						for (int j = 0; j < 5 && n < count; j++, n++)
						{
							_out[n] = new IntSeqData()
							{
								bits = (int)(d >> (DImt[j] + b * j)) & mask,
								nonbits = DITritsTable[j * 256 + x],
							};
						}
					}
				}
				else
				{
					for (int i = 0, p = offset; i < block_count; i++, p += block_size)
					{
						ulong d = GetBits64(input, p, (i < block_count - 1) ? block_size : last_block_size);
						int x = (int)((d >> b & 3) | (d >> b * 2 & 0xc) | (d >> b * 3 & 0x10) | (d >> b * 4 & 0x60) | (d >> b * 5 & 0x80));
						for (int j = 0; j < 5 && n < count; j++, n++)
						{
							_out[n] = new IntSeqData()
							{
								bits = unchecked((int)(d >> (DImt[j] + b * j))) & mask,
								nonbits = DITritsTable[j * 256 + x],
							};
						}
					}
				}
			}
			else if (a == 5)
			{
				int mask = (1 << b) - 1;
				int block_count = (count + 2) / 3;
				int last_block_count = (count + 2) % 3 + 1;
				int block_size = 7 + 3 * b;
				int last_block_size = (block_size * last_block_count + 2) / 3;

				if (reverse)
				{
					for (int i = 0, p = offset; i < block_count; i++, p -= block_size)
					{
						int now_size = (i < block_count - 1) ? block_size : last_block_size;
						ulong d = BitReverseU64(GetBits64(input, p - now_size, now_size), now_size);
						int x = (int)((d >> b & 7) | (d >> b * 2 & 0x18) | (d >> b * 3 & 0x60));
						for (int j = 0; j < 3 && n < count; j++, n++)
						{
							_out[n] = new IntSeqData()
							{
								bits = (int)d >> (DImq[j] + b * j) & mask,
								nonbits = DIQuintsTable[j * 128 + x],
							};
						}
					}
				}
				else
				{
					for (int i = 0, p = offset; i < block_count; i++, p += block_size)
					{
						ulong d = GetBits64(input, p, (i < block_count - 1) ? block_size : last_block_size);
						int x = (int)((d >> b & 7) | (d >> b * 2 & 0x18) | (d >> b * 3 & 0x60));
						for (int j = 0; j < 3 && n < count; j++, n++)
						{
							_out[n] = new IntSeqData()
							{
								bits = (int)d >> (DImq[j] + b * j) & mask,
								nonbits = DIQuintsTable[j * 128 + x],
							};
						}
					}
				}
			}
			else
			{
				if (reverse)
				{
					for (int p = offset - b; n < count; n++, p -= b)
					{
						_out[n] = new IntSeqData()
						{
							bits = BitReverseU8((byte)GetBits(input, p, b), b),
							nonbits = 0,
						};
					}
				}
				else
				{
					for (int p = offset; n < count; n++, p += b)
					{
						_out[n] = new IntSeqData()
						{
							bits = GetBits(input, p, b),
							nonbits = 0,
						};
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static uint Color(uint r, uint g, uint b, uint a)
		{
			return r << 16 | g << 8 | b | a << 24;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static byte BitReverseU8(byte c, int bits)
		{
			return (byte)(BitReverseTable[c] >> (8 - bits));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong BitReverseU64(ulong d, int bits)
		{
			ulong ret;
			unchecked
			{
				ret =
				   (ulong)BitReverseTable[(int)(d >> 0 & 0xff)] << 56 |
				   (ulong)BitReverseTable[(int)(d >> 8 & 0xff)] << 48 |
				   (ulong)BitReverseTable[(int)(d >> 16 & 0xff)] << 40 |
				   (ulong)BitReverseTable[(int)(d >> 24 & 0xff)] << 32 |
				   (ulong)BitReverseTable[(int)(d >> 32 & 0xff)] << 24 |
				   (ulong)BitReverseTable[(int)(d >> 40 & 0xff)] << 16 |
				   (ulong)BitReverseTable[(int)(d >> 48 & 0xff)] << 8 |
				   (ulong)BitReverseTable[(int)(d >> 56 & 0xff)];
			}
			return ret >> (64 - bits);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe static int GetBits(byte* input, int bit, int len)
		{
			return (*((int*)(input + bit / 8)) >> (bit % 8)) & ((1 << len) - 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe static ulong GetBits64(byte* input, int bit, int len)
		{
			ulong mask = len == 64 ? 0xffffffffffffffff : (1UL << len) - 1;
			if (len < 1)
				return 0;
			else if (bit >= 64)
				return *((ulong*)(input + 8)) >> (bit - 64) & mask;
			else if (bit <= 0)
				return *((ulong*)input) << -bit & mask;
			else if (bit + len <= 64)
				return *((ulong*)input) >> bit & mask;
			else
				return (*((ulong*)input) >> bit | *((ulong*)(input + 8)) << (64 - bit)) & mask;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static byte Clamp(int n)
		{
			return n < 0 ? byte.MinValue : n > 255 ? byte.MaxValue : (byte)n;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static unsafe void BitTransferSigned(int* a, int* b)
		{
			*b = (*b >> 1) | (*a & 0x80);
			*a = (*a >> 1) & 0x3f;
			if ((*a & 0x20) != 0)
				*a -= 0x40;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe static void SetEndpoint(int* endpoint, int r1, int g1, int b1, int a1, int r2, int g2, int b2, int a2)
		{
			endpoint[0] = r1;
			endpoint[1] = g1;
			endpoint[2] = b1;
			endpoint[3] = a1;
			endpoint[4] = r2;
			endpoint[5] = g2;
			endpoint[6] = b2;
			endpoint[7] = a2;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe static void SetEndpointClamp(int* endpoint, int r1, int g1, int b1, int a1, int r2, int g2, int b2, int a2)
		{
			endpoint[0] = Clamp(r1);
			endpoint[1] = Clamp(g1);
			endpoint[2] = Clamp(b1);
			endpoint[3] = Clamp(a1);
			endpoint[4] = Clamp(r2);
			endpoint[5] = Clamp(g2);
			endpoint[6] = Clamp(b2);
			endpoint[7] = Clamp(a2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe static void SetEndpointBlue(int* endpoint, int r1, int g1, int b1, int a1, int r2, int g2, int b2, int a2)
		{
			endpoint[0] = (r1 + b1) >> 1;
			endpoint[1] = (g1 + b1) >> 1;
			endpoint[2] = b1;
			endpoint[3] = a1;
			endpoint[4] = (r2 + b2) >> 1;
			endpoint[5] = (g2 + b2) >> 1;
			endpoint[6] = b2;
			endpoint[7] = a2;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe static void SetEndpointBlueClamp(int* endpoint, int r1, int g1, int b1, int a1, int r2, int g2, int b2, int a2)
		{
			endpoint[0] = Clamp((r1 + b1) >> 1);
			endpoint[1] = Clamp((g1 + b1) >> 1);
			endpoint[2] = Clamp(b1);
			endpoint[3] = Clamp(a1);
			endpoint[4] = Clamp((r2 + b2) >> 1);
			endpoint[5] = Clamp((g2 + b2) >> 1);
			endpoint[6] = Clamp(b2);
			endpoint[7] = Clamp(a2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static byte SelectColor(int v0, int v1, int weight)
		{
			return (byte)(((((v0 << 8 | v0) * (64 - weight) + (v1 << 8 | v1) * weight + 32) >> 6) * 255 + 32768) / 65536);
		}

		private static readonly byte[] BitReverseTable = new byte[]
		{
			0x00, 0x80, 0x40, 0xC0, 0x20, 0xA0, 0x60, 0xE0, 0x10, 0x90, 0x50, 0xD0, 0x30, 0xB0, 0x70, 0xF0,
			0x08, 0x88, 0x48, 0xC8, 0x28, 0xA8, 0x68, 0xE8, 0x18, 0x98, 0x58, 0xD8, 0x38, 0xB8, 0x78, 0xF8,
			0x04, 0x84, 0x44, 0xC4, 0x24, 0xA4, 0x64, 0xE4, 0x14, 0x94, 0x54, 0xD4, 0x34, 0xB4, 0x74, 0xF4,
			0x0C, 0x8C, 0x4C, 0xCC, 0x2C, 0xAC, 0x6C, 0xEC, 0x1C, 0x9C, 0x5C, 0xDC, 0x3C, 0xBC, 0x7C, 0xFC,
			0x02, 0x82, 0x42, 0xC2, 0x22, 0xA2, 0x62, 0xE2, 0x12, 0x92, 0x52, 0xD2, 0x32, 0xB2, 0x72, 0xF2,
			0x0A, 0x8A, 0x4A, 0xCA, 0x2A, 0xAA, 0x6A, 0xEA, 0x1A, 0x9A, 0x5A, 0xDA, 0x3A, 0xBA, 0x7A, 0xFA,
			0x06, 0x86, 0x46, 0xC6, 0x26, 0xA6, 0x66, 0xE6, 0x16, 0x96, 0x56, 0xD6, 0x36, 0xB6, 0x76, 0xF6,
			0x0E, 0x8E, 0x4E, 0xCE, 0x2E, 0xAE, 0x6E, 0xEE, 0x1E, 0x9E, 0x5E, 0xDE, 0x3E, 0xBE, 0x7E, 0xFE,
			0x01, 0x81, 0x41, 0xC1, 0x21, 0xA1, 0x61, 0xE1, 0x11, 0x91, 0x51, 0xD1, 0x31, 0xB1, 0x71, 0xF1,
			0x09, 0x89, 0x49, 0xC9, 0x29, 0xA9, 0x69, 0xE9, 0x19, 0x99, 0x59, 0xD9, 0x39, 0xB9, 0x79, 0xF9,
			0x05, 0x85, 0x45, 0xC5, 0x25, 0xA5, 0x65, 0xE5, 0x15, 0x95, 0x55, 0xD5, 0x35, 0xB5, 0x75, 0xF5,
			0x0D, 0x8D, 0x4D, 0xCD, 0x2D, 0xAD, 0x6D, 0xED, 0x1D, 0x9D, 0x5D, 0xDD, 0x3D, 0xBD, 0x7D, 0xFD,
			0x03, 0x83, 0x43, 0xC3, 0x23, 0xA3, 0x63, 0xE3, 0x13, 0x93, 0x53, 0xD3, 0x33, 0xB3, 0x73, 0xF3,
			0x0B, 0x8B, 0x4B, 0xCB, 0x2B, 0xAB, 0x6B, 0xEB, 0x1B, 0x9B, 0x5B, 0xDB, 0x3B, 0xBB, 0x7B, 0xFB,
			0x07, 0x87, 0x47, 0xC7, 0x27, 0xA7, 0x67, 0xE7, 0x17, 0x97, 0x57, 0xD7, 0x37, 0xB7, 0x77, 0xF7,
			0x0F, 0x8F, 0x4F, 0xCF, 0x2F, 0xAF, 0x6F, 0xEF, 0x1F, 0x9F, 0x5F, 0xDF, 0x3F, 0xBF, 0x7F, 0xFF
		};

		private static readonly int[] WeightPrecTableA = new int[] { 0, 0, 0, 3, 0, 5, 3, 0, 0, 0, 5, 3, 0, 5, 3, 0 };
		private static readonly int[] WeightPrecTableB = new int[] { 0, 0, 1, 0, 2, 0, 1, 3, 0, 0, 1, 2, 4, 2, 3, 5 };

		private static readonly int[] CemTableA = new int[] { 0, 3, 5, 0, 3, 5, 0, 3, 5, 0, 3, 5, 0, 3, 5, 0, 3, 0, 0 };
		private static readonly int[] CemTableB = new int[] { 8, 6, 5, 7, 5, 4, 6, 4, 3, 5, 3, 2, 4, 2, 1, 3, 1, 2, 1 };

		private static readonly int[] DImt = new int[] { 0, 2, 4, 5, 7 };
		private static readonly int[] DImq = new int[] { 0, 3, 5 };
		private static readonly int[] DITritsTable =
		{
			0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 1, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 1, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2,
			0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 1, 2, 2, 2, 0, 0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 1, 2, 2, 2, 0, 0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 1, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 1, 2, 2, 2, 1, 0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 1, 2, 2, 2, 1,
			0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 2, 2, 2, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 2, 2, 2, 2,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2
		};
		private static readonly int[] DIQuintsTable =
		{
			0, 1, 2, 3, 4, 0, 4, 4, 0, 1, 2, 3, 4, 1, 4, 4, 0, 1, 2, 3, 4, 2, 4, 4, 0, 1, 2, 3, 4, 3, 4, 4, 0, 1, 2, 3, 4, 0, 4, 0, 0, 1, 2, 3, 4, 1, 4, 1, 0, 1, 2, 3, 4, 2, 4, 2, 0, 1, 2, 3, 4, 3, 4, 3, 0, 1, 2, 3, 4, 0, 2, 3, 0, 1, 2, 3, 4, 1, 2, 3, 0, 1, 2, 3, 4, 2, 2, 3, 0, 1, 2, 3, 4, 3, 2, 3, 0, 1, 2, 3, 4, 0, 0, 1, 0, 1, 2, 3, 4, 1, 0, 1, 0, 1, 2, 3, 4, 2, 0, 1, 0, 1, 2, 3, 4, 3, 0, 1,
			0, 0, 0, 0, 0, 4, 4, 4, 1, 1, 1, 1, 1, 4, 4, 4, 2, 2, 2, 2, 2, 4, 4, 4, 3, 3, 3, 3, 3, 4, 4, 4, 0, 0, 0, 0, 0, 4, 0, 4, 1, 1, 1, 1, 1, 4, 1, 4, 2, 2, 2, 2, 2, 4, 2, 4, 3, 3, 3, 3, 3, 4, 3, 4, 0, 0, 0, 0, 0, 4, 0, 0, 1, 1, 1, 1, 1, 4, 1, 1, 2, 2, 2, 2, 2, 4, 2, 2, 3, 3, 3, 3, 3, 4, 3, 3, 0, 0, 0, 0, 0, 4, 0, 0, 1, 1, 1, 1, 1, 4, 1, 1, 2, 2, 2, 2, 2, 4, 2, 2, 3, 3, 3, 3, 3, 4, 3, 3,
			0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 1, 4, 0, 0, 0, 0, 0, 0, 2, 4, 0, 0, 0, 0, 0, 0, 3, 4, 1, 1, 1, 1, 1, 1, 4, 4, 1, 1, 1, 1, 1, 1, 4, 4, 1, 1, 1, 1, 1, 1, 4, 4, 1, 1, 1, 1, 1, 1, 4, 4, 2, 2, 2, 2, 2, 2, 4, 4, 2, 2, 2, 2, 2, 2, 4, 4, 2, 2, 2, 2, 2, 2, 4, 4, 2, 2, 2, 2, 2, 2, 4, 4, 3, 3, 3, 3, 3, 3, 4, 4, 3, 3, 3, 3, 3, 3, 4, 4, 3, 3, 3, 3, 3, 3, 4, 4, 3, 3, 3, 3, 3, 3, 4, 4,
		};

		private static readonly int[] DETritsTable = new int[] { 0, 204, 93, 44, 22, 11, 5 };
		private static readonly int[] DEQuintsTable = new int[] { 0, 113, 54, 26, 13, 6 };
	}
}
