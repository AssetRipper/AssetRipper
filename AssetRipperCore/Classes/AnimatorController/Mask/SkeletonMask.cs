using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.AnimatorController.Mask
{
	public sealed class SkeletonMask : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			Data = reader.ReadAssetArray<SkeletonMaskElement>();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public SkeletonMaskElement[] Data { get; set; }
	}
}
