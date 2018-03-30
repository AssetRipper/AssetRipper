namespace UtinyRipper.Classes.Shaders
{
	public struct SerializedShaderDependency : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			From = stream.ReadStringAligned();
			To = stream.ReadStringAligned();
		}

		public string From { get; private set; }
		public string To { get; private set; }
	}
}
