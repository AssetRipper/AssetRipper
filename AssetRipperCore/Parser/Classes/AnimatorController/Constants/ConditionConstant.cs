using AssetRipper.Classes.AnimatorTransitions;
using AssetRipper.Converters;
using AssetRipper.YAML;
using System;

namespace AssetRipper.Classes.AnimatorControllers
{
	public struct ConditionConstant : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			ConditionMode = (AnimatorConditionMode)reader.ReadUInt32();
			EventID = reader.ReadUInt32();
			EventThreshold = reader.ReadSingle();
			ExitTime = reader.ReadSingle();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public AnimatorConditionMode ConditionMode { get; set; }
		public uint EventID { get; set; }
		public float EventThreshold { get; set; }
		public float ExitTime { get; set; }
	}
}
