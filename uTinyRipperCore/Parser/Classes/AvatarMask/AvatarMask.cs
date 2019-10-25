using System.Collections.Generic;
using uTinyRipper.Project;
using uTinyRipper.Classes.AvatarMasks;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class AvatarMask : NamedObject
	{
		public AvatarMask(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			m_mask = reader.ReadUInt32Array();
			m_elements = reader.ReadAssetArray<TransformMaskElement>();
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
