using System.IO;
using System.Text;
using uTinyRipper;

namespace uTinyRipperGUI.TextureContainers.KTX
{
	public static class KTXContainer
	{
		public static void ExportKXTHeader(Stream destination, KTXContainerParameters @params)
		{
			using (BinaryWriter binWriter = new BinaryWriter(destination, Encoding.UTF8, true))
			{
				binWriter.Write(Identifier);
				binWriter.Write(EndianessLE);
				binWriter.Write((uint)Type);
				binWriter.Write(TypeSize);
				binWriter.Write((uint)Format);
				binWriter.Write((uint)@params.InternalFormat);
				binWriter.Write((uint)@params.BaseInternalFormat);
				binWriter.Write(@params.Width);
				binWriter.Write(@params.Height);
				binWriter.Write(PixelDepth);
				binWriter.Write(NumberOfArrayElements);
				binWriter.Write(NumberOfFaces);
				binWriter.Write(NumberOfMipmapLevels);
				binWriter.Write(BytesOfKeyValueData);
				binWriter.Write(@params.DataLength);
			}
		}

		public static void ExportKXT(Stream destination, Stream source, KTXContainerParameters @params)
		{
			ExportKXTHeader(destination, @params);
			source.CopyStream(destination, @params.DataLength);
		}

		private const int HeaderSize = 68;


		private static readonly byte[] Identifier = { 0xAB, 0x4B, 0x54, 0x58, 0x20, 0x31, 0x31, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A };
		private const uint EndianessLE = 0x04030201;
		private const uint EndianessBE = 0x01020304;

		// for compressed - 0
		private const KTXType Type = 0;
		private const int TypeSize = 1;
		// for compressed - 0
		private const KTXFormat Format = 0;
		private const int PixelDepth = 0;
		private const int NumberOfArrayElements = 0;
		// number of cubemap faces
		private const int NumberOfFaces = 1;
		private const int NumberOfMipmapLevels = 1;
		private const int BytesOfKeyValueData = 0;

	}
}
