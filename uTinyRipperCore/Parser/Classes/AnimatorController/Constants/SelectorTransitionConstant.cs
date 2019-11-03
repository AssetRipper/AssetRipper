using System;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorControllers
{
	public struct SelectorTransitionConstant : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Destination = unchecked((int)reader.ReadUInt32());
			ConditionConstantArray = reader.ReadAssetArray<OffsetPtr<ConditionConstant>>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public int Destination { get; set; }
		public OffsetPtr<ConditionConstant>[] ConditionConstantArray { get; set; }
	}
}
