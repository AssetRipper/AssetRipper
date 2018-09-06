using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimatorControllers
{
	public struct Blend1dDataConstant : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			m_childThresholdArray = reader.ReadSingleArray();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public IReadOnlyList<float> ChildThresholdArray => m_childThresholdArray;

		private float[] m_childThresholdArray;
	}
}
