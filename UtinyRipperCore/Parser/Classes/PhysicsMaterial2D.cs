using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public sealed class PhysicsMaterial2D : NamedObject
	{
		public PhysicsMaterial2D(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			Friction = stream.ReadSingle();
			Bounciness = stream.ReadSingle();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.Add("friction", Friction);
			node.Add("bounciness", Bounciness);
			return node;
		}

		public float Friction { get; private set; }
		public float Bounciness { get; private set; }
	}
}
