using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes
{
	public sealed class EdgeCollider2D : Collider2D
	{
		public EdgeCollider2D(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 5.6.0b5 and greater
		/// </summary>
		public static bool HasEdgeRadius(UnityVersion version) => version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 5);

		/// <summary>
		/// 2020 and greater
		/// </summary>
		public static bool HasAdjacentPoints(UnityVersion version) => version.IsGreaterEqual(2020);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasEdgeRadius(reader.Version))
			{
				EdgeRadius = reader.ReadSingle();
			}
			Points = reader.ReadAssetArray<Vector2f>();
			if (HasAdjacentPoints(reader.Version))
			{
				AdjacentStartPoint.Read(reader);
				AdjacentEndPoint.Read(reader);
				UseAdjacentStartPoint = reader.ReadBoolean();
				UseAdjacentEndPoint = reader.ReadBoolean();
				reader.AlignStream();
			}
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

		public Vector2f AdjacentStartPoint { get; } = new();

		public Vector2f AdjacentEndPoint { get; } = new();

		public bool UseAdjacentStartPoint { get; set; }

		public bool UseAdjacentEndPoint { get; set; }

		public const string EdgeRadiusName = "m_EdgeRadius";
		public const string PointsName = "m_Points";
	}
}
