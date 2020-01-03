using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

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

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(CollisionShape0, CollisionShape0Name);
			yield return context.FetchDependency(CollisionShape1, CollisionShape1Name);
			yield return context.FetchDependency(CollisionShape2, CollisionShape2Name);
			yield return context.FetchDependency(CollisionShape3, CollisionShape3Name);
			yield return context.FetchDependency(CollisionShape4, CollisionShape4Name);
			yield return context.FetchDependency(CollisionShape5, CollisionShape5Name);
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add(CollisionShape0Name, CollisionShape0.ExportYAML(container));
			node.Add(CollisionShape1Name, CollisionShape1.ExportYAML(container));
			node.Add(CollisionShape2Name, CollisionShape2.ExportYAML(container));
			node.Add(CollisionShape3Name, CollisionShape3.ExportYAML(container));
			node.Add(CollisionShape4Name, CollisionShape4.ExportYAML(container));
			node.Add(CollisionShape5Name, CollisionShape5.ExportYAML(container));
			node.Add(InsideName, (int)Inside);
			node.Add(OutsideName, (int)Outside);
			node.Add(EnterName, (int)Enter);
			node.Add(ExitName, (int)Exit);
			node.Add(RadiusScaleName, RadiusScale);
			return node;
		}

		public TriggerAction Inside { get; set; }
		public TriggerAction Outside { get; set; }
		public TriggerAction Enter { get; set; }
		public TriggerAction Exit { get; set; }
		public float RadiusScale { get; set; }

		public const string CollisionShape0Name = "collisionShape0";
		public const string CollisionShape1Name = "collisionShape1";
		public const string CollisionShape2Name = "collisionShape2";
		public const string CollisionShape3Name = "collisionShape3";
		public const string CollisionShape4Name = "collisionShape4";
		public const string CollisionShape5Name = "collisionShape5";
		public const string InsideName = "inside";
		public const string OutsideName = "outside";
		public const string EnterName = "enter";
		public const string ExitName = "exit";
		public const string RadiusScaleName = "radiusScale";

		public PPtr<Component> CollisionShape0;
		public PPtr<Component> CollisionShape1;
		public PPtr<Component> CollisionShape2;
		public PPtr<Component> CollisionShape3;
		public PPtr<Component> CollisionShape4;
		public PPtr<Component> CollisionShape5;
	}
}
