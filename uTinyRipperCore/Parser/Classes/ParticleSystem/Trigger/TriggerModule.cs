using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.ParticleSystems
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

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			CollisionShape0.Read(reader);
			CollisionShape1.Read(reader);
			CollisionShape2.Read(reader);
			CollisionShape3.Read(reader);
			CollisionShape4.Read(reader);
			CollisionShape5.Read(reader);
			Inside = (TriggerAction)reader.ReadInt32();
			Outside = (TriggerAction)reader.ReadInt32();
			Enter = (TriggerAction)reader.ReadInt32();
			Exit = (TriggerAction)reader.ReadInt32();
			RadiusScale = reader.ReadSingle();
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
			node.Add("enter", (int)Enter);
			node.Add("exit", (int)Exit);
			node.Add("radiusScale", RadiusScale);
			return node;
		}

		public TriggerAction Inside { get; private set; }
		public TriggerAction Outside { get; private set; }
		public TriggerAction Enter { get; private set; }
		public TriggerAction Exit { get; private set; }
		public float RadiusScale { get; private set; }

		public PPtr<Component> CollisionShape0;
		public PPtr<Component> CollisionShape1;
		public PPtr<Component> CollisionShape2;
		public PPtr<Component> CollisionShape3;
		public PPtr<Component> CollisionShape4;
		public PPtr<Component> CollisionShape5;
	}
}
