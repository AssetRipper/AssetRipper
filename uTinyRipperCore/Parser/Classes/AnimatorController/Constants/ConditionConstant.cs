using System;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorControllers
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

		public AnimatorConditionMode ConditionMode { get; private set; }
		public uint EventID { get; private set; }
		public float EventThreshold { get; private set; }
		public float ExitTime { get; private set; }
	}
}
