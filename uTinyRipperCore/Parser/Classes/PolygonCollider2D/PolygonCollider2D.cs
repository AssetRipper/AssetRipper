using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.BoxCollider2Ds;
using uTinyRipper.Classes.PolygonCollider2Ds;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class PolygonCollider2D : Collider2D
	{
		public PolygonCollider2D(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadSpriteTilingProperty(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			if (IsReadSpriteTilingProperty(reader.Version))
			{
				SpriteTilingProperty.Read(reader);
				AutoTiling = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
			Points.Read(reader);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_SpriteTilingProperty", SpriteTilingProperty.ExportYAML(container));
			node.Add("m_AutoTiling", AutoTiling);
			node.Add("m_Points", Points.ExportYAML(container));
			return node;
		}

		public bool AutoTiling { get; private set; }

		public SpriteTilingProperty SpriteTilingProperty;
		/// <summary>
		/// Poly previously
		/// </summary>
		public Polygon2D Points;
	}
}
