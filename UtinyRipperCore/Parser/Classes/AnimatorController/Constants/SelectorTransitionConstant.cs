using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimatorControllers
{
	public struct SelectorTransitionConstant : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Destination = (int)reader.ReadUInt32();
			m_conditionConstantArray = reader.ReadArray<OffsetPtr<ConditionConstant>>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public int Destination { get; private set; }
		public IReadOnlyList<OffsetPtr<ConditionConstant>> ConditionConstantArray => m_conditionConstantArray;
		
		private OffsetPtr<ConditionConstant>[] m_conditionConstantArray;
	}
}
