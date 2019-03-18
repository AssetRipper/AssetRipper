using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.BoxCollider2Ds;
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
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

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
				reader.AlignStream(AlignType.Align4);
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
			node.Add("m_SpriteTilingProperty", SpriteTilingProperty.ExportYAML(container));
			node.Add("m_AutoTiling", AutoTiling);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Size", Size.ExportYAML(container));
			node.Add("m_EdgeRadius", EdgeRadius);
			return node;
		}

		public bool AutoTiling { get; private set; }
		public float EdgeRadius { get; private set; }

		public SpriteTilingProperty SpriteTilingProperty;
		public Vector2f Size;
		public Vector2f Center;
	}
}
