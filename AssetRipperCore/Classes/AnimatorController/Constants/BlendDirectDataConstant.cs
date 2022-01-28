using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;

namespace AssetRipper.Core.Classes.AnimatorController.Constants
{
	public sealed class BlendDirectDataConstant : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			m_ChildBlendEventIDArray = reader.ReadUInt32Array();
			m_NormalizedBlendValues = reader.ReadBoolean();
			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public uint[] m_ChildBlendEventIDArray { get; set; }
		public bool m_NormalizedBlendValues { get; set; }
	}
}
