using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.AnimatorControllers.Editor;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorControllers
{
	public struct SelectorTransitionConstant : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Destination = unchecked((int)reader.ReadUInt32());
			m_conditionConstantArray = reader.ReadAssetArray<OffsetPtr<ConditionConstant>>();
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
