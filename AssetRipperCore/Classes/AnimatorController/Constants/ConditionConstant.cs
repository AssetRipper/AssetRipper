using AssetRipper.Core.Classes.AnimatorTransition;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;

namespace AssetRipper.Core.Classes.AnimatorController.Constants
{
	public sealed class ConditionConstant : IAssetReadable, IYAMLExportable
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
