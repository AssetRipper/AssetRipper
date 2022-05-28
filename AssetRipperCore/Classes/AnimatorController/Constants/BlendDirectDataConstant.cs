using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.AnimatorController.Constants
{
	public sealed class BlendDirectDataConstant : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			m_ChildBlendEventIDArray = reader.ReadUInt32Array();
			m_NormalizedBlendValues = reader.ReadBoolean();
			reader.AlignStream();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public uint[] m_ChildBlendEventIDArray { get; set; }
		public bool m_NormalizedBlendValues { get; set; }
	}
}
