namespace uTinyRipper.Classes.Shaders
{
	public struct SerializedShaderDependency : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			From = reader.ReadString();
			To = reader.ReadString();
		}

		public string From { get; set; }
		public string To { get; set; }
	}
}
