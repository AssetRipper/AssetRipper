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
		
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool IsReadSpriteTilingProperty(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool IsReadCenter(Version version)
		{
			return version.IsLess(5);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadEdgeRadius(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(5))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadSpriteTilingProperty(reader.Version))
			{
				SpriteTilingProperty.Read(reader);
				AutoTiling = reader.ReadBoolean();
				reader.AlignStream();
			}

			Size.Read(reader);
			if (IsReadCenter(reader.Version))
			{
				Center.Read(reader);
			}
			if (IsReadEdgeRadius(reader.Version))
			{
				EdgeRadius = reader.ReadSingle();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(SpriteTilingPropertyName, SpriteTilingProperty.ExportYAML(container));
			node.Add(AutoTilingName, AutoTiling);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(SizeName, Size.ExportYAML(container));
			node.Add(EdgeRadiusName, EdgeRadius);
			return node;
		}

		public bool AutoTiling { get; private set; }
		public float EdgeRadius { get; private set; }

		public const string SpriteTilingPropertyName = "m_SpriteTilingProperty";
		public const string AutoTilingName = "m_AutoTiling";
		public const string SizeName = "m_Size";
		public const string EdgeRadiusName = "m_EdgeRadius";

		public SpriteTilingProperty SpriteTilingProperty;
		public Vector2f Size;
		public Vector2f Center;
	}
}
