using System;
using System.IO;
using System.Text;

namespace UtinyRipper.Converter.Textures.DDS
{
	public static class DDSDecompressor
	{
		private struct Color8888
		{
			public byte Red { get; set; }
			public byte Green { get; set; }
			public byte Blue { get; set; }
			public byte Alpha { get; set; }
		}

		private struct ColorMask
		{
			public int Shift1 { get; set; }
			public int Shift2 { get; set; }
			public int Mult { get; set; }
		}

		public static long GetDecompressedSize(DDSConvertParameters @params)
		{
			int width = @params.Width;
			int height = @params.Height;
			int depth = @params.BitMapDepth;
			int bpp = GetRGBABytesPerPixel(@params);
			int bpc = GetBytesPerColor(@params);
			int bps = width * bpp * bpc;
			long sizeOfPlane = bps * height;

			return depth * sizeOfPlane + height * bps + width * bpp;
		}

		public static void DecompressDXT1(Stream destination, Stream source, DDSConvertParameters @params)
		{
			int depth = @params.BitMapDepth;
			int width = @params.Width;
			int height = @params.Height;
			int bpp = GetRGBABytesPerPixel(@params);
			int bpc = GetBytesPerColor(@params);
			int bps = width * bpp * bpc;
			long sizeOfPlane = bps * height;

			Color8888[] colors = new Color8888[4];

			long position = destination.Position;
			using (BinaryReader reader = new BinaryReader(source, Encoding.UTF8, true))
			{
				for (int z = 0; z < depth; z++)
				{
					// mirror Y
					for (int y = height - 1; y >= 0; y -= 4)
					{
						for (int x = 0; x < width; x += 4)
						{
							ushort color0 = reader.ReadUInt16();
							ushort color1 = reader.ReadUInt16();
							uint bitMask = reader.ReadUInt32();

							colors[0] = DxtcRead3bColor(color0);
							colors[1] = DxtcRead3bColor(color1);

							if (color0 > color1)
							{
								// Four-color block: derive the other two colors.
								// 00 = color_0, 01 = color_1, 10 = color_2, 11 = color_3
								// These 2-bit codes correspond to the 2-bit fields
								// stored in the 64-bit block.
								colors[2].Blue = unchecked((byte)((2 * colors[0].Blue + colors[1].Blue + 1) / 3));
								colors[2].Green = unchecked((byte)((2 * colors[0].Green + colors[1].Green + 1) / 3));
								colors[2].Red = unchecked((byte)((2 * colors[0].Red + colors[1].Red + 1) / 3));

								colors[2].Alpha = 0xFF;
								colors[3].Alpha = 0xFF;
							}
							else
							{
								// Three-color block: derive the other color.
								// 00 = color_0,  01 = color_1,  10 = color_2,
								// 11 = transparent.
								// These 2-bit codes correspond to the 2-bit fields 
								// stored in the 64-bit block.
								colors[2].Blue = unchecked((byte)((colors[0].Blue + colors[1].Blue) / 2));
								colors[2].Green = unchecked((byte)((colors[0].Green + colors[1].Green) / 2));
								colors[2].Red = unchecked((byte)((colors[0].Red + colors[1].Red) / 2));

								colors[2].Alpha = 0xFF;
								colors[3].Alpha = 0x0;
							}

							colors[3].Blue = unchecked((byte)((colors[0].Blue + 2 * colors[1].Blue + 1) / 3));
							colors[3].Green = unchecked((byte)((colors[0].Green + 2 * colors[1].Green + 1) / 3));
							colors[3].Red = unchecked((byte)((colors[0].Red + 2 * colors[1].Red + 1) / 3));

							int bitIndex = 0;
							for (int ly = 0; ly < 4; ly++)
							{
								for (int lx = 0; lx < 4; lx++, bitIndex++)
								{
									int colorIndex = unchecked((int)((bitMask & (3 << bitIndex * 2)) >> bitIndex * 2));
									Color8888 color = colors[colorIndex];
									if ((x + lx) < width && (y - ly) < height && (y - ly) >= 0)
									{
										// mirror Y
										long offset = z * sizeOfPlane + (y - ly) * bps + (x + lx) * bpp;
										destination.Position = position + offset;

										destination.WriteByte(color.Red);
										destination.WriteByte(color.Green);
										destination.WriteByte(color.Blue);
										destination.WriteByte(color.Alpha);
									}
								}
							}
						}
					}
				}
			}
		}

