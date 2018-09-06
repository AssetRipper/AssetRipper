namespace UtinyRipper.Classes.AssetBundles
{
	public struct AssetBundleScriptInfo : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			ClassName = reader.ReadStringAligned();
			NameSpace = reader.ReadStringAligned();
			AssemblyName = reader.ReadStringAligned();
			Hash = reader.ReadUInt32();
		}

		public string ClassName { get; private set; }
		public string NameSpace { get; private set; }
		public string AssemblyName { get; private set; }
		public uint Hash { get; private set; }
	}
}
