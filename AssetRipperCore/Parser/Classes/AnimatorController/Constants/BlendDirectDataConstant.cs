using AssetRipper.Converters.Project;
using AssetRipper.Parser.IO.Asset;
using AssetRipper.Parser.IO.Asset.Reader;
using AssetRipper.YAML;
using System;

namespace AssetRipper.Parser.Classes.AnimatorController.Constants
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