		public static void DecompressDXT3(Stream destination, Stream source, DDSConvertParameters @params)
		{
			int depth = @params.BitMapDepth;
			int width = @params.Width;
			int height = @params.Height;
			int bpp = GetRGBABytesPerPixel(@params);
			int bpc = GetBytesPerColor(@params);
			int bps = width * bpp * bpc;
			long sizeOfPlane = bps * height;

			Color8888[] colors = new Color8888[4];
			byte[] alphas = new byte[16];

			long position = destination.Position;
			using (BinaryReader reader = new BinaryReader(source, Encoding.UTF8, true))
			{
				for (int z = 0; z < depth; z++)
				{
					// mirror Y
					for (int y = height - 1; y >= 0; y -= 4)
					{
						for (int x = 0; x < width; x += 4)
						{
							for (int i = 0; i < 4; ++i)
							{
								ushort alpha = reader.ReadUInt16();
								alphas[i * 4 + 0] = (byte)(((alpha >> 0) & 0xF) * 0x11);
								alphas[i * 4 + 1] = (byte)(((alpha >> 4) & 0xF) * 0x11);
								alphas[i * 4 + 2] = (byte)(((alpha >> 8) & 0xF) * 0x11);
								alphas[i * 4 + 3] = (byte)(((alpha >> 12) & 0xF) * 0x11);
							}

							ushort color0 = reader.ReadUInt16();
							ushort color1 = reader.ReadUInt16();
							uint bitMask = reader.ReadUInt32();

							colors[0] = DxtcRead3bColor(color0);
							colors[1] = DxtcRead3bColor(color1);

							if (color0 > color1)
							{
								// Four-color block: derive the other two colors.
								// 00 = color_0, 01 = color_1, 10 = color_2, 11 = color_3
								// These 2-bit codes correspond to the 2-bit fields
								// stored in the 64-bit block.
								colors[2].Blue = unchecked((byte)((2 * colors[0].Blue + colors[1].Blue + 1) / 3));
								colors[2].Green = unchecked((byte)((2 * colors[0].Green + colors[1].Green + 1) / 3));
								colors[2].Red = unchecked((byte)((2 * colors[0].Red + colors[1].Red + 1) / 3));
							}
							else
							{
								// Three-color block: derive the other color.
								// 00 = color_0,  01 = color_1,  10 = color_2,
								// 11 = transparent.
								// These 2-bit codes correspond to the 2-bit fields 
								// stored in the 64-bit block.
								colors[2].Blue = unchecked((byte)((colors[0].Blue + colors[1].Blue) / 2));
								colors[2].Green = unchecked((byte)((colors[0].Green + colors[1].Green) / 2));
								colors[2].Red = unchecked((byte)((colors[0].Red + colors[1].Red) / 2));
							}

							colors[3].Blue = unchecked((byte)((colors[0].Blue + 2 * colors[1].Blue + 1) / 3));
							colors[3].Green = unchecked((byte)((colors[0].Green + 2 * colors[1].Green + 1) / 3));
							colors[3].Red = unchecked((byte)((colors[0].Red + 2 * colors[1].Red + 1) / 3));

							int bitIndex = 0;
							for (int ly = 0; ly < 4; ly++)
							{
								for (int lx = 0; lx < 4; lx++, bitIndex++)
								{
									int colorIndex = unchecked((int)((bitMask & (3 << bitIndex * 2)) >> bitIndex * 2));
									Color8888 color = colors[colorIndex];
									color.Alpha = alphas[bitIndex];
									if ((x + lx) < width && (y - ly) < height && (y - ly) >= 0)
									{
										// mirror Y
										long offset = z * sizeOfPlane + (y - ly) * bps + (x + lx) * bpp;
										destination.Position = position + offset;

										destination.WriteByte(color.Red);
										destination.WriteByte(color.Green);
										destination.WriteByte(color.Blue);
										destination.WriteByte(color.Alpha);
									}
								}
							}
						}
					}
				}
			}
		}

