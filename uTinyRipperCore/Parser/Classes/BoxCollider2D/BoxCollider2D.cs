using uTinyRipper.Classes.BoxCollider2Ds;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class BoxCollider2D : Collider2D
	{
		public BoxCollider2D(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(5))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 5.6.0b5 and greater
		/// </summary>
		public static bool HasSpriteTilingProperty(Version version) => version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 5);
		/// <summary>
		/// 5.6.0b3 and greater
		/// </summary>
		public static bool HasAutoTiling(Version version) => version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 3);
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool HasCenter(Version version) => version.IsLess(5);
		/// <summary>
		/// 5.6.0b5 and greater
		/// </summary>
		public static bool HasEdgeRadius(Version version) => version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 5);

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

			Size.Read(reader);
			if (HasCenter(reader.Version))
			{
				Center.Read(reader);
			}
			if (HasEdgeRadius(reader.Version))
			{
				EdgeRadius = reader.ReadSingle();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(SpriteTilingPropertyName, SpriteTilingProperty.ExportYAML(container));
			node.Add(AutoTilingName, AutoTiling);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(SizeName, Size.ExportYAML(container));
			node.Add(EdgeRadiusName, EdgeRadius);
			return node;
		}

		public bool AutoTiling { get; set; }
		public float EdgeRadius { get; set; }

		public const string SpriteTilingPropertyName = "m_SpriteTilingProperty";
		public const string AutoTilingName = "m_AutoTiling";
		public const string SizeName = "m_Size";
		public const string EdgeRadiusName = "m_EdgeRadius";

		public SpriteTilingProperty SpriteTilingProperty;
		public Vector2f Size;
		public Vector2f Center;
	}
}
