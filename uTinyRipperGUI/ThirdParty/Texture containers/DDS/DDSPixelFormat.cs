using System.IO;

namespace uTinyRipperGUI.TextureContainers.DDS
{
	public struct DDSPixelFormat
	{
		public void Write(BinaryWriter writer)
		{
			writer.Write(StructSize);
			writer.Write((uint)Flags);
			writer.Write((uint)FourCC);
			writer.Write((uint)RGBBitCount);
			writer.Write(RBitMask);
			writer.Write(GBitMask);
			writer.Write(BBitMask);
			writer.Write(ABitMask);
		}
		
		public DDPFFlags Flags { get; set; }
		public DDSFourCCType FourCC { get; set; }
		public int RGBBitCount { get; set; }
		public uint RBitMask { get; set; }
		public uint GBitMask { get; set; }
		public uint BBitMask { get; set; }
		public uint ABitMask { get; set; }

		private const uint StructSize = 0x20;
	}
}
