using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimatorControllers
{
	public struct SelectorTransitionConstant : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			Destination = stream.ReadUInt32();
			m_conditionConstantArray = stream.ReadArray<OffsetPtr<ConditionConstant>>();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			throw new NotSupportedException();
		}

		public uint Destination { get; private set; }
		public IReadOnlyList<OffsetPtr<ConditionConstant>> ConditionConstantArray => m_conditionConstantArray;
		
		private OffsetPtr<ConditionConstant>[] m_conditionConstantArray;
	}
}
