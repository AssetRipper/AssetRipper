namespace uTinyRipper.Classes.Shaders
{
	public struct SerializedShaderDependency : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			From = reader.ReadString();
			To = reader.ReadString();
		}

		public string From { get; private set; }
		public string To { get; private set; }
	}
}
