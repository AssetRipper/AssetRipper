using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.BoxCollider2Ds;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
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
			return version.IsGreaterEqual(2017);
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

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			if (IsReadSpriteTilingProperty(stream.Version))
			{
				SpriteTilingProperty.Read(stream);
				AutoTiling = stream.ReadBoolean();
				stream.AlignStream(AlignType.Align4);
			}

			Size.Read(stream);
			if (IsReadCenter(stream.Version))
			{
				Center.Read(stream);
			}
			if (IsReadEdgeRadius(stream.Version))
			{
				EdgeRadius = stream.ReadSingle();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.Add("m_SpriteTilingProperty", SpriteTilingProperty.ExportYAML(exporter));
			node.Add("m_AutoTiling", AutoTiling);
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("m_Size", Size.ExportYAML(exporter));
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