		public static void DecompressDXT5(Stream destination, Stream source, DDSConvertParameters @params)
		{
			int depth = @params.BitMapDepth;
			int width = @params.Width;
			int height = @params.Height;
			int bpp = GetRGBABytesPerPixel(@params);
			int bpc = GetBytesPerColor(@params);
			int bps = width * bpp * bpc;
			long sizeOfPlane = bps * height;

			Color8888[] colors = new Color8888[4];
			ushort[] alphas = new ushort[8];
			byte[] alphaMask = new byte[6];

			long position = destination.Position;
			using (BinaryReader reader = new BinaryReader(source, Encoding.UTF8, true))
			{
				for (int z = 0; z < depth; z++)
				{
					// mirror Y
					for (int y = height - 1; y >= 0; y -= 4)
					{
						for (int x = 0; x < width; x += 4)
						{
							alphas[0] = reader.ReadByte();
							alphas[1] = reader.ReadByte();
							reader.Read(alphaMask, 0, alphaMask.Length);

							colors[0] = DxtcRead2bColor(reader);
							colors[1] = DxtcRead2bColor(reader);
							uint bitmask = reader.ReadUInt32();

							// Four-color block: derive the other two colors.
							// 00 = color_0, 01 = color_1, 10 = color_2, 11	= color_3
							// These 2-bit codes correspond to the 2-bit fields
							// stored in the 64-bit block.
							colors[2].Blue = unchecked((byte)((2 * colors[0].Blue + colors[1].Blue + 1) / 3));
							colors[2].Green = unchecked((byte)((2 * colors[0].Green + colors[1].Green + 1) / 3));
							colors[2].Red = unchecked((byte)((2 * colors[0].Red + colors[1].Red + 1) / 3));

							colors[3].Blue = unchecked((byte)((colors[0].Blue + 2 * colors[1].Blue + 1) / 3));
							colors[3].Green = unchecked((byte)((colors[0].Green + 2 * colors[1].Green + 1) / 3));
							colors[3].Red = unchecked((byte)((colors[0].Red + 2 * colors[1].Red + 1) / 3));

							int bitIndex = 0;
							for (int ly = 0; ly < 4; ly++)
							{
								for (int lx = 0; lx < 4; lx++, bitIndex++)
								{
									int colorIndex = (int)((bitmask & (0x03 << bitIndex * 2)) >> bitIndex * 2);
									Color8888 col = colors[colorIndex];
									// only put pixels out < width or height
									if ((x + lx) < width && (y - ly) < height && (y - ly) >= 0)
									{
										// mirror Y
										long offset = z * sizeOfPlane + (y - ly) * bps + (x + lx) * bpp;
										destination.Position = position + offset;

										destination.WriteByte(col.Red);
										destination.WriteByte(col.Green);
										destination.WriteByte(col.Blue);
									}
								}
							}

							// 8-alpha or 6-alpha block?
							if (alphas[0] > alphas[1])
							{
								// 8-alpha block:  derive the other six alphas.
								// Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
								alphas[2] = unchecked((ushort)((6 * alphas[0] + 1 * alphas[1] + 3) / 7)); // bit code 010
								alphas[3] = unchecked((ushort)((5 * alphas[0] + 2 * alphas[1] + 3) / 7)); // bit code 011
								alphas[4] = unchecked((ushort)((4 * alphas[0] + 3 * alphas[1] + 3) / 7)); // bit code 100
								alphas[5] = unchecked((ushort)((3 * alphas[0] + 4 * alphas[1] + 3) / 7)); // bit code 101
								alphas[6] = unchecked((ushort)((2 * alphas[0] + 5 * alphas[1] + 3) / 7)); // bit code 110
								alphas[7] = unchecked((ushort)((1 * alphas[0] + 6 * alphas[1] + 3) / 7)); // bit code 111
							}
							else
							{
								// 6-alpha block.
								// Bit code 000 = alpha_0, 001 = alpha_1, others are interpolated.
								alphas[2] = unchecked((ushort)((4 * alphas[0] + 1 * alphas[1] + 2) / 5)); // Bit code 010
								alphas[3] = unchecked((ushort)((3 * alphas[0] + 2 * alphas[1] + 2) / 5)); // Bit code 011
								alphas[4] = unchecked((ushort)((2 * alphas[0] + 3 * alphas[1] + 2) / 5)); // Bit code 100
								alphas[5] = unchecked((ushort)((1 * alphas[0] + 4 * alphas[1] + 2) / 5)); // Bit code 101
								alphas[6] = 0x00; // Bit code 110
								alphas[7] = 0xFF; // Bit code 111
							}

							// Note: Have to separate the next two loops,
							// it operates on a 6-byte system.

							// First three bytes
							//uint bits = (uint)(alphamask[0]);
							uint bits = unchecked((uint)((alphaMask[0]) | (alphaMask[1] << 8) | (alphaMask[2] << 16)));
							for (int ly = 0; ly < 2; ly++)
							{
								for (int lx = 0; lx < 4; lx++, bits >>= 3)
								{
									// only put pixels out < width or height
									if ((x + lx) < width && (y - ly) < height && (y - ly) >= 0)
									{
										byte alphaValue = unchecked((byte)alphas[bits & 0x07]);
										// mirror Y
										long offset = z * sizeOfPlane + (y - ly) * bps + (x + lx) * bpp + 3;
										destination.Position = position + offset;
										destination.WriteByte(alphaValue);
									}
								}
							}

							// Last three bytes
							//bits = (uint)(alphamask[3]);
							bits = unchecked((uint)((alphaMask[3]) | (alphaMask[4] << 8) | (alphaMask[5] << 16)));
							for (int ly = 2; ly < 4; ly++)
							{
								for (int lx = 0; lx < 4; lx++, bits >>= 3)
								{
									// only put pixels out < width or height
									if ((x + lx) < width && (y - ly) < height && (y - ly) >= 0)
									{
										byte alphaValue = unchecked((byte)alphas[bits & 0x07]);
										// mirror Y
										long offset = z * sizeOfPlane + (y - ly) * bps + (x + lx) * bpp + 3;
										destination.Position = position + offset;
										destination.WriteByte(alphaValue);
									}
								}
							}
						}
					}
				}
			}
		}

