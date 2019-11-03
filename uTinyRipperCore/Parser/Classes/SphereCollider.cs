using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class SphereCollider : Collider
	{
		public SphereCollider(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
		{
			// min version is 2nd
			return 2;
		}

		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsAlign(Version version) => version.IsGreaterEqual(2, 1);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}
			
			Radius = reader.ReadSingle();
			Center.Read(reader);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(RadiusName, Radius);
			node.Add(CenterName, Center.ExportYAML(container));
			return node;
		}

		public float Radius { get; set; }

		public const string RadiusName = "m_Radius";
		public const string CenterName = "m_Center";

		public Vector3f Center;

		protected override bool IncludesIsTrigger => true;
		protected override bool IncludesMaterial => true;
	}
}
