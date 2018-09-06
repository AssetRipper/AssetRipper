namespace UtinyRipper.Classes.Shaders
{
	public struct SerializedShaderDependency : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			From = reader.ReadStringAligned();
			To = reader.ReadStringAligned();
		}

		public string From { get; private set; }
		public string To { get; private set; }
	}
}
