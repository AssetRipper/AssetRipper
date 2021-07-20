using AssetRipper.Converters.Project;
using AssetRipper.Parser.IO.Asset;
using AssetRipper.Parser.IO.Asset.Reader;
using AssetRipper.YAML;
using System;

namespace AssetRipper.Parser.Classes.AnimatorController.Mask
{
	public struct SkeletonMask : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Data = reader.ReadAssetArray<SkeletonMaskElement>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public SkeletonMaskElement[] Data { get; set; }
	}
}
