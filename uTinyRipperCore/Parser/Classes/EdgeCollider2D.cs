using System.Collections.Generic;
using uTinyRipper.AssetExporters;
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
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadEdgeRadius(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadEdgeRadius(reader.Version))
			{
				EdgeRadius = reader.ReadSingle();
			}
			m_points = reader.ReadAssetArray<Vector2f>();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_EdgeRadius", EdgeRadius);
			node.Add("m_Points", m_points.ExportYAML(container));
			return node;
		}

		public float EdgeRadius { get; private set; }
		public IReadOnlyList<Vector2f> Points => m_points;

		private Vector2f[] m_points;
	}
}
