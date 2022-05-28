using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.AnimatorController.Constants
{
	public sealed class SelectorTransitionConstant : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			Destination = unchecked((int)reader.ReadUInt32());
			ConditionConstantArray = reader.ReadAssetArray<OffsetPtr<ConditionConstant>>();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public int Destination { get; set; }
		public OffsetPtr<ConditionConstant>[] ConditionConstantArray { get; set; }
	}
}
