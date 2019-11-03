using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class PhysicsMaterial2D : NamedObject
	{
		public PhysicsMaterial2D(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Friction = reader.ReadSingle();
			Bounciness = reader.ReadSingle();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(FrictionName, Friction);
			node.Add(BouncinessName, Bounciness);
			return node;
		}

		public float Friction { get; set; }
		public float Bounciness { get; set; }

		public const string FrictionName = "friction";
		public const string BouncinessName = "bounciness";
	}
}
