using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.AssetBundle
{
	public sealed class AssetBundleScriptInfo : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			ClassName = reader.ReadString();
			NameSpace = reader.ReadString();
			AssemblyName = reader.ReadString();
			Hash = reader.ReadUInt32();
		}

		public string ClassName { get; set; }
		public string NameSpace { get; set; }
		public string AssemblyName { get; set; }
		public uint Hash { get; set; }
	}
}
