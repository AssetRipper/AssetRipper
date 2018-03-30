using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct TriggerModule : IAssetReadable, IYAMLExportable
	{
		/*private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}*/

		public void Read(AssetStream stream)
		{
			Enabled = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
			
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

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			//node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("enabled", Enabled);
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

		public bool Enabled { get; private set; }
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
