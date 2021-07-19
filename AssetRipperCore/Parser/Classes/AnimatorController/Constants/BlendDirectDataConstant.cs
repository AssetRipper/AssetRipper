using AssetRipper.Converters;
using AssetRipper.YAML;
using System;

namespace AssetRipper.Classes.AnimatorControllers
{
	public struct BlendDirectDataConstant : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			ChildBlendEventIDArray = reader.ReadUInt32Array();
			NormalizedBlendValues = reader.ReadBoolean();
			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public uint[] ChildBlendEventIDArray { get; set; }
		public bool NormalizedBlendValues { get; set; }
	}
}
