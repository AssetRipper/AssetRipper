namespace UtinyRipper.Classes
{
	public struct ColorRGBA32 : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			RGBA = stream.ReadUInt32();
		}
		
		public uint RGBA { get; private set; }
	}
}
