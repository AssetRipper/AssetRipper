using AssetRipper.Core.Project;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Classes.Misc.Serializable;
using AssetRipper.Core.Classes.Utils.Extensions;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.YAML;
using AssetRipper.Core.Math;

namespace AssetRipper.Core.Classes
{
	public sealed class CircleCollider2D : Collider2D
	{
		public CircleCollider2D(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(5))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool HasCenter(Version version) => version.IsLess(5);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Radius = reader.ReadSingle();
			if (HasCenter(reader.Version))
			{
				Center.Read(reader);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(RadiusName, Radius);
			return node;
		}

		public float Radius { get; set; }

		public const string RadiusName = "m_Radius";

		public Vector2f Center;
	}
}
