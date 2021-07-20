using AssetRipper.Converters.Project;
using AssetRipper.Parser.IO.Asset;
using AssetRipper.Parser.IO.Asset.Reader;
using AssetRipper.YAML;
using System;

namespace AssetRipper.Parser.Classes.AnimatorController.Constants
{
	public struct Blend1dDataConstant : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			ChildThresholdArray = reader.ReadSingleArray();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public float[] ChildThresholdArray { get; set; }
	}
}
