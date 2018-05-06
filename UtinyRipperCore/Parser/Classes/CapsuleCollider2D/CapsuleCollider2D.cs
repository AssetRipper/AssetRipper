using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.CapsuleCollider2Ds;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public sealed class CapsuleCollider2D : Collider2D
	{
		public CapsuleCollider2D(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			Size.Read(stream);
			Direction = (CapsuleDirection2D)stream.ReadInt32();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_Size", Size.ExportYAML(container));
			node.Add("m_Direction", (int)Direction);
			return node;
		}

		public CapsuleDirection2D Direction { get; private set; }

		public Vector2f Size;
	}
}
