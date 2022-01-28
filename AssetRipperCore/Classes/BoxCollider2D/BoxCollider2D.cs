using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.BoxCollider2D
{
	public sealed class BoxCollider2D : Collider2D
	{
		public BoxCollider2D(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
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
		public static bool HasSpriteTilingProperty(UnityVersion version) => version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 5);
		/// <summary>
		/// 5.6.0b3 and greater
		/// </summary>
		public static bool HasAutoTiling(UnityVersion version) => version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 3);
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool HasCenter(UnityVersion version) => version.IsLess(5);
		/// <summary>
		/// 5.6.0b5 and greater
		/// </summary>
		public static bool HasEdgeRadius(UnityVersion version) => version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 5);

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

		public SpriteTilingProperty SpriteTilingProperty = new();
		public Vector2f Size = new();
		public Vector2f Center = new();
	}
}
