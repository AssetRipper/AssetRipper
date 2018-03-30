using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimatorControllers
{
	public struct BlendDirectDataConstant : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			m_childBlendEventIDArray = stream.ReadUInt32Array();
			NormalizedBlendValues = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			throw new NotSupportedException();
		}

		public IReadOnlyList<uint> ChildBlendEventIDArray => m_childBlendEventIDArray;
		public bool NormalizedBlendValues { get; private set; }

		private uint[] m_childBlendEventIDArray;
	}
}
