using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.AvatarMasks;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public sealed class AvatarMask : NamedObject
	{
		public AvatarMask(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			m_mask = stream.ReadUInt32Array();
			m_elements = stream.ReadArray<TransformMaskElement>();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_Mask", Mask.ExportYAML(true));
			node.Add("m_Elements", Elements.ExportYAML(container));
			return node;
		}

		public override string ExportExtension => "mask";

		public IReadOnlyList<uint> Mask => m_mask;
		public IReadOnlyList<TransformMaskElement> Elements => m_elements;

		private uint[] m_mask;
		private TransformMaskElement[] m_elements;
	}
}
