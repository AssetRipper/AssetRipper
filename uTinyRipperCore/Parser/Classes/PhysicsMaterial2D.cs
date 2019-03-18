using uTinyRipper.AssetExporters;
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
			node.Add("friction", Friction);
			node.Add("bounciness", Bounciness);
			return node;
		}

		public float Friction { get; private set; }
		public float Bounciness { get; private set; }
	}
}
