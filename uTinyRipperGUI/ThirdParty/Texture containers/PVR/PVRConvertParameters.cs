namespace uTinyRipperGUI.TextureContainers.PVR
{
	public struct PVRConvertParameters
	{
		public long DataLength { get; set; }
		public PVRPixelFormat PixelFormat { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int MipMapCount { get; set; }
	}
}
