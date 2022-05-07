using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.CapsuleCollider2D
{
	public sealed class CapsuleCollider2D : Collider2D
	{
		public CapsuleCollider2D(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Size.Read(reader);
			Direction = (CapsuleDirection2D)reader.ReadInt32();
		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.Add(SizeName, Size.ExportYaml(container));
			node.Add(DirectionName, (int)Direction);
			return node;
		}

		public CapsuleDirection2D Direction { get; set; }

		public const string SizeName = "m_Size";
		public const string DirectionName = "m_Direction";

		public Vector2f Size = new();
	}
}
