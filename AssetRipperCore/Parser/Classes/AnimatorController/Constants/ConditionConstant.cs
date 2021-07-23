using AssetRipper.Converters.Project;
using AssetRipper.Parser.Classes.AnimatorTransition;
using AssetRipper.IO.Asset;
using AssetRipper.YAML;
using System;

namespace AssetRipper.Parser.Classes.AnimatorController.Constants
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
