namespace uTinyRipperGUI.TextureContainers.KTX
{
	public struct KTXContainerParameters
	{
		public long DataLength { get; set; }
		public KTXInternalFormat InternalFormat { get; set; }
		public KTXBaseInternalFormat BaseInternalFormat { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
	}
}
