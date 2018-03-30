using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.ParticleSystems;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public sealed class ParticleSystem : Component
	{
		public ParticleSystem(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 5;
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			
			LengthInSec = stream.ReadSingle();
			SimulationSpeed = stream.ReadSingle();
			StopAction = stream.ReadInt32();
			Looping = stream.ReadBoolean();
			Prewarm = stream.ReadBoolean();
			PlayOnAwake = stream.ReadBoolean();
			UseUnscaledTime = stream.ReadBoolean();
			AutoRandomSeed = stream.ReadBoolean();
			UseRigidbodyForVelocity = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
			
			StartDelay.Read(stream);
			stream.AlignStream(AlignType.Align4);
			
			MoveWithTransform = stream.ReadInt32();
			stream.AlignStream(AlignType.Align4);
			
			MoveWithCustomTransform.Read(stream);
			ScalingMode = stream.ReadInt32();
			RandomSeed = stream.ReadInt32();
			InitialModule.Read(stream);
			ShapeModule.Read(stream);
			EmissionModule.Read(stream);
			SizeModule.Read(stream);
			RotationModule.Read(stream);
			ColorModule.Read(stream);
			UVModule.Read(stream);
			VelocityModule.Read(stream);
			InheritVelocityModule.Read(stream);
			ForceModule.Read(stream);
			ExternalForcesModule.Read(stream);
			ClampVelocityModule.Read(stream);
			NoiseModule.Read(stream);
			SizeBySpeedModule.Read(stream);
			RotationBySpeedModule.Read(stream);
			ColorBySpeedModule.Read(stream);
			CollisionModule.Read(stream);
			TriggerModule.Read(stream);
			SubModule.Read(stream);
			LightsModule.Read(stream);
			TrailModule.Read(stream);
			CustomDataModule.Read(stream);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("lengthInSec", LengthInSec);
			node.Add("simulationSpeed", SimulationSpeed);
			node.Add("stopAction", StopAction);
			node.Add("looping", Looping);
			node.Add("prewarm", Prewarm);
			node.Add("playOnAwake", PlayOnAwake);
			node.Add("useUnscaledTime", UseUnscaledTime);
			node.Add("autoRandomSeed", AutoRandomSeed);
			node.Add("useRigidbodyForVelocity", UseRigidbodyForVelocity);
			node.Add("startDelay", StartDelay.ExportYAML(exporter));
			node.Add("moveWithTransform", MoveWithTransform);
			node.Add("moveWithCustomTransform", MoveWithCustomTransform.ExportYAML(exporter));
			node.Add("scalingMode", ScalingMode);
			node.Add("randomSeed", RandomSeed);
			node.Add("InitialModule", InitialModule.ExportYAML(exporter));
			node.Add("ShapeModule", ShapeModule.ExportYAML(exporter));
			node.Add("EmissionModule", EmissionModule.ExportYAML(exporter));
			node.Add("SizeModule", SizeModule.ExportYAML(exporter));
			node.Add("RotationModule", RotationModule.ExportYAML(exporter));
			node.Add("ColorModule", ColorModule.ExportYAML(exporter));
			node.Add("UVModule", UVModule.ExportYAML(exporter));
			node.Add("VelocityModule", VelocityModule.ExportYAML(exporter));
			node.Add("InheritVelocityModule", InheritVelocityModule.ExportYAML(exporter));
			node.Add("ForceModule", ForceModule.ExportYAML(exporter));
			node.Add("ExternalForcesModule", ExternalForcesModule.ExportYAML(exporter));
			node.Add("ClampVelocityModule", ClampVelocityModule.ExportYAML(exporter));
			node.Add("NoiseModule", NoiseModule.ExportYAML(exporter));
			node.Add("SizeBySpeedModule", SizeBySpeedModule.ExportYAML(exporter));
			node.Add("RotationBySpeedModule", RotationBySpeedModule.ExportYAML(exporter));
			node.Add("ColorBySpeedModule", ColorBySpeedModule.ExportYAML(exporter));
			node.Add("CollisionModule", CollisionModule.ExportYAML(exporter));
			node.Add("TriggerModule", TriggerModule.ExportYAML(exporter));
			node.Add("SubModule", SubModule.ExportYAML(exporter));
			node.Add("LightsModule", LightsModule.ExportYAML(exporter));
			node.Add("TrailModule", TrailModule.ExportYAML(exporter));
			node.Add("CustomDataModule", CustomDataModule.ExportYAML(exporter));
			throw new System.NotImplementedException();
		}

		public float LengthInSec { get; private set; }
		public float SimulationSpeed { get; private set; }
		public int StopAction { get; private set; }
		public bool Looping { get; private set; }
		public bool Prewarm { get; private set; }
		public bool PlayOnAwake { get; private set; }
		public bool UseUnscaledTime { get; private set; }
		public bool AutoRandomSeed { get; private set; }
		public bool UseRigidbodyForVelocity { get; private set; }
		public int MoveWithTransform { get; private set; }
		public int ScalingMode { get; private set; }
		public int RandomSeed { get; private set; }
		
		public MinMaxCurve StartDelay;
		public PPtr<Transform> MoveWithCustomTransform;
		public InitialModule InitialModule;
		public ShapeModule ShapeModule;
		public EmissionModule EmissionModule;
		public SizeModule SizeModule;
		public RotationModule RotationModule;
		public ColorModule ColorModule;
		public UVModule UVModule;
		public VelocityModule VelocityModule;
		public InheritVelocityModule InheritVelocityModule;
		public ForceModule ForceModule;
		public ExternalForcesModule ExternalForcesModule;
		public ClampVelocityModule ClampVelocityModule;
		public NoiseModule NoiseModule;
		public SizeBySpeedModule SizeBySpeedModule;
		public RotationBySpeedModule RotationBySpeedModule;
		public ColorBySpeedModule ColorBySpeedModule;
		public CollisionModule CollisionModule;
		public TriggerModule TriggerModule;
		public SubModule SubModule;
		public LightsModule LightsModule;
		public TrailModule TrailModule;
		public CustomDataModule CustomDataModule;
	}
}