		public static void DecompressRGB(Stream destination, Stream source, DDSConvertParameters @params)
		{
			DecompressRGBA(destination, source, @params, false);
		}

		public static void DecompressRGBA(Stream destination, Stream source, DDSConvertParameters @params)
		{
			DecompressRGBA(destination, source, @params, true);
		}

		private static void DecompressRGBA(Stream destination, Stream source, DDSConvertParameters @params, bool isAlpha)
		{
			int depth = @params.BitMapDepth;
			int width = @params.Width;
			int height = @params.Height;
			int bpp = GetRGBABytesPerPixel(@params);
			int bpc = GetBytesPerColor(@params);
			int bps = width * bpp * bpc;
			long sizeOfPlane = bps * height;
			
			int pixelSize = (@params.RGBBitCount + 7) / 8;
			ColorMask rMask = ComputeMaskParams(@params.RBitMask);
			ColorMask gMask = ComputeMaskParams(@params.GBitMask);
			ColorMask bMask = ComputeMaskParams(@params.BBitMask);
			ColorMask aMask = ComputeMaskParams(@params.ABitMask);

			long position = destination.Position;
			using (BinaryReader reader = new BinaryReader(source, Encoding.UTF8, true))
			{
				for(int z = 0; z < depth; z++)
				{
					// mirror Y
					for(int j = height - 1; j >= 0; j--)
					{
						long offset = z * sizeOfPlane + j * bps;
						destination.Position = position + offset;

						for (int i = 0; i < width; i++)
						{
							uint pixel = ReadPixel(reader, pixelSize);
							uint pixelColor = pixel & @params.RBitMask;
							byte red = unchecked((byte)(((pixelColor >> rMask.Shift1) * rMask.Mult) >> rMask.Shift2));
							pixelColor = pixel & @params.GBitMask;
							byte green = unchecked((byte)(((pixelColor >> gMask.Shift1) * gMask.Mult) >> gMask.Shift2));
							pixelColor = pixel & @params.BBitMask;
							byte blue = unchecked((byte)(((pixelColor >> bMask.Shift1) * bMask.Mult) >> bMask.Shift2));

							byte alpha = 0xFF;
							if (isAlpha)
							{
								pixelColor = pixel & @params.ABitMask;
								alpha = unchecked((byte)(((pixelColor >> aMask.Shift1) * aMask.Mult) >> aMask.Shift2));
							}

							destination.WriteByte(blue);
							destination.WriteByte(green);
							destination.WriteByte(red);
							destination.WriteByte(alpha);
						}
					}
				}
			}
		}

		private static int GetRGBABytesPerPixel(DDSConvertParameters @params)
		{
			if(@params.PixelFormatFlags.IsFourCC())
			{
				switch(@params.FourCC)
				{
					case DDSFourCCType.DXT1:
					case DDSFourCCType.DXT2:
					case DDSFourCCType.DXT3:
					case DDSFourCCType.DXT4:
					case DDSFourCCType.DXT5:
						return 4;
						
					default:
						throw new Exception($"Unsupported CCType {@params.FourCC}");
				}
			}
			else
			{
				return 4;
			}
		}

