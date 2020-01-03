using uTinyRipper.Classes.CapsuleCollider2Ds;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class CapsuleCollider2D : Collider2D
	{
		public CapsuleCollider2D(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Size.Read(reader);
			Direction = (CapsuleDirection2D)reader.ReadInt32();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(SizeName, Size.ExportYAML(container));
			node.Add(DirectionName, (int)Direction);
			return node;
		}

		public CapsuleDirection2D Direction { get; set; }

		public const string SizeName = "m_Size";
		public const string DirectionName = "m_Direction";

		public Vector2f Size;
	}
}
