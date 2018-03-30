using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace UtinyRipper.Converter.Textures.DDS
{
	public static class DDSConverter
	{
		public static void ExportDDSHeader(byte[] buffer, int offset, DDSConvertParameters @params)
		{
			using (MemoryStream stream = new MemoryStream(buffer))
			{
				stream.Position = offset;
				ExportDDSHeader(stream, @params);
			}
		}

		public static void ExportDDSHeader(Stream destination, DDSConvertParameters @params)
		{
			using (BinaryWriter binWriter = new BinaryWriter(destination, Encoding.Default, true))
			{
				binWriter.Write(MagicNumber);
				binWriter.Write(HeaderSize);
				binWriter.Write((uint)@params.DFlags);
				binWriter.Write(@params.Height);
				binWriter.Write(@params.Width);
				binWriter.Write(@params.PitchOrLinearSize);
				binWriter.Write(@params.Depth);
				binWriter.Write(@params.MipMapCount);
				// read alphabitdepth?
				for (int i = 0; i < 11; i++) // reserved
				{
					binWriter.Write(0);
				}
				DDSPixelFormat pixelFormat = new DDSPixelFormat()
				{
					Flags = @params.PixelFormatFlags,
					FourCC = @params.FourCC,
					RGBBitCount = @params.RGBBitCount,
					RBitMask = @params.RBitMask,
					GBitMask = @params.GBitMask,
					BBitMask = @params.BBitMask,
					ABitMask = @params.ABitMask,
				};
				pixelFormat.Write(binWriter);
				binWriter.Write((uint)@params.Caps);
				binWriter.Write((uint)Caps2);
				binWriter.Write(0); // caps3
				binWriter.Write(0); // caps4
				binWriter.Write(0); // reserved (texturestage?)
			}
		}

		public static void ExportDDS(byte[] buffer, int offset, Stream source, DDSConvertParameters @params)
		{
			using (MemoryStream stream = new MemoryStream(buffer))
			{
				stream.Position = offset;
				ExportDDS(stream, source, @params);
			}
		}

		public static void ExportDDS(Stream destination, Stream source, DDSConvertParameters @params)
		{
			if(IsRGBA32(@params))
			{
				ExportRGBA32ToDDS(destination, source, @params);
			}
			else if(IsARGB32(@params))
			{
				ExportARGB32ToDDS(destination, source, @params);
			}
			else if(IsRGBA16(@params))
			{
				ExportRGBA16ToDDS(destination, source, @params);
			}
			else if(IsAlpha8(@params))
			{
				ExportAlpha8ToDDS(destination, source, @params);
			}
			else if(IsR8(@params))
			{
				ExportR8ToDDS(destination, source, @params);
			}
			else if (IsR16(@params))
			{
				ExportR16ToDDS(destination, source, @params);
			}
			else if (IsRG16(@params))
			{
				ExportRG16ToDDS(destination, source, @params);
			}
			else
			{
				ExportDDSHeader(destination, @params);
				source.CopyStream(destination, @params.DataLength);
			}
		}

		public static void ExportBitmap(byte[] buffer, int offset, Stream source, DDSConvertParameters @params)
		{
			using (MemoryStream stream = new MemoryStream(buffer))
			{
				stream.Position = offset;
				ExportBitmap(stream, source, @params);
			}
		}

		public static void ExportBitmap(Stream destination, Stream source, DDSConvertParameters @params)
		{
			int width = @params.Width;
			int height = @params.Height;
			int size = width * height * 4;
			byte[] buffer = new byte[size];
			using (MemoryStream memStream = new MemoryStream(buffer))
			{
				ExportBitmapData(memStream, source, @params);
				Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
				Rectangle rect = new Rectangle(0, 0, width, height);
				BitmapData bitData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
				IntPtr pointer = bitData.Scan0;
				Marshal.Copy(buffer, 0, pointer, size);
				bitmap.UnlockBits(bitData);

				bitmap.Save(destination, ImageFormat.Png);
			}
		}

		private static void ExportBitmapData(Stream destination, Stream source, DDSConvertParameters @params)
		{
			if(@params.PixelFormatFlags.IsFourCC())
			{
				switch(@params.FourCC)
				{
					case DDSFourCCType.DXT1:
						DDSDecompressor.DecompressDXT1(destination, source, @params);
						break;
						
					case DDSFourCCType.DXT5:
						DDSDecompressor.DecompressDXT5(destination, source, @params);
						break;

					default:
						throw new NotImplementedException(@params.FourCC.ToString());
				}
			}
			else
			{
				if (@params.PixelFormatFlags.IsLuminace())
				{
					throw new NotSupportedException("Luminace isn't supported");
				}
				else
				{
					if (@params.PixelFormatFlags.IsAlphaPixels())
					{
						DDSDecompressor.DecompressRGBA(destination, source, @params);
					}
					else
					{
						DDSDecompressor.DecompressRGB(destination, source, @params);
					}
				}
			}
		}

		private static void ExportRGBA32ToDDS(Stream destination, Stream source, DDSConvertParameters @params)
		{
			DDSConvertParameters bgraParams = new DDSConvertParameters();
			@params.CopyTo(bgraParams);
			bgraParams.RBitMask = 0xFF0000;
			bgraParams.BBitMask = 0xFF;
			ExportDDSHeader(destination, bgraParams);

			using (BinaryReader binReader = new BinaryReader(source, Encoding.Default, true))
			{
				long pixelCount = @params.BitMapDepth * @params.Height * @params.Width;
				for (int i = 0; i < pixelCount; i++)
				{
					byte R = binReader.ReadByte();
					byte G = binReader.ReadByte();
					byte B = binReader.ReadByte();
					byte A = binReader.ReadByte();
					destination.WriteByte(B);     // B
					destination.WriteByte(G);     // G
					destination.WriteByte(R);     // R
					destination.WriteByte(A);     // A
				}
			}
		}

		private static void ExportARGB32ToDDS(Stream destination, Stream source, DDSConvertParameters @params)
		{
			DDSConvertParameters bgraParams = new DDSConvertParameters();
			@params.CopyTo(bgraParams);
			bgraParams.RBitMask = 0xFF0000;
			bgraParams.GBitMask = 0xFF00;
			bgraParams.BBitMask = 0xFF;
			bgraParams.ABitMask = 0xFF000000;
			ExportDDSHeader(destination, bgraParams);

			using (BinaryReader binReader = new BinaryReader(source, Encoding.Default, true))
			{
				long pixelCount = @params.BitMapDepth * @params.Height * @params.Width;
				for (int i = 0; i < pixelCount; i++)
				{
					byte A = binReader.ReadByte();
					byte R = binReader.ReadByte();
					byte G = binReader.ReadByte();
					byte B = binReader.ReadByte();
					destination.WriteByte(B);     // B
					destination.WriteByte(G);     // G
					destination.WriteByte(R);     // R
					destination.WriteByte(A);     // A
				}
			}
		}

		private static void ExportRGBA16ToDDS(Stream destination, Stream source, DDSConvertParameters @params)
		{
			DDSConvertParameters bgraParams = new DDSConvertParameters();
			@params.CopyTo(bgraParams);
			bgraParams.RBitMask = 0xF00;
			bgraParams.BBitMask = 0xF;
			ExportDDSHeader(destination, bgraParams);

			using (BinaryReader binReader = new BinaryReader(source, Encoding.Default, true))
			{
				long pixelCount = @params.BitMapDepth * @params.Height * @params.Width;
				for (int i = 0; i < pixelCount; i++)
				{
					int pixel = binReader.ReadUInt16();
					int c1 = (0x00F0 & pixel) >> 4;     // B
					int c2 = (0x0F00 & pixel) >> 4;     // G
					destination.WriteByte((byte)(c1 | c2));
					c1 = (0xF000 & pixel) >> 12;        // R
					c2 = (0x000F & pixel) << 4;         // A
					destination.WriteByte((byte)(c1 | c2));
				}
			}
		}

		private static void ExportAlpha8ToDDS(Stream destination, Stream source, DDSConvertParameters @params)
		{
			DDSConvertParameters bgraParams = new DDSConvertParameters();
			@params.CopyTo(bgraParams);
			bgraParams.RGBBitCount = 32;
			bgraParams.RBitMask = 0xFF0000;
			bgraParams.GBitMask = 0xFF00;
			bgraParams.BBitMask = 0xFF;
			bgraParams.ABitMask = 0xFF000000;
			ExportDDSHeader(destination, bgraParams);

			using (BinaryReader binReader = new BinaryReader(source, Encoding.Default, true))
			{
				long pixelCount = @params.BitMapDepth * @params.Height * @params.Width;
				for (int i = 0; i < pixelCount; i++)
				{
					byte A = binReader.ReadByte();
					destination.WriteByte(0xFF);    // B
					destination.WriteByte(0xFF);    // G
					destination.WriteByte(0xFF);    // R
					destination.WriteByte(A);       // A
				}
			}
		}

		private static void ExportR8ToDDS(Stream destination, Stream source, DDSConvertParameters @params)
		{
			DDSConvertParameters bgraParams = new DDSConvertParameters();
			@params.CopyTo(bgraParams);
			bgraParams.RGBBitCount = 32;
			bgraParams.RBitMask = 0xFF0000;
			bgraParams.GBitMask = 0xFF00;
			bgraParams.BBitMask = 0xFF;
			bgraParams.ABitMask = 0xFF000000;
			ExportDDSHeader(destination, bgraParams);

			using (BinaryReader binReader = new BinaryReader(source, Encoding.Default, true))
			{
				long pixelCount = @params.BitMapDepth * @params.Height * @params.Width;
				for (int i = 0; i < pixelCount; i++)
				{
					byte R = binReader.ReadByte();
					destination.WriteByte(0);	    // B
					destination.WriteByte(0);		// G
					destination.WriteByte(R);		// R
					destination.WriteByte(0xFF);	// A
				}
			}
		}

		private static void ExportR16ToDDS(Stream destination, Stream source, DDSConvertParameters @params)
		{
			DDSConvertParameters bgraParams = new DDSConvertParameters();
			@params.CopyTo(bgraParams);
			bgraParams.RGBBitCount = 32;
			bgraParams.RBitMask = 0xFF0000;
			bgraParams.GBitMask = 0xFF00;
			bgraParams.BBitMask = 0xFF;
			bgraParams.ABitMask = 0xFF000000;
			ExportDDSHeader(destination, bgraParams);

			using (BinaryReader binReader = new BinaryReader(source, Encoding.Default, true))
			{
				long pixelCount = @params.BitMapDepth * @params.Height * @params.Width;
				for (int i = 0; i < pixelCount; i++)
				{
					ushort pixel = binReader.ReadUInt16();
					float f = Half.ToHalf(pixel);
					byte R = (byte)Math.Ceiling(f * 255.0);
					destination.WriteByte(0);       // B
					destination.WriteByte(0);       // G
					destination.WriteByte(R);       // R
					destination.WriteByte(0xFF);    // A
				}
			}
		}

		private static void ExportRG16ToDDS(Stream destination, Stream source, DDSConvertParameters @params)
		{
			DDSConvertParameters bgraParams = new DDSConvertParameters();
			@params.CopyTo(bgraParams);
			bgraParams.RGBBitCount = 32;
			bgraParams.RBitMask = 0xFF0000;
			bgraParams.GBitMask = 0xFF00;
			bgraParams.BBitMask = 0xFF;
			bgraParams.ABitMask = 0xFF000000;
			ExportDDSHeader(destination, bgraParams);

			using (BinaryReader binReader = new BinaryReader(source, Encoding.Default, true))
			{
				long pixelCount = @params.BitMapDepth * @params.Height * @params.Width;
				for (int i = 0; i < pixelCount; i++)
				{
					byte R = binReader.ReadByte();
					byte G = binReader.ReadByte();
					destination.WriteByte(0);		// B
					destination.WriteByte(G);		// G
					destination.WriteByte(R);		// R
					destination.WriteByte(0xFF);	// A
				}
			}
		}

		private static bool IsRGBA32(DDSConvertParameters @params)
		{
			return @params.RBitMask == 0xFF && @params.GBitMask == 0xFF00 && @params.BBitMask == 0xFF0000 && @params.ABitMask == 0xFF000000;
		}

		private static bool IsARGB32(DDSConvertParameters @params)
		{
			return @params.RBitMask == 0xFF00 && @params.GBitMask == 0xFF0000 && @params.BBitMask == 0xFF000000 && @params.ABitMask == 0xFF;
		}

		private static bool IsRGBA16(DDSConvertParameters @params)
		{
			return @params.RBitMask == 0xF000 && @params.GBitMask == 0xF00 && @params.BBitMask == 0xF0 && @params.ABitMask == 0xF;
		}

		private static bool IsAlpha8(DDSConvertParameters @params)
		{
			return @params.RBitMask == 0 && @params.GBitMask == 0 && @params.BBitMask == 0 && @params.ABitMask == 0xFF;
		}

		private static bool IsR8(DDSConvertParameters @params)
		{
			return @params.RBitMask == 0xFF && @params.GBitMask == 0 && @params.BBitMask == 0 && @params.ABitMask == 0;
		}

		private static bool IsR16(DDSConvertParameters @params)
		{
			return @params.RBitMask == 0xFFFF && @params.GBitMask == 0 && @params.BBitMask == 0 && @params.ABitMask == 0;
		}

		private static bool IsRG16(DDSConvertParameters @params)
		{
			return @params.RBitMask == 0xFF && @params.GBitMask == 0xFF00 && @params.BBitMask == 0 && @params.ABitMask == 0;
		}

		/// <summary>
		/// ASCII 'DDS '
		/// </summary>
		private const uint MagicNumber = 0x20534444;
		private const uint HeaderSize = 0x7C;
		
		private const DDSCaps2Flags Caps2 = 0;
	}
}
