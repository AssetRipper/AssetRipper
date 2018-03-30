using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimatorControllers
{
	public struct SkeletonMask : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			m_data = stream.ReadArray<SkeletonMaskElement>();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			throw new NotSupportedException();
		}

		public IReadOnlyList<SkeletonMaskElement> Data => m_data;

		private SkeletonMaskElement[] m_data;
	}
}
