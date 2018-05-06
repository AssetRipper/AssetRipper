using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimatorControllers
{
	public struct SelectorStateConstant : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			m_transitionConstantArray = stream.ReadArray<OffsetPtr<SelectorTransitionConstant>>();
			FullPathID = stream.ReadUInt32();
			IsEntry = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public IReadOnlyList<OffsetPtr<SelectorTransitionConstant>> TransitionConstantArray => m_transitionConstantArray;
		public uint FullPathID { get; private set; }
		public bool IsEntry { get; private set; }

		private OffsetPtr<SelectorTransitionConstant>[] m_transitionConstantArray;
	}
}
