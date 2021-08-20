using AssetRipper.Core.Project;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.YAML;
using System;
using AssetRipper.Core.IO;

namespace AssetRipper.Core.Classes.AnimatorController.Constants
{
	public struct Blend1dDataConstant : IAssetReadable, IYAMLExportable
	{
		public Blend1dDataConstant(ObjectReader reader)
		{
			m_ChildThresholdArray = reader.ReadSingleArray();
		}

		public void Read(AssetReader reader)
		{
			m_ChildThresholdArray = reader.ReadSingleArray();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public float[] m_ChildThresholdArray { get; set; }
	}
}
