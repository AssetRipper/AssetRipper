namespace uTinyRipper.Converter.Textures.KTX
{
	public struct KTXConvertParameters
	{
		public long DataLength { get; set; }
		public KTXInternalFormat InternalFormat { get; set; }
		public KTXBaseInternalFormat BaseInternalFormat { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
	}
}
