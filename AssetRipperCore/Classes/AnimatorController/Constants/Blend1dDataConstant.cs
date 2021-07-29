using AssetRipper.Project;
using AssetRipper.IO.Asset;
using AssetRipper.YAML;
using System;
using AssetRipper.IO;
using AssetRipper.IO.Extensions;

namespace AssetRipper.Classes.AnimatorController.Constants
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
