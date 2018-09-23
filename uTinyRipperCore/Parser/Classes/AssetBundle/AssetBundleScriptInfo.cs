namespace uTinyRipper.Classes.AssetBundles
{
	public struct AssetBundleScriptInfo : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			ClassName = reader.ReadString();
			NameSpace = reader.ReadString();
			AssemblyName = reader.ReadString();
			Hash = reader.ReadUInt32();
		}

		public string ClassName { get; private set; }
		public string NameSpace { get; private set; }
		public string AssemblyName { get; private set; }
		public uint Hash { get; private set; }
	}
}