		private static int GetBytesPerPixel(DDSConvertParameters @params)
		{
			if (@params.PixelFormatFlags.IsFourCC())
			{
				switch (@params.FourCC)
				{
					case DDSFourCCType.DXT1:
					case DDSFourCCType.DXT2:
					case DDSFourCCType.DXT3:
					case DDSFourCCType.DXT4:
					case DDSFourCCType.DXT5:
						return 4;

					case DDSFourCCType.ATI1:
						return 1;

					case DDSFourCCType.ATI2:
					case DDSFourCCType.RXGB:
						return 3;

					case DDSFourCCType.oNULL:
						return 2;

					case DDSFourCCType.DollarNULL:
					case DDSFourCCType.qNULL:
					case DDSFourCCType.sNULL:
						return 8;

					case DDSFourCCType.tNULL:
						return 16;

					case DDSFourCCType.pNULL:
					case DDSFourCCType.rNULL:
						return 4;

					default:
						throw new Exception($"Unsupported CCType {@params.FourCC}");
				}
			}
			else
			{
				return @params.RGBBitCount / 8;
			}
		}

		private static int GetBytesPerColor(DDSConvertParameters @params)
		{
			if (@params.PixelFormatFlags.IsFourCC())
			{
				switch (@params.FourCC)
				{
					case DDSFourCCType.DXT1:
					case DDSFourCCType.DXT2:
					case DDSFourCCType.DXT3:
					case DDSFourCCType.DXT4:
					case DDSFourCCType.DXT5:
						return 1;
						
					case DDSFourCCType.ATI1:
					case DDSFourCCType.ATI2:
						return 1;

					case DDSFourCCType.RXGB:
						return 1;

					case DDSFourCCType.qNULL:
					case DDSFourCCType.oNULL:
					case DDSFourCCType.pNULL:
						return 4;

					case DDSFourCCType.rNULL:
					case DDSFourCCType.sNULL:
					case DDSFourCCType.tNULL:
						return 4;

					case DDSFourCCType.DollarNULL:
						return 2;

					default:
						throw new Exception($"Unsupported CCType {@params.FourCC}");
				}
			}
			else
			{
				return 1;
			}
		}

		private static Color8888 DxtcRead2bColor(BinaryReader reader)
		{
			byte data0 = reader.ReadByte();
			byte data1 = reader.ReadByte();

			byte r = unchecked((byte)(data0 & 0x1F));
			byte g = unchecked((byte)(((data0 & 0xE0) >> 5) | ((data1 & 0x7) << 3)));
			byte b = unchecked((byte)((data1 & 0xF8) >> 3));

			Color8888 op = default;
			op.Red = unchecked((byte)(r << 3 | r >> 2));
			op.Green = unchecked((byte)(g << 2 | g >> 3));
			op.Blue = unchecked((byte)(b << 3 | b >> 2));
			return op;
		}

		private static Color8888 DxtcRead3bColor(ushort data)
		{
			byte r = unchecked((byte)(data & 0x1F));
			byte g = unchecked((byte)((data & 0x7E0) >> 5));
			byte b = unchecked((byte)((data & 0xF800) >> 11));

			Color8888 op = default;
			op.Red = unchecked((byte)(r << 3 | r >> 2));
			op.Green = unchecked((byte)(g << 2 | g >> 3));
			op.Blue = unchecked((byte)(b << 3 | r >> 2));
			op.Alpha = 0xFF;
			return op;
		}

		private static uint ReadPixel(BinaryReader reader, int pixelSize)
		{
			switch(pixelSize)
			{
				case 1:
					return reader.ReadByte();
				case 2:
					return reader.ReadUInt16();
				case 3:
					uint value = reader.ReadUInt16();
					value |= unchecked((uint)(reader.ReadByte() << 16));
					return value;
				case 4:
					return reader.ReadUInt32();
				default:
					throw new ArgumentException($"Unsupported pixel size {pixelSize}", nameof(pixelSize));
			}
		}

		private static ColorMask ComputeMaskParams(uint mask)
		{
			ColorMask maskParams = new ColorMask()
			{
				Shift1 = 0,
				Shift2 = 0,
				Mult = 1,
			};
			if(mask == 0)
			{
				maskParams.Mult = 0;
				return maskParams;
			}

			while ((mask & 1) == 0)
			{
				mask >>= 1;
				maskParams.Shift1++;
			}
			int bc = 0;
			while ((mask & (1 << bc)) != 0)
			{
				bc++;
			}
			while ((mask * maskParams.Mult) < 255)
			{
				maskParams.Mult = (maskParams.Mult << bc) + 1;
			}
			mask *= unchecked((uint)maskParams.Mult);

			while ((mask & ~0xFF) != 0)
			{
				mask >>= 1;
				maskParams.Shift2++;
			}
			return maskParams;
		}
	}
}
