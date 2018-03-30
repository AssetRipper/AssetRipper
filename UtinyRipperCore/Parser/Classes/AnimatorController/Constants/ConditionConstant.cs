using System;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimatorControllers
{
	public struct ConditionConstant : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			ConditionMode = stream.ReadUInt32();
			EventID = stream.ReadUInt32();
			EventThreshold = stream.ReadSingle();
			ExitTime = stream.ReadSingle();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			throw new NotSupportedException();
		}

		public uint ConditionMode { get; private set; }
		public uint EventID { get; private set; }
		public float EventThreshold { get; private set; }
		public float ExitTime { get; private set; }
	}
}
