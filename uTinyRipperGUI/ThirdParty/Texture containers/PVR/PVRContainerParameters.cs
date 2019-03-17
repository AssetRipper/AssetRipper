namespace uTinyRipperGUI.TextureContainers.PVR
{
	public struct PVRContainerParameters
	{
		public long DataLength { get; set; }
		public PVRPixelFormat PixelFormat { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int MipMapCount { get; set; }
	}
}
