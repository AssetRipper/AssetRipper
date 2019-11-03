using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class CapsuleCollider : Collider
	{
		public CapsuleCollider(AssetInfo assetInfo):
			base(assetInfo)
		{
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
			Height = reader.ReadSingle();
			Direction = reader.ReadInt32();
			Center.Read(reader);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(RadiusName, Radius);
			node.Add(HeightName, Height);
			node.Add(DirectionName, Direction);
			node.Add(CenterName, Center.ExportYAML(container));
			return node;
		}

		public float Radius { get; set; }
		public float Height { get; set; }
		public int Direction { get; set; }

		public const string RadiusName = "m_Radius";
		public const string HeightName = "m_Height";
		public const string DirectionName = "m_Direction";
		public const string CenterName = "m_Center";

		public Vector3f Center;

		protected override bool IncludesIsTrigger => true;
		protected override bool IncludesMaterial => true;
	}
}
