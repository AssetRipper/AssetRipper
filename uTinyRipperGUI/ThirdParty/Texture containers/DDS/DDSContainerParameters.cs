namespace uTinyRipperGUI.TextureContainers.DDS
{
	public class DDSContainerParameters
	{
		public void CopyTo(DDSContainerParameters dest)
		{
			dest.DataLength = DataLength;
			dest.MipMapCount = MipMapCount;
			dest.Width = Width;
			dest.Height = Height;
			dest.IsPitchOrLinearSize = IsPitchOrLinearSize;
			dest.PixelFormatFlags = PixelFormatFlags;
			dest.FourCC = FourCC;
			dest.RGBBitCount = RGBBitCount;
			dest.RBitMask = RBitMask;
			dest.GBitMask = GBitMask;
			dest.BBitMask = BBitMask;
			dest.ABitMask = ABitMask;
			dest.Caps = Caps;
		}

		public long DataLength { get; set; }
		public int MipMapCount { get; set; }
		public DDSDFlags DFlags => DDSDFlags.CAPS | DDSDFlags.HEIGHT | DDSDFlags.WIDTH |
					DDSDFlags.PIXELFORMAT | (MipMapCount == 1 ? 0 : DDSDFlags.MIPMAPCOUNT);
		public int Width { get; set; }
		public int Height { get; set; }
		public int Depth => 0;
		public int BitMapDepth => Depth == 0 ? 1 : Depth;
		public bool IsPitchOrLinearSize { get; set; }
		public int PitchOrLinearSize => IsPitchOrLinearSize ? Height * Width / 2 : 0;
		public DDPFFlags PixelFormatFlags { get; set; }
		public DDSFourCCType FourCC { get; set; }
		public int RGBBitCount { get; set; }
		public uint RBitMask { get; set; }
		public uint GBitMask { get; set; }
		public uint BBitMask { get; set; }
		public uint ABitMask { get; set; }
		public DDSCapsFlags Caps { get; set; }
	}
}
