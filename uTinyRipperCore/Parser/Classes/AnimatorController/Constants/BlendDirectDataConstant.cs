using System;
using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorControllers
{
	public struct BlendDirectDataConstant : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			m_childBlendEventIDArray = reader.ReadUInt32Array();
			NormalizedBlendValues = reader.ReadBoolean();
			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public IReadOnlyList<uint> ChildBlendEventIDArray => m_childBlendEventIDArray;
		public bool NormalizedBlendValues { get; private set; }

		private uint[] m_childBlendEventIDArray;
	}
}
