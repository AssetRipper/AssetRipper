using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.ParticleSystems
{
	public sealed class TriggerModule : ParticleSystemModule, IDependent
	{
		public TriggerModule()
		{
		}

		public TriggerModule(bool _)
		{
			Inside = TriggerAction.Kill;
			RadiusScale = 1.0f;
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			
			CollisionShape0.Read(stream);
			CollisionShape1.Read(stream);
			CollisionShape2.Read(stream);
			CollisionShape3.Read(stream);
			CollisionShape4.Read(stream);
			CollisionShape5.Read(stream);
			Inside = (TriggerAction)stream.ReadInt32();
			Outside = (TriggerAction)stream.ReadInt32();
			Enter = stream.ReadInt32();
			Exit = stream.ReadInt32();
			RadiusScale = stream.ReadSingle();
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return CollisionShape0.FetchDependency(file, isLog, () => nameof(TriggerModule), "collisionShape0");
			yield return CollisionShape1.FetchDependency(file, isLog, () => nameof(TriggerModule), "collisionShape1");
			yield return CollisionShape2.FetchDependency(file, isLog, () => nameof(TriggerModule), "collisionShape2");
			yield return CollisionShape3.FetchDependency(file, isLog, () => nameof(TriggerModule), "collisionShape3");
			yield return CollisionShape4.FetchDependency(file, isLog, () => nameof(TriggerModule), "collisionShape4");
			yield return CollisionShape5.FetchDependency(file, isLog, () => nameof(TriggerModule), "collisionShape5");
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add("collisionShape0", CollisionShape0.ExportYAML(container));
			node.Add("collisionShape1", CollisionShape1.ExportYAML(container));
			node.Add("collisionShape2", CollisionShape2.ExportYAML(container));
			node.Add("collisionShape3", CollisionShape3.ExportYAML(container));
			node.Add("collisionShape4", CollisionShape4.ExportYAML(container));
			node.Add("collisionShape5", CollisionShape5.ExportYAML(container));
			node.Add("inside", (int)Inside);
			node.Add("outside", (int)Outside);
			node.Add("enter", Enter);
			node.Add("exit", Exit);
			node.Add("radiusScale", RadiusScale);
			return node;
		}

		public TriggerAction Inside { get; private set; }
		public TriggerAction Outside { get; private set; }
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
