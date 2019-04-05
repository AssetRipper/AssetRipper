using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Astc
{
	public sealed class AstcDecoder
	{
		private class BlockData
		{
			public BlockData()
			{
				endpoints = new int[4][];
				for (int i = 0; i < 4; i++)
				{
					endpoints[i] = new int[8];
				}

				weights = new int[144][];
				for (int i = 0; i < 144; i++)
				{
					weights[i] = new int[2];
				}
			}

			public int bw;
			public int bh;
			public int width;
			public int height;
			public int part_num;
			public int dual_plane;
			public int plane_selector;
			public int weight_range;
			public int weight_num; // max: 120
			public int[] cem = new int[4];
			public int cem_range;
			public int endpoint_value_num; // max: 32
			public int[][] endpoints;
			public int[][] weights;
			public int[] partition = new int[144];
		}

		private struct IntSeqData
		{
			public int bits;
			public int nonbits;
		}

		public byte[] DecodeASTC(byte[] input, int width, int height, int blockWidth, int blockHeight)
		{
			byte[] output = new byte[width * height * 4];
			DecodeASTC(input, width, height, blockWidth, blockHeight, output);
			return output;
		}

		public void DecodeASTC(byte[] input, int width, int height, int blockWidth, int blockHeight, byte[] output)
		{
			const int BlockLength = 16;

			int inputOffset = 0;
			int width4 = width * 4;
			int blockWidth4 = blockWidth * 4;
			int blockCountWidth = (width + blockWidth - 1) / blockWidth;
			int blockCountHeight = (height + blockHeight - 1) / blockHeight;
			int clenLast = (width + blockWidth - 1) % blockWidth + 1;
			int clenLast4 = clenLast * 4;
			uint[] blockBuffer = new uint[blockWidth * blockHeight];

			for (int t = 0; t < blockCountHeight; t++)
			{
				for (int s = 0; s < blockCountWidth; s++, inputOffset += BlockLength)
				{
					DecodeBlock(input, inputOffset, blockWidth, blockHeight, blockBuffer);
					int clen = s < blockCountWidth - 1 ? blockWidth4 : clenLast4;
					for (int i = 0, y = height - t * blockHeight - 1; i < blockHeight && y >= 0; i++, y--)
					{
						Buffer.BlockCopy(blockBuffer, i * blockWidth4, output, y * width4 + s * blockWidth4, clen);
					}
				}
			}
		}

		private void DecodeBlock(byte[] input, int ioff, int blockWidth, int blockHeight, uint[] output)
		{
			if (input[ioff] == 0xfc && (input[ioff + 1] & 1) == 1)
			{
				uint c = Color(input[ioff + 9], input[ioff + 11], input[ioff + 13], input[ioff + 15]);
				for (int i = 0; i < blockWidth * blockHeight; i++)
				{
					output[i] = c;
				}
			}
			else
			{
				m_blockData.bw = blockWidth;
				m_blockData.bh = blockHeight;
				DecodeBlockParameters(input, ioff, m_blockData);
				DecodeEndpoints(input, ioff, m_blockData);
				DecodeWeights(input, ioff, m_blockData);
				if (m_blockData.part_num > 1)
				{
					SelectPartition(input, ioff, m_blockData);
				}
				ApplicateColor(m_blockData, output);
			}
		}

		private static void DecodeBlockParameters(byte[] input, int ioff, BlockData block_data)
		{
			block_data.dual_plane = (input[ioff + 1] & 4) >> 2;
			block_data.weight_range = (input[ioff + 0] >> 4 & 1) | (input[ioff + 1] << 2 & 8);

			if ((input[ioff] & 3) != 0)
			{
				block_data.weight_range |= input[ioff] << 1 & 6;
				switch (input[ioff] & 0xc)
				{
					case 0:
						block_data.width = (BitConverter.ToInt32(input, ioff) >> 7 & 3) + 4;
						block_data.height = (input[ioff] >> 5 & 3) + 2;
						break;
					case 4:
						block_data.width = (BitConverter.ToInt32(input, ioff) >> 7 & 3) + 8;
						block_data.height = (input[ioff] >> 5 & 3) + 2;
						break;
					case 8:
						block_data.width = (input[ioff] >> 5 & 3) + 2;
						block_data.height = (BitConverter.ToInt32(input, ioff) >> 7 & 3) + 8;
						break;
					case 12:
						if ((input[ioff + 1] & 1) != 0)
						{
							block_data.width = (input[ioff] >> 7 & 1) + 2;
							block_data.height = (input[ioff] >> 5 & 3) + 2;
						}
						else
						{
							block_data.width = (input[ioff] >> 5 & 3) + 2;
							block_data.height = (input[ioff] >> 7 & 1) + 6;
						}
						break;
				}
			}
			else
			{
				block_data.weight_range |= input[ioff] >> 1 & 6;
				switch (BitConverter.ToInt32(input, ioff) & 0x180)
				{
					case 0:
						block_data.width = 12;
						block_data.height = (input[ioff] >> 5 & 3) + 2;
						break;
					case 0x80:
						block_data.width = (input[ioff] >> 5 & 3) + 2;
						block_data.height = 12;
						break;
					case 0x100:
						block_data.width = (input[ioff] >> 5 & 3) + 6;
						block_data.height = (input[ioff + 1] >> 1 & 3) + 6;
						block_data.dual_plane = 0;
						block_data.weight_range &= 7;
						break;
					case 0x180:
						block_data.width = (input[ioff] & 0x20) != 0 ? 10 : 6;
						block_data.height = (input[ioff] & 0x20) != 0 ? 6 : 10;
						break;
				}
			}

			block_data.part_num = (input[ioff + 1] >> 3 & 3) + 1;

			block_data.weight_num = block_data.width * block_data.height;
			if (block_data.dual_plane != 0)
				block_data.weight_num *= 2;

			int weight_bits, config_bits, cem_base = 0;

			switch (WeightPrecTableA[block_data.weight_range])
			{
				case 3:
					weight_bits = block_data.weight_num * WeightPrecTableB[block_data.weight_range] + (block_data.weight_num * 8 + 4) / 5;
					break;
				case 5:
					weight_bits = block_data.weight_num * WeightPrecTableB[block_data.weight_range] + (block_data.weight_num * 7 + 2) / 3;
					break;
				default:
					weight_bits = block_data.weight_num * WeightPrecTableB[block_data.weight_range];
					break;
			}

			if (block_data.part_num == 1)
			{
				block_data.cem[0] = BitConverter.ToInt32(input, ioff + 1) >> 5 & 0xf;
				config_bits = 17;
			}
			else
			{
				cem_base = BitConverter.ToInt32(input, ioff + 2) >> 7 & 3;
				if (cem_base == 0)
				{
					int cem = input[ioff + 3] >> 1 & 0xf;
					for (int i = 0; i < block_data.part_num; i++)
					{
						block_data.cem[i] = cem;
					}
					config_bits = 29;
				}
				else
				{
					for (int i = 0; i < block_data.part_num; i++)
					{
						block_data.cem[i] = ((input[ioff + 3] >> (i + 1) & 1) + cem_base - 1) << 2;
					}
					switch (block_data.part_num)
					{
						case 2:
							block_data.cem[0] |= input[ioff + 3] >> 3 & 3;
							block_data.cem[1] |= GetBits(input, ioff, 126 - weight_bits, 2);
							break;
						case 3:
							block_data.cem[0] |= input[ioff + 3] >> 4 & 1;
							block_data.cem[0] |= GetBits(input, ioff, 122 - weight_bits, 2) & 2;
							block_data.cem[1] |= GetBits(input, ioff, 124 - weight_bits, 2);
							block_data.cem[2] |= GetBits(input, ioff, 126 - weight_bits, 2);
							break;
						case 4:
							for (int i = 0; i < 4; i++)
							{
								block_data.cem[i] |= GetBits(input, ioff, 120 + i * 2 - weight_bits, 2);
							}
							break;
					}
					config_bits = 25 + block_data.part_num * 3;
				}
			}

			if (block_data.dual_plane != 0)
			{
				config_bits += 2;
				block_data.plane_selector = GetBits(input, ioff, cem_base != 0 ? 130 - weight_bits - block_data.part_num * 3 : 126 - weight_bits, 2);
			}

			int remain_bits = 128 - config_bits - weight_bits;

			block_data.endpoint_value_num = 0;
			for (int i = 0; i < block_data.part_num; i++)
			{
				block_data.endpoint_value_num += (block_data.cem[i] >> 1 & 6) + 2;
			}

			for (int i = 0, endpoint_bits; i < CemTableA.Count; i++)
			{
				switch (CemTableA[i])
				{
					case 3:
						endpoint_bits = block_data.endpoint_value_num * CemTableB[i] + (block_data.endpoint_value_num * 8 + 4) / 5;
						break;
					case 5:
						endpoint_bits = block_data.endpoint_value_num * CemTableB[i] + (block_data.endpoint_value_num * 7 + 2) / 3;
						break;
					default:
						endpoint_bits = block_data.endpoint_value_num * CemTableB[i];
						break;
				}

				if (endpoint_bits <= remain_bits)
				{
					block_data.cem_range = i;
					break;
				}
			}
		}

		private void DecodeEndpoints(byte[] input, int ioff, BlockData data)
		{
			DecodeIntseq(input, ioff, data.part_num == 1 ? 17 : 29, CemTableA[data.cem_range], CemTableB[data.cem_range], data.endpoint_value_num, false, m_epSeq);

			switch (CemTableA[data.cem_range])
			{
				case 3:
					for (int i = 0, b = 0, c = DETritsTable[CemTableB[data.cem_range]]; i < data.endpoint_value_num; i++)
					{
						int a = (m_epSeq[i].bits & 1) * 0x1ff;
						int x = m_epSeq[i].bits >> 1;
						switch (CemTableB[data.cem_range])
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
						m_ev[i] = (a & 0x80) | ((m_epSeq[i].nonbits * c + b) ^ a) >> 2;
					}
					break;

				case 5:
					for (int i = 0, b = 0, c = DEQuintsTable[CemTableB[data.cem_range]]; i < data.endpoint_value_num; i++)
					{
						int a = (m_epSeq[i].bits & 1) * 0x1ff;
						int x = m_epSeq[i].bits >> 1;
						switch (CemTableB[data.cem_range])
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
						m_ev[i] = (a & 0x80) | ((m_epSeq[i].nonbits * c + b) ^ a) >> 2;
					}
					break;

				default:
					switch (CemTableB[data.cem_range])
					{
						case 1:
							for (int i = 0; i < data.endpoint_value_num; i++)
								m_ev[i] = m_epSeq[i].bits * 0xff;
							break;
						case 2:
							for (int i = 0; i < data.endpoint_value_num; i++)
								m_ev[i] = m_epSeq[i].bits * 0x55;
							break;
						case 3:
							for (int i = 0; i < data.endpoint_value_num; i++)
								m_ev[i] = m_epSeq[i].bits << 5 | m_epSeq[i].bits << 2 | m_epSeq[i].bits >> 1;
							break;
						case 4:
							for (int i = 0; i < data.endpoint_value_num; i++)
								m_ev[i] = m_epSeq[i].bits << 4 | m_epSeq[i].bits;
							break;
						case 5:
							for (int i = 0; i < data.endpoint_value_num; i++)
								m_ev[i] = m_epSeq[i].bits << 3 | m_epSeq[i].bits >> 2;
							break;
						case 6:
							for (int i = 0; i < data.endpoint_value_num; i++)
								m_ev[i] = m_epSeq[i].bits << 2 | m_epSeq[i].bits >> 4;
							break;
						case 7:
							for (int i = 0; i < data.endpoint_value_num; i++)
								m_ev[i] = m_epSeq[i].bits << 1 | m_epSeq[i].bits >> 6;
							break;
						case 8:
							for (int i = 0; i < data.endpoint_value_num; i++)
								m_ev[i] = m_epSeq[i].bits;
							break;
					}
					break;
			}

			int v = 0;
			for (int cem = 0; cem < data.part_num; v += (data.cem[cem] / 4 + 1) * 2, cem++)
			{
				switch (data.cem[cem])
				{
					case 0:
						SetEndpoint(data.endpoints[cem], m_ev[v], m_ev[v], m_ev[v], 255, m_ev[v + 1], m_ev[v + 1], m_ev[v + 1], 255);
						break;
					case 1:
						{
							int l0 = (m_ev[v] >> 2) | (m_ev[v + 1] & 0xc0);
							int l1 = Clamp(l0 + (m_ev[v + 1] & 0x3f));
							SetEndpoint(data.endpoints[cem], l0, l0, l0, 255, l1, l1, l1, 255);
						}
						break;
					case 4:
						SetEndpoint(data.endpoints[cem], m_ev[v], m_ev[v], m_ev[v], m_ev[v + 2], m_ev[v + 1], m_ev[v + 1], m_ev[v + 1], m_ev[v + 3]);
						break;
					case 5:
						BitTransferSigned(m_ev, v + 1, v + 0);
						BitTransferSigned(m_ev, v + 3, v + 2);
						m_ev[v + 1] += m_ev[v + 0];
						SetEndpointClamp(data.endpoints[cem], m_ev[v], m_ev[v], m_ev[v], m_ev[v + 2], m_ev[v + 1], m_ev[v + 1], m_ev[v + 1], m_ev[v + 2] + m_ev[v + 3]);
						break;
					case 6:
						SetEndpoint(data.endpoints[cem], m_ev[v] * m_ev[v + 3] >> 8, m_ev[v + 1] * m_ev[v + 3] >> 8, m_ev[v + 2] * m_ev[v + 3] >> 8, 255, m_ev[v], m_ev[v + 1], m_ev[v + 2], 255);
						break;
					case 8:
						if (m_ev[v] + m_ev[v + 2] + m_ev[v + 4] <= m_ev[v + 1] + m_ev[v + 3] + m_ev[v + 5])
							SetEndpoint(data.endpoints[cem], m_ev[v], m_ev[v + 2], m_ev[v + 4], 255, m_ev[v + 1], m_ev[v + 3], m_ev[v + 5], 255);
						else
							SetEndpointBlue(data.endpoints[cem], m_ev[v + 1], m_ev[v + 3], m_ev[v + 5], 255, m_ev[v + 0], m_ev[v + 2], m_ev[v + 4], 255);
						break;
					case 9:
						BitTransferSigned(m_ev, v + 1, v + 0);
						BitTransferSigned(m_ev, v + 3, v + 2);
						BitTransferSigned(m_ev, v + 5, v + 4);
						if (m_ev[v + 1] + m_ev[v + 3] + m_ev[v + 5] >= 0)
							SetEndpointClamp(data.endpoints[cem], m_ev[v], m_ev[v + 2], m_ev[v + 4], 255, m_ev[v + 0] + m_ev[v + 1], m_ev[v + 2] + m_ev[v + 3], m_ev[v + 4] + m_ev[v + 5], 255);
						else
							SetEndpointBlueClamp(data.endpoints[cem], m_ev[v] + m_ev[v + 1], m_ev[v + 2] + m_ev[v + 3], m_ev[v + 4] + m_ev[v + 5], 255, m_ev[v], m_ev[v + 2], m_ev[v + 4], 255);
						break;
					case 10:
						SetEndpoint(data.endpoints[cem], m_ev[v] * m_ev[v + 3] >> 8, m_ev[v + 1] * m_ev[v + 3] >> 8, m_ev[v + 2] * m_ev[v + 3] >> 8, m_ev[v + 4], m_ev[v], m_ev[v + 1], m_ev[v + 2], m_ev[v + 5]);
						break;
					case 12:
						if (m_ev[v] + m_ev[v + 2] + m_ev[v + 4] <= m_ev[v + 1] + m_ev[v + 3] + m_ev[v + 5])
							SetEndpoint(data.endpoints[cem], m_ev[v], m_ev[v + 2], m_ev[v + 4], m_ev[v + 6], m_ev[v + 1], m_ev[v + 3], m_ev[v + 5], m_ev[v + 7]);
						else
							SetEndpointBlue(data.endpoints[cem], m_ev[v + 1], m_ev[v + 3], m_ev[v + 5], m_ev[v + 7], m_ev[v], m_ev[v + 2], m_ev[v + 4], m_ev[v + 6]);
						break;
					case 13:
						BitTransferSigned(m_ev, v + 1, v + 0);
						BitTransferSigned(m_ev, v + 3, v + 2);
						BitTransferSigned(m_ev, v + 5, v + 4);
						BitTransferSigned(m_ev, v + 7, v + 6);
						if (m_ev[v + 1] + m_ev[v + 3] + m_ev[v + 5] >= 0)
							SetEndpointClamp(data.endpoints[cem], m_ev[v], m_ev[v + 2], m_ev[v + 4], m_ev[v + 6], m_ev[v] + m_ev[v + 1], m_ev[v + 2] + m_ev[v + 3], m_ev[v + 4] + m_ev[v + 5], m_ev[v + 6] + m_ev[v + 7]);
						else
							SetEndpointBlueClamp(data.endpoints[cem], m_ev[v] + m_ev[v + 1], m_ev[v + 2] + m_ev[v + 3], m_ev[v + 4] + m_ev[v + 5], m_ev[v + 6] + m_ev[v + 7], m_ev[v], m_ev[v + 2], m_ev[v + 4], m_ev[v + 6]);
						break;
					default:
						throw new Exception("Unsupported ASTC format");
				}
			}
		}

		private void DecodeWeights(byte[] input, int ioff, BlockData data)
		{
			DecodeIntseq(input, ioff, 128, WeightPrecTableA[data.weight_range], WeightPrecTableB[data.weight_range], data.weight_num, true, m_wSeq);

			if (WeightPrecTableA[data.weight_range] == 0)
			{
				switch (WeightPrecTableB[data.weight_range])
				{
					case 1:
						for (int i = 0; i < data.weight_num; i++)
							m_wv[i] = m_wSeq[i].bits != 0 ? 63 : 0;
						break;
					case 2:
						for (int i = 0; i < data.weight_num; i++)
							m_wv[i] = m_wSeq[i].bits << 4 | m_wSeq[i].bits << 2 | m_wSeq[i].bits;
						break;
					case 3:
						for (int i = 0; i < data.weight_num; i++)
							m_wv[i] = m_wSeq[i].bits << 3 | m_wSeq[i].bits;
						break;
					case 4:
						for (int i = 0; i < data.weight_num; i++)
							m_wv[i] = m_wSeq[i].bits << 2 | m_wSeq[i].bits >> 2;
						break;
					case 5:
						for (int i = 0; i < data.weight_num; i++)
							m_wv[i] = m_wSeq[i].bits << 1 | m_wSeq[i].bits >> 4;
						break;
				}
				for (int i = 0; i < data.weight_num; i++)
				{
					if (m_wv[i] > 32)
					{
						++m_wv[i];
					}
				}
			}
			else if (WeightPrecTableB[data.weight_range] == 0)
			{
				int s = WeightPrecTableA[data.weight_range] == 3 ? 32 : 16;
				for (int i = 0; i < data.weight_num; i++)
				{
					m_wv[i] = m_wSeq[i].nonbits * s;
				}
			}
			else
			{
				if (WeightPrecTableA[data.weight_range] == 3)
				{
					switch (WeightPrecTableB[data.weight_range])
					{
						case 1:
							for (int i = 0; i < data.weight_num; i++)
							{
								m_wv[i] = m_wSeq[i].nonbits * 50;
							}
							break;
						case 2:
							for (int i = 0; i < data.weight_num; i++)
							{
								m_wv[i] = m_wSeq[i].nonbits * 23;
								if ((m_wSeq[i].bits & 2) != 0)
								{
									m_wv[i] += 0b1000101;
								}
							}
							break;
						case 3:
							for (int i = 0; i < data.weight_num; i++)
							{
								m_wv[i] = m_wSeq[i].nonbits * 11 + ((m_wSeq[i].bits << 4 | m_wSeq[i].bits >> 1) & 0b1100011);
							}
							break;
					}
				}
				else if (WeightPrecTableA[data.weight_range] == 5)
				{
					switch (WeightPrecTableB[data.weight_range])
					{
						case 1:
							for (int i = 0; i < data.weight_num; i++)
								m_wv[i] = m_wSeq[i].nonbits * 28;
							break;
						case 2:
							for (int i = 0; i < data.weight_num; i++)
							{
								m_wv[i] = m_wSeq[i].nonbits * 13;
								if ((m_wSeq[i].bits & 2) != 0)
								{
									m_wv[i] += 0b1000010;
								}
							}
							break;
					}
				}
				for (int i = 0; i < data.weight_num; i++)
				{
					int a = (m_wSeq[i].bits & 1) * 0x7f;
					m_wv[i] = (a & 0x20) | ((m_wv[i] ^ a) >> 2);
					if (m_wv[i] > 32)
					{
						++m_wv[i];
					}
				}
			}

			int ds = (1024 + data.bw / 2) / (data.bw - 1);
			int dt = (1024 + data.bh / 2) / (data.bh - 1);
			int pn = data.dual_plane != 0 ? 2 : 1;

			for (int t = 0, i = 0; t < data.bh; t++)
			{
				for (int s = 0; s < data.bw; s++, i++)
				{
					int gs = (ds * s * (data.width - 1) + 32) >> 6;
					int gt = (dt * t * (data.height - 1) + 32) >> 6;
					int fs = gs & 0xf;
					int ft = gt & 0xf;
					int v = (gs >> 4) + (gt >> 4) * data.width;
					int w11 = (fs * ft + 8) >> 4;
					int w10 = ft - w11;
					int w01 = fs - w11;
					int w00 = 16 - fs - ft + w11;

					for (int p = 0; p < pn; p++)
					{
						int p00 = m_wv[v * pn + p];
						int p01 = m_wv[(v + 1) * pn + p];
						int p10 = m_wv[(v + data.width) * pn + p];
						int p11 = m_wv[(v + data.width + 1) * pn + p];
						data.weights[i][p] = (p00 * w00 + p01 * w01 + p10 * w10 + p11 * w11 + 8) >> 4;
					}
				}
			}
		}

		private void SelectPartition(byte[] input, int ioff, BlockData data)
		{
			bool small_block = data.bw * data.bh < 31;
			int seed = (BitConverter.ToInt32(input, ioff) >> 13 & 0x3ff) | (data.part_num - 1) << 10;

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

			for (int i = 0; i < 8; i++)
			{
				m_seeds[i] = (int)((rnum >> (i * 4)) & 0xF);
				m_seeds[i] *= m_seeds[i];
			}

			m_sh[0] = (seed & 2) != 0 ? 4 : 5;
			m_sh[1] = data.part_num == 3 ? 6 : 5;

			if ((seed & 1) != 0)
			{
				for (int i = 0; i < 8; i++)
				{
					m_seeds[i] >>= m_sh[i % 2];
				}
			}
			else
			{
				for (int i = 0; i < 8; i++)
				{
					m_seeds[i] >>= m_sh[1 - i % 2];
				}
			}

			if (small_block)
			{
				for (int t = 0, i = 0; t < data.bh; t++)
				{
					for (int s = 0; s < data.bw; s++, i++)
					{
						int x = s << 1;
						int y = t << 1;
						int a = (int)((m_seeds[0] * x + m_seeds[1] * y + (rnum >> 14)) & 0x3f);
						int b = (int)((m_seeds[2] * x + m_seeds[3] * y + (rnum >> 10)) & 0x3f);
						int c = (int)(data.part_num < 3 ? 0 : (m_seeds[4] * x + m_seeds[5] * y + (rnum >> 6)) & 0x3f);
						int d = (int)(data.part_num < 4 ? 0 : (m_seeds[6] * x + m_seeds[7] * y + (rnum >> 2)) & 0x3f);
						data.partition[i] = (a >= b && a >= c && a >= d) ? 0 : (b >= c && b >= d) ? 1 : (c >= d) ? 2 : 3;
					}
				}
			}
			else
			{
				for (int y = 0, i = 0; y < data.bh; y++)
				{
					for (int x = 0; x < data.bw; x++, i++)
					{
						int a = (int)((m_seeds[0] * x + m_seeds[1] * y + (rnum >> 14)) & 0x3f);
						int b = (int)((m_seeds[2] * x + m_seeds[3] * y + (rnum >> 10)) & 0x3f);
						int c = (int)(data.part_num < 3 ? 0 : (m_seeds[4] * x + m_seeds[5] * y + (rnum >> 6)) & 0x3f);
						int d = (int)(data.part_num < 4 ? 0 : (m_seeds[6] * x + m_seeds[7] * y + (rnum >> 2)) & 0x3f);
						data.partition[i] = (a >= b && a >= c && a >= d) ? 0 : (b >= c && b >= d) ? 1 : (c >= d) ? 2 : 3;
					}
				}
			}
		}

		private void ApplicateColor(BlockData data, uint[] output)
		{
			if (data.dual_plane != 0)
			{
				m_ps[0] = m_ps[1] = m_ps[2] = m_ps[3] = 0;
				m_ps[data.plane_selector] = 1;
				if (data.part_num > 1)
				{
					for (int i = 0; i < data.bw * data.bh; i++)
					{
						int p = data.partition[i];
						byte r = SelectColor(data.endpoints[p][0], data.endpoints[p][4], data.weights[i][m_ps[0]]);
						byte g = SelectColor(data.endpoints[p][1], data.endpoints[p][5], data.weights[i][m_ps[1]]);
						byte b = SelectColor(data.endpoints[p][2], data.endpoints[p][6], data.weights[i][m_ps[2]]);
						byte a = SelectColor(data.endpoints[p][3], data.endpoints[p][7], data.weights[i][m_ps[3]]);
						output[i] = Color(r, g, b, a);
					}
				}
				else
				{
					for (int i = 0; i < data.bw * data.bh; i++)
					{
						byte r = SelectColor(data.endpoints[0][0], data.endpoints[0][4], data.weights[i][m_ps[0]]);
						byte g = SelectColor(data.endpoints[0][1], data.endpoints[0][5], data.weights[i][m_ps[1]]);
						byte b = SelectColor(data.endpoints[0][2], data.endpoints[0][6], data.weights[i][m_ps[2]]);
						byte a = SelectColor(data.endpoints[0][3], data.endpoints[0][7], data.weights[i][m_ps[3]]);
						output[i] = Color(r, g, b, a);
					}
				}
			}
			else if (data.part_num > 1)
			{
				for (int i = 0; i < data.bw * data.bh; i++)
				{
					int p = data.partition[i];
					byte r = SelectColor(data.endpoints[p][0], data.endpoints[p][4], data.weights[i][0]);
					byte g = SelectColor(data.endpoints[p][1], data.endpoints[p][5], data.weights[i][0]);
					byte b = SelectColor(data.endpoints[p][2], data.endpoints[p][6], data.weights[i][0]);
					byte a = SelectColor(data.endpoints[p][3], data.endpoints[p][7], data.weights[i][0]);
					output[i] = Color(r, g, b, a);
				}
			}
			else
			{
				for (int i = 0; i < data.bw * data.bh; i++)
				{
					byte r = SelectColor(data.endpoints[0][0], data.endpoints[0][4], data.weights[i][0]);
					byte g = SelectColor(data.endpoints[0][1], data.endpoints[0][5], data.weights[i][0]);
					byte b = SelectColor(data.endpoints[0][2], data.endpoints[0][6], data.weights[i][0]);
					byte a = SelectColor(data.endpoints[0][3], data.endpoints[0][7], data.weights[i][0]);
					output[i] = Color(r, g, b, a);
				}
			}
		}

		private static void DecodeIntseq(byte[] input, int ioff, int offset, int a, int b, int count, bool reverse, IntSeqData[] _out)
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
						ulong d = BitReverseU64(GetBits64(input, ioff, p - now_size, now_size), now_size);
						int x = (int)((d >> b & 3) | (d >> b * 2 & 0xc) | (d >> b * 3 & 0x10) | (d >> b * 4 & 0x60) | (d >> b * 5 & 0x80));
						for (int j = 0; j < 5 && n < count; j++, n++)
						{
							_out[n] = new IntSeqData()
							{
								bits = (int)(d >> (DImt[j] + b * j)) & mask,
								nonbits = DITritsTable[j][x],
							};
						}
					}
				}
				else
				{
					for (int i = 0, p = offset; i < block_count; i++, p += block_size)
					{
						ulong d = GetBits64(input, ioff, p, (i < block_count - 1) ? block_size : last_block_size);
						int x = (int)((d >> b & 3) | (d >> b * 2 & 0xc) | (d >> b * 3 & 0x10) | (d >> b * 4 & 0x60) | (d >> b * 5 & 0x80));
						for (int j = 0; j < 5 && n < count; j++, n++)
						{
							_out[n] = new IntSeqData()
							{
								bits = unchecked((int)(d >> (DImt[j] + b * j))) & mask,
								nonbits = DITritsTable[j][x],
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
						ulong d = BitReverseU64(GetBits64(input, ioff, p - now_size, now_size), now_size);
						int x = (int)((d >> b & 7) | (d >> b * 2 & 0x18) | (d >> b * 3 & 0x60));
						for (int j = 0; j < 3 && n < count; j++, n++)
						{
							_out[n] = new IntSeqData()
							{
								bits = (int)d >> (DImq[j] + b * j) & mask,
								nonbits = DIQuintsTable[j][x],
							};
						}
					}
				}
				else
				{
					for (int i = 0, p = offset; i < block_count; i++, p += block_size)
					{
						ulong d = GetBits64(input, ioff, p, (i < block_count - 1) ? block_size : last_block_size);
						int x = (int)((d >> b & 7) | (d >> b * 2 & 0x18) | (d >> b * 3 & 0x60));
						for (int j = 0; j < 3 && n < count; j++, n++)
						{
							_out[n] = new IntSeqData()
							{
								bits = (int)d >> (DImq[j] + b * j) & mask,
								nonbits = DIQuintsTable[j][x],
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
							bits = BitReverseU8((byte)GetBits(input, ioff, p, b), b),
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
							bits = GetBits(input, ioff, p, b),
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
		private static int GetBits(byte[] input, int ioff, int bit, int len)
		{
			return (BitConverter.ToInt32(input, ioff + bit / 8) >> (bit % 8)) & ((1 << len) - 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ulong GetBits64(byte[] input, int ioff, int bit, int len)
		{
			ulong mask = len == 64 ? 0xffffffffffffffff : (1UL << len) - 1;
			if (len < 1)
				return 0;
			else if (bit >= 64)
				return BitConverter.ToUInt64(input, ioff + 8) >> (bit - 64) & mask;
			else if (bit <= 0)
				return BitConverter.ToUInt64(input, ioff) << -bit & mask;
			else if (bit + len <= 64)
				return BitConverter.ToUInt64(input, ioff) >> bit & mask;
			else
				return (BitConverter.ToUInt64(input, ioff) >> bit | BitConverter.ToUInt64(input, ioff + 8) << (64 - bit)) & mask;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static byte Clamp(int n)
		{
			return n < 0 ? byte.MinValue : n > 255 ? byte.MaxValue : (byte)n;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void BitTransferSigned(int[] buffer, int index1, int index2)
		{
			int value1 = buffer[index1];
			int value2 = buffer[index2];
			BitTransferSigned(ref value1, ref value2);
			buffer[index1] = value1;
			buffer[index2] = value2;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void BitTransferSigned(ref int a, ref int b)
		{
			b = (b >> 1) | (a & 0x80);
			a = (a >> 1) & 0x3f;
			if ((a & 0x20) != 0)
				a -= 0x40;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void SetEndpoint(int[] endpoint, int r1, int g1, int b1, int a1, int r2, int g2, int b2, int a2)
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
		private static void SetEndpointClamp(int[] endpoint, int r1, int g1, int b1, int a1, int r2, int g2, int b2, int a2)
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
		private static void SetEndpointBlue(int[] endpoint, int r1, int g1, int b1, int a1, int r2, int g2, int b2, int a2)
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
		private static void SetEndpointBlueClamp(int[] endpoint, int r1, int g1, int b1, int a1, int r2, int g2, int b2, int a2)
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

		private static readonly IReadOnlyList<byte> BitReverseTable = new byte[]
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

		private static readonly IReadOnlyList<int> WeightPrecTableA = new int[] { 0, 0, 0, 3, 0, 5, 3, 0, 0, 0, 5, 3, 0, 5, 3, 0 };
		private static readonly IReadOnlyList<int> WeightPrecTableB = new int[] { 0, 0, 1, 0, 2, 0, 1, 3, 0, 0, 1, 2, 4, 2, 3, 5 };

		private static readonly IReadOnlyList<int> CemTableA = new int[] { 0, 3, 5, 0, 3, 5, 0, 3, 5, 0, 3, 5, 0, 3, 5, 0, 3, 0, 0 };
		private static readonly IReadOnlyList<int> CemTableB = new int[] { 8, 6, 5, 7, 5, 4, 6, 4, 3, 5, 3, 2, 4, 2, 1, 3, 1, 2, 1 };

		private static readonly IReadOnlyList<int> DImt = new int[] { 0, 2, 4, 5, 7 };
		private static readonly IReadOnlyList<int> DImq = new int[] { 0, 3, 5 };
		private static readonly int[][] DITritsTable =
		{
			new int[] {0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 1, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 1, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2, 0, 1, 2, 0, 0, 1, 2, 1, 0, 1, 2, 2, 0, 1, 2, 2},
			new int[] {0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 1, 2, 2, 2, 0, 0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 1, 2, 2, 2, 0, 0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 1, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 1, 2, 2, 2, 1, 0, 0, 0, 0, 1, 1, 1, 0, 2, 2, 2, 0, 2, 2, 2, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 1, 2, 2, 2, 1},
			new int[] {0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 2, 2, 2, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 0, 2, 2, 2, 2, 2, 1, 1, 1, 2, 1, 1, 1, 2, 1, 1, 1, 2, 2, 2, 2, 2},
			new int[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2},
			new int[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2}
		};
		private static readonly int[][] DIQuintsTable =
		{
			new int[] {0, 1, 2, 3, 4, 0, 4, 4, 0, 1, 2, 3, 4, 1, 4, 4, 0, 1, 2, 3, 4, 2, 4, 4, 0, 1, 2, 3, 4, 3, 4, 4, 0, 1, 2, 3, 4, 0, 4, 0, 0, 1, 2, 3, 4, 1, 4, 1, 0, 1, 2, 3, 4, 2, 4, 2, 0, 1, 2, 3, 4, 3, 4, 3, 0, 1, 2, 3, 4, 0, 2, 3, 0, 1, 2, 3, 4, 1, 2, 3, 0, 1, 2, 3, 4, 2, 2, 3, 0, 1, 2, 3, 4, 3, 2, 3, 0, 1, 2, 3, 4, 0, 0, 1, 0, 1, 2, 3, 4, 1, 0, 1, 0, 1, 2, 3, 4, 2, 0, 1, 0, 1, 2, 3, 4, 3, 0, 1},
			new int[] {0, 0, 0, 0, 0, 4, 4, 4, 1, 1, 1, 1, 1, 4, 4, 4, 2, 2, 2, 2, 2, 4, 4, 4, 3, 3, 3, 3, 3, 4, 4, 4, 0, 0, 0, 0, 0, 4, 0, 4, 1, 1, 1, 1, 1, 4, 1, 4, 2, 2, 2, 2, 2, 4, 2, 4, 3, 3, 3, 3, 3, 4, 3, 4, 0, 0, 0, 0, 0, 4, 0, 0, 1, 1, 1, 1, 1, 4, 1, 1, 2, 2, 2, 2, 2, 4, 2, 2, 3, 3, 3, 3, 3, 4, 3, 3, 0, 0, 0, 0, 0, 4, 0, 0, 1, 1, 1, 1, 1, 4, 1, 1, 2, 2, 2, 2, 2, 4, 2, 2, 3, 3, 3, 3, 3, 4, 3, 3},
			new int[] {0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 1, 4, 0, 0, 0, 0, 0, 0, 2, 4, 0, 0, 0, 0, 0, 0, 3, 4, 1, 1, 1, 1, 1, 1, 4, 4, 1, 1, 1, 1, 1, 1, 4, 4, 1, 1, 1, 1, 1, 1, 4, 4, 1, 1, 1, 1, 1, 1, 4, 4, 2, 2, 2, 2, 2, 2, 4, 4, 2, 2, 2, 2, 2, 2, 4, 4, 2, 2, 2, 2, 2, 2, 4, 4, 2, 2, 2, 2, 2, 2, 4, 4, 3, 3, 3, 3, 3, 3, 4, 4, 3, 3, 3, 3, 3, 3, 4, 4, 3, 3, 3, 3, 3, 3, 4, 4, 3, 3, 3, 3, 3, 3, 4, 4}
		};

		private static readonly IReadOnlyList<int> DETritsTable = new int[] { 0, 204, 93, 44, 22, 11, 5 };
		private static readonly IReadOnlyList<int> DEQuintsTable = new int[] { 0, 113, 54, 26, 13, 6 };

		private readonly BlockData m_blockData = new BlockData();
		private readonly IntSeqData[] m_epSeq = new IntSeqData[32];
		private readonly int[] m_ev = new int[32];
		private readonly IntSeqData[] m_wSeq = new IntSeqData[128];
		private readonly int[] m_wv = new int[128];
		private readonly int[] m_seeds = new int[8];
		private readonly int[] m_ps = new int[4];
		private readonly int[] m_sh = new int[2];
	}
}
