using System.IO;
using System.Text;
using uTinyRipper;

namespace uTinyRipperGUI.TextureContainers.PVR
{
	public static class PVRContainer
	{
		public static void ExportPVRHeader(Stream destination, PVRContainerParameters @params)
		{
			using (BinaryWriter binWriter = new BinaryWriter(destination, Encoding.UTF8, true))
			{
				binWriter.Write(Version);
				binWriter.Write((uint)Flags);
				binWriter.Write((ulong)@params.PixelFormat);
				binWriter.Write((uint)ColourSpace);
				binWriter.Write((uint)ChannelType);
				binWriter.Write(@params.Height);
				binWriter.Write(@params.Width);
				binWriter.Write(Depth);
				binWriter.Write(NumSurfaces);
				binWriter.Write(NumFaces);
				binWriter.Write(@params.MipMapCount);
				binWriter.Write(MetaDataSize);
			}
		}

		public static void ExportPVR(Stream destination, Stream source, PVRContainerParameters @params)
		{
			ExportPVRHeader(destination, @params);
			source.CopyStream(destination, @params.DataLength);
		}
		
		private const int Version = 0x03525650;
		private const PVRFlag Flags = PVRFlag.NoFlag;
		private const PVRColourSpace ColourSpace = PVRColourSpace.LinearRGB;
		private const PVRChannelType ChannelType = PVRChannelType.UnsignedByteNormalised;
		private const int Depth = 1;
		// For texture arrays
		private const int NumSurfaces = 1;
		// For cube maps
		private const int NumFaces = 1;
		private const int MetaDataSize = 0;
	}
}
