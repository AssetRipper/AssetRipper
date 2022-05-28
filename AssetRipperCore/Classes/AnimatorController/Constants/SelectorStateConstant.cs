using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.AnimatorController.Constants
{
	public sealed class SelectorStateConstant : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			TransitionConstantArray = reader.ReadAssetArray<OffsetPtr<SelectorTransitionConstant>>();
			FullPathID = reader.ReadUInt32();
			IsEntry = reader.ReadBoolean();
			reader.AlignStream();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public OffsetPtr<SelectorTransitionConstant>[] TransitionConstantArray { get; set; }
		public uint FullPathID { get; set; }
		public bool IsEntry { get; set; }
	}
}
