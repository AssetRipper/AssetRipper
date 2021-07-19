using AssetRipper.Classes.Misc;
using AssetRipper.Converters;
using AssetRipper.YAML;
using System;

namespace AssetRipper.Classes.AnimatorControllers
{
	public struct SelectorStateConstant : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			TransitionConstantArray = reader.ReadAssetArray<OffsetPtr<SelectorTransitionConstant>>();
			FullPathID = reader.ReadUInt32();
			IsEntry = reader.ReadBoolean();
			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public OffsetPtr<SelectorTransitionConstant>[] TransitionConstantArray { get; set; }
		public uint FullPathID { get; set; }
		public bool IsEntry { get; set; }
	}
}
