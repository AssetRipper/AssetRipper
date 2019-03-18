using System;
using System.IO;
using System.Text;

namespace uTinyRipperGUI.TextureContainers.DDS
{
	public static class DDSContainer
	{
		public static void ExportDDSHeader(byte[] buffer, int offset, DDSContainerParameters @params)
		{
			using (MemoryStream stream = new MemoryStream(buffer))
			{
				stream.Position = offset;
				ExportDDSHeader(stream, @params);
			}
		}

		public static void ExportDDSHeader(Stream destination, DDSContainerParameters @params)
		{
			using (BinaryWriter binWriter = new BinaryWriter(destination, Encoding.UTF8, true))
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

		public static void ExportDDS(Stream source, Stream destination, DDSContainerParameters @params)
		{
			using (BinaryReader sourceReader = new BinaryReader(source))
			{
				ExportDDS(sourceReader, destination, @params);
			}
		}

		public static void ExportDDS(BinaryReader sourceReader, Stream destination, DDSContainerParameters @params)
		{
			if(IsRGBA32(@params))
			{
				ExportRGBA32ToDDS(sourceReader, destination, @params);
			}
			else if(IsARGB32(@params))
			{
				ExportARGB32ToDDS(sourceReader, destination, @params);
			}
			else if(IsRGBA16(@params))
			{
				ExportRGBA16ToDDS(sourceReader, destination, @params);
			}
			else if(IsAlpha8(@params))
			{
				ExportAlpha8ToDDS(sourceReader, destination, @params);
			}
			else if(IsR8(@params))
			{
				ExportR8ToDDS(sourceReader, destination, @params);
			}
			else if (IsR16(@params))
			{
				ExportR16ToDDS(sourceReader, destination, @params);
			}
			else if (IsRG16(@params))
			{
				ExportRG16ToDDS(sourceReader, destination, @params);
			}
			else
			{
				ExportDDSHeader(destination, @params);
				for (int i = 0; i < @params.DataLength; i += 2)
				{
					ushort value = sourceReader.ReadUInt16();
					byte value0 = unchecked((byte)(value >> 0));
					byte value1 = unchecked((byte)(value >> 8));
					destination.WriteByte(value0);
					destination.WriteByte(value1);
				}
			}
		}

		private static void ExportRGBA32ToDDS(BinaryReader sourceReader, Stream destination, DDSContainerParameters @params)
		{
			DDSContainerParameters bgraParams = new DDSContainerParameters();
			@params.CopyTo(bgraParams);
			bgraParams.RBitMask = 0xFF0000;
			bgraParams.BBitMask = 0xFF;
			ExportDDSHeader(destination, bgraParams);

			long pixelCount = @params.BitMapDepth * @params.Height * @params.Width;
			for (int i = 0; i < pixelCount; i++)
			{
				byte R = sourceReader.ReadByte();
				byte G = sourceReader.ReadByte();
				byte B = sourceReader.ReadByte();
				byte A = sourceReader.ReadByte();
				destination.WriteByte(B);     // B
				destination.WriteByte(G);     // G
				destination.WriteByte(R);     // R
				destination.WriteByte(A);     // A
			}
		}

		private static void ExportARGB32ToDDS(BinaryReader sourceReader, Stream destination, DDSContainerParameters @params)
		{
			DDSContainerParameters bgraParams = new DDSContainerParameters();
			@params.CopyTo(bgraParams);
			bgraParams.RBitMask = 0xFF0000;
			bgraParams.GBitMask = 0xFF00;
			bgraParams.BBitMask = 0xFF;
			bgraParams.ABitMask = 0xFF000000;
			ExportDDSHeader(destination, bgraParams);

			long pixelCount = @params.BitMapDepth * @params.Height * @params.Width;
			for (int i = 0; i < pixelCount; i++)
			{
				byte A = sourceReader.ReadByte();
				byte R = sourceReader.ReadByte();
				byte G = sourceReader.ReadByte();
				byte B = sourceReader.ReadByte();
				destination.WriteByte(B);     // B
				destination.WriteByte(G);     // G
				destination.WriteByte(R);     // R
				destination.WriteByte(A);     // A
			}
		}

		private static void ExportRGBA16ToDDS(BinaryReader sourceReader, Stream destination, DDSContainerParameters @params)
		{
			DDSContainerParameters bgraParams = new DDSContainerParameters();
			@params.CopyTo(bgraParams);
			bgraParams.RBitMask = 0xF00;
			bgraParams.BBitMask = 0xF;
			ExportDDSHeader(destination, bgraParams);

			long pixelCount = @params.BitMapDepth * @params.Height * @params.Width;
			for (int i = 0; i < pixelCount; i++)
			{
				int pixel = sourceReader.ReadUInt16();
				int c1 = (0x00F0 & pixel) >> 4;     // B
				int c2 = (0x0F00 & pixel) >> 4;     // G
				destination.WriteByte((byte)(c1 | c2));
				c1 = (0xF000 & pixel) >> 12;        // R
				c2 = (0x000F & pixel) << 4;         // A
				destination.WriteByte((byte)(c1 | c2));
			}
		}

		private static void ExportAlpha8ToDDS(BinaryReader sourceReader, Stream destination, DDSContainerParameters @params)
		{
			DDSContainerParameters bgraParams = new DDSContainerParameters();
			@params.CopyTo(bgraParams);
			bgraParams.RGBBitCount = 32;
			bgraParams.RBitMask = 0xFF0000;
			bgraParams.GBitMask = 0xFF00;
			bgraParams.BBitMask = 0xFF;
			bgraParams.ABitMask = 0xFF000000;
			ExportDDSHeader(destination, bgraParams);

			long pixelCount = @params.BitMapDepth * @params.Height * @params.Width;
			for (int i = 0; i < pixelCount; i++)
			{
				byte A = sourceReader.ReadByte();
				destination.WriteByte(0xFF);    // B
				destination.WriteByte(0xFF);    // G
				destination.WriteByte(0xFF);    // R
				destination.WriteByte(A);       // A
			}
		}

		private static void ExportR8ToDDS(BinaryReader sourceReader, Stream destination, DDSContainerParameters @params)
		{
			DDSContainerParameters bgraParams = new DDSContainerParameters();
			@params.CopyTo(bgraParams);
			bgraParams.RGBBitCount = 32;
			bgraParams.RBitMask = 0xFF0000;
			bgraParams.GBitMask = 0xFF00;
			bgraParams.BBitMask = 0xFF;
			bgraParams.ABitMask = 0xFF000000;
			ExportDDSHeader(destination, bgraParams);

			long pixelCount = @params.BitMapDepth * @params.Height * @params.Width;
			for (int i = 0; i < pixelCount; i++)
			{
				byte R = sourceReader.ReadByte();
				destination.WriteByte(0);       // B
				destination.WriteByte(0);       // G
				destination.WriteByte(R);       // R
				destination.WriteByte(0xFF);    // A
			}
		}

		private static void ExportR16ToDDS(BinaryReader sourceReader, Stream destination, DDSContainerParameters @params)
		{
			DDSContainerParameters bgraParams = new DDSContainerParameters();
			@params.CopyTo(bgraParams);
			bgraParams.RGBBitCount = 32;
			bgraParams.RBitMask = 0xFF0000;
			bgraParams.GBitMask = 0xFF00;
			bgraParams.BBitMask = 0xFF;
			bgraParams.ABitMask = 0xFF000000;
			ExportDDSHeader(destination, bgraParams);

			long pixelCount = @params.BitMapDepth * @params.Height * @params.Width;
			for (int i = 0; i < pixelCount; i++)
			{
				ushort pixel = sourceReader.ReadUInt16();
				float f = Half.ToHalf(pixel);
				byte R = (byte)Math.Ceiling(f * 255.0);
				destination.WriteByte(0);       // B
				destination.WriteByte(0);       // G
				destination.WriteByte(R);       // R
				destination.WriteByte(0xFF);    // A
			}
		}

		private static void ExportRG16ToDDS(BinaryReader sourceReader, Stream destination, DDSContainerParameters @params)
		{
			DDSContainerParameters bgraParams = new DDSContainerParameters();
			@params.CopyTo(bgraParams);
			bgraParams.RGBBitCount = 32;
			bgraParams.RBitMask = 0xFF0000;
			bgraParams.GBitMask = 0xFF00;
			bgraParams.BBitMask = 0xFF;
			bgraParams.ABitMask = 0xFF000000;
			ExportDDSHeader(destination, bgraParams);

			long pixelCount = @params.BitMapDepth * @params.Height * @params.Width;
			for (int i = 0; i < pixelCount; i++)
			{
				byte R = sourceReader.ReadByte();
				byte G = sourceReader.ReadByte();
				destination.WriteByte(0);       // B
				destination.WriteByte(G);       // G
				destination.WriteByte(R);       // R
				destination.WriteByte(0xFF);    // A
			}
		}

		private static bool IsRGBA32(DDSContainerParameters @params)
		{
			return @params.RBitMask == 0xFF && @params.GBitMask == 0xFF00 && @params.BBitMask == 0xFF0000 && @params.ABitMask == 0xFF000000;
		}

		private static bool IsARGB32(DDSContainerParameters @params)
		{
			return @params.RBitMask == 0xFF00 && @params.GBitMask == 0xFF0000 && @params.BBitMask == 0xFF000000 && @params.ABitMask == 0xFF;
		}

		private static bool IsRGBA16(DDSContainerParameters @params)
		{
			return @params.RBitMask == 0xF000 && @params.GBitMask == 0xF00 && @params.BBitMask == 0xF0 && @params.ABitMask == 0xF;
		}

		private static bool IsAlpha8(DDSContainerParameters @params)
		{
			return @params.RBitMask == 0 && @params.GBitMask == 0 && @params.BBitMask == 0 && @params.ABitMask == 0xFF;
		}

		private static bool IsR8(DDSContainerParameters @params)
		{
			return @params.RBitMask == 0xFF && @params.GBitMask == 0 && @params.BBitMask == 0 && @params.ABitMask == 0;
		}

		private static bool IsR16(DDSContainerParameters @params)
		{
			return @params.RBitMask == 0xFFFF && @params.GBitMask == 0 && @params.BBitMask == 0 && @params.ABitMask == 0;
		}

		private static bool IsRG16(DDSContainerParameters @params)
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
