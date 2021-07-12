using System;
using AssetRipper.Converters;
using AssetRipper.YAML;

namespace AssetRipper.Classes.AnimatorControllers
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
