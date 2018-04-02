using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public class TriggerModule : ParticleSystemModule
	{
		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			
			CollisionShape0.Read(stream);
			CollisionShape1.Read(stream);
			CollisionShape2.Read(stream);
			CollisionShape3.Read(stream);
			CollisionShape4.Read(stream);
			CollisionShape5.Read(stream);
			Inside = stream.ReadInt32();
			Outside = stream.ReadInt32();
			Enter = stream.ReadInt32();
			Exit = stream.ReadInt32();
			RadiusScale = stream.ReadSingle();
		}

		public override YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(exporter);
			node.Add("collisionShape0", CollisionShape0.ExportYAML(exporter));
			node.Add("collisionShape1", CollisionShape1.ExportYAML(exporter));
			node.Add("collisionShape2", CollisionShape2.ExportYAML(exporter));
			node.Add("collisionShape3", CollisionShape3.ExportYAML(exporter));
			node.Add("collisionShape4", CollisionShape4.ExportYAML(exporter));
			node.Add("collisionShape5", CollisionShape5.ExportYAML(exporter));
			node.Add("inside", Inside);
			node.Add("outside", Outside);
			node.Add("enter", Enter);
			node.Add("exit", Exit);
			node.Add("radiusScale", RadiusScale);
			return node;
		}

		public int Inside { get; private set; }
		public int Outside { get; private set; }
		public int Enter { get; private set; }
		public int Exit { get; private set; }
		public float RadiusScale { get; private set; }

		public PPtr<Component> CollisionShape0;
		public PPtr<Component> CollisionShape1;
		public PPtr<Component> CollisionShape2;
		public PPtr<Component> CollisionShape3;
		public PPtr<Component> CollisionShape4;
		public PPtr<Component> CollisionShape5;
	}
}
