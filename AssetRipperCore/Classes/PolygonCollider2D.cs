using AssetRipper.Core.Classes.BoxCollider2D;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes
{
	public sealed class PolygonCollider2D : Collider2D
	{
		public PolygonCollider2D(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 5.6.0b5 and greater
		/// </summary>
		public static bool HasSpriteTilingProperty(UnityVersion version) => version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 5);
		/// <summary>
		/// 5.6.0b3 and greater
		/// </summary>
		public static bool HasAutoTiling(UnityVersion version) => version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 3);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasSpriteTilingProperty(reader.Version))
			{
				SpriteTilingProperty.Read(reader);
			}
			if (HasAutoTiling(reader.Version))
			{
				AutoTiling = reader.ReadBoolean();
				reader.AlignStream();
			}
			Points.Read(reader);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(SpriteTilingPropertyName, SpriteTilingProperty.ExportYAML(container));
			node.Add(AutoTilingName, AutoTiling);
			node.Add(PointsName, Points.ExportYAML(container));
			return node;
		}

		public bool AutoTiling { get; set; }

		public const string SpriteTilingPropertyName = "m_SpriteTilingProperty";
		public const string AutoTilingName = "m_AutoTiling";
		public const string PointsName = "m_Points";

		public SpriteTilingProperty SpriteTilingProperty = new();
		/// <summary>
		/// Poly previously
		/// </summary>
		public Polygon2D Points = new();
	}
}
