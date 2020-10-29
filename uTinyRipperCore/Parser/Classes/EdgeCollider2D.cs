using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class EdgeCollider2D : Collider2D
	{
		public EdgeCollider2D(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 5.6.0b5 and greater
		/// </summary>
		public static bool HasEdgeRadius(Version version) => version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 5);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasEdgeRadius(reader.Version))
			{
				EdgeRadius = reader.ReadSingle();
			}
			Points = reader.ReadAssetArray<Vector2f>();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(EdgeRadiusName, EdgeRadius);
			node.Add(PointsName, Points.ExportYAML(container));
			return node;
		}

		public float EdgeRadius { get; set; }
		public Vector2f[] Points { get; set; }

		public const string EdgeRadiusName = "m_EdgeRadius";
		public const string PointsName = "m_Points";
	}
}
