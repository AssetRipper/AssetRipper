using AssetRipper.Core.Classes.AnimatorTransition;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.AnimatorController.Constants
{
	public sealed class ConditionConstant : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			ConditionMode = (AnimatorConditionMode)reader.ReadUInt32();
			EventID = reader.ReadUInt32();
			EventThreshold = reader.ReadSingle();
			ExitTime = reader.ReadSingle();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public AnimatorConditionMode ConditionMode { get; set; }
		public uint EventID { get; set; }
		public float EventThreshold { get; set; }
		public float ExitTime { get; set; }
	}
}
