using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;

namespace AssetRipper.Core.Classes.AnimatorController.Constants
{
	public sealed class SelectorTransitionConstant : IAssetReadable, IYAMLExportable
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
