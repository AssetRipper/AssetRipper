using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.ParticleSystems;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class ParticleSystem : Component
	{
		public ParticleSystem(AssetInfo assetInfo):
			base(assetInfo)
		{
		}
		
		/// <summary>
		/// Less than 5.3.0
		/// </summary>
		public static bool IsReadStartDelaySingle(Version version)
		{
			return version.IsLess(5, 3);
		}
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool IsReadStopAction(Version version)
		{
			return version.IsGreaterEqual(2017, 2);
		}
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadCullingMode(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}
		/// <summary>
		/// 2017.1.0b2 and greater
		/// </summary>
		public static bool IsReadUseUnscaledTime(Version version)
		{
			return version.IsGreaterEqual(2017, 1, 0, VersionType.Beta, 2);
		}
		/// <summary>
		/// 5.4.0p4 and greater
		/// </summary>
		public static bool IsReadAutoRandomSeed(Version version)
		{
			return version.IsGreaterEqual(5, 4, 0, VersionType.Patch, 4);
		}
		/// <summary>
		/// 2017.1.0f1 and greater
		/// </summary>
		public static bool IsReadUseRigidbodyForVelocity(Version version)
		{
			return version.IsGreaterEqual(2017, 1, 0, VersionType.Final);
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadMoveWithCustomTransform(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadScalingMode(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadInheritVelocityModule(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadExternalForcesModule(Version version)
		{
			return version.IsGreaterEqual(4);
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadNoiseModule(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadTriggerModule(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadLightsModule(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadCustomDataModule(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
		
		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		private static bool IsStartDelayFirst(Version version)
		{
			return version.IsLess(5, 5);
		}
		/// <summary>
		/// Less than 5.4.0p4
		/// </summary>
		private static bool IsRandomSeedFirst(Version version)
		{
			return version.IsLess(5, 4, 0, VersionType.Patch, 4);
		}
		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		private static bool IsMoveWithTransformBool(Version version)
		{
			return version.IsLess(5, 5);
		}
		/// <summary>
		/// 5.4.0p4 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(5, 4, 0, VersionType.Patch, 4);
		}
		
		private static int GetSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(2018, 3))
			{
				return 6;
			}
			if (version.IsGreaterEqual(5, 5))
			{
				return 5;
			}
			if (version.IsGreaterEqual(5, 4, 0, VersionType.Patch, 4))
			{
				return 4;
			}
			// there is no 3rd version
			if (version.IsGreaterEqual(5, 3))
			{
				return 2;
			}
			return 1;
		}
		
		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			LengthInSec = reader.ReadSingle();
			if (IsStartDelayFirst(reader.Version))
			{
				if (IsReadStartDelaySingle(reader.Version))
				{
					StartDelaySingle = reader.ReadSingle();
				}
				else
				{
					StartDelay.Read(reader);
				}
			}
			
			SimulationSpeed = reader.ReadSingle();
			if (IsReadStopAction(reader.Version))
			{
				StopAction = (ParticleSystemStopAction)reader.ReadInt32();
			}

			if (IsRandomSeedFirst(reader.Version))
			{
				RandomSeed = unchecked((int)reader.ReadUInt32());
			}
			
			if (IsReadCullingMode(reader.Version))
			{
				CullingMode = (ParticleSystemCullingMode)reader.ReadInt32();
				RingBufferMode = (ParticleSystemRingBufferMode)reader.ReadInt32();
				RingBufferLoopRange.Read(reader);
			}

			Looping = reader.ReadBoolean();
			Prewarm = reader.ReadBoolean();
			PlayOnAwake = reader.ReadBoolean();
			if (IsReadUseUnscaledTime(reader.Version))
			{
				UseUnscaledTime = reader.ReadBoolean();
			}
			if (IsMoveWithTransformBool(reader.Version))
			{
				MoveWithTransform = reader.ReadBoolean() ? ParticleSystemSimulationSpace.Local : ParticleSystemSimulationSpace.World;
			}
			if (IsReadAutoRandomSeed(reader.Version))
			{
				AutoRandomSeed = reader.ReadBoolean();
			}
			if (IsReadUseRigidbodyForVelocity(reader.Version))
			{
				UseRigidbodyForVelocity = reader.ReadBoolean();
			}
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			if (!IsStartDelayFirst(reader.Version))
			{
				StartDelay.Read(reader);
				reader.AlignStream(AlignType.Align4);
			}
			if (!IsMoveWithTransformBool(reader.Version))
			{
				MoveWithTransform = (ParticleSystemSimulationSpace)reader.ReadInt32();
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadMoveWithCustomTransform(reader.Version))
			{
				MoveWithCustomTransform.Read(reader);
			}
			if (IsReadScalingMode(reader.Version))
			{
				ScalingMode = (ParticleSystemScalingMode)reader.ReadInt32();
			}
			if (!IsRandomSeedFirst(reader.Version))
			{
				RandomSeed = reader.ReadInt32();
			}

			InitialModule.Read(reader);
			ShapeModule.Read(reader);
			EmissionModule.Read(reader);
			SizeModule.Read(reader);
			RotationModule.Read(reader);
			ColorModule.Read(reader);
			UVModule.Read(reader);
			VelocityModule.Read(reader);
			if (IsReadInheritVelocityModule(reader.Version))
			{
				InheritVelocityModule.Read(reader);
			}
			ForceModule.Read(reader);
			if (IsReadExternalForcesModule(reader.Version))
			{
				ExternalForcesModule.Read(reader);
			}
			ClampVelocityModule.Read(reader);
			if (IsReadNoiseModule(reader.Version))
			{
				NoiseModule.Read(reader);
			}
			SizeBySpeedModule.Read(reader);
			RotationBySpeedModule.Read(reader);
			ColorBySpeedModule.Read(reader);
			CollisionModule.Read(reader);
			if (IsReadTriggerModule(reader.Version))
			{
				TriggerModule.Read(reader);
			}
			SubModule.Read(reader);
			if (IsReadLightsModule(reader.Version))
			{
				LightsModule.Read(reader);
				TrailModule.Read(reader);
			}
			if (IsReadCustomDataModule(reader.Version))
			{
				CustomDataModule.Read(reader);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
			
			yield return MoveWithCustomTransform.FetchDependency(file, isLog, ToLogString, "moveWithCustomTransform");
			foreach(Object asset in CollisionModule.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
			foreach(Object asset in SubModule.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(LengthInSecName, LengthInSec);
			node.Add(SimulationSpeedName, SimulationSpeed);
			node.Add(StopActionName, (int)StopAction);
			node.Add(LoopingName, Looping);
			node.Add(PrewarmName, Prewarm);
			node.Add(PlayOnAwakeName, PlayOnAwake);
			node.Add(UseUnscaledTimeName, UseUnscaledTime);
			node.Add(AutoRandomSeedName, GetAutoRandomSeed(container.Version));
			node.Add(UseRigidbodyForVelocityName, GetUseRigidbodyForVelocity(container.Version));
			node.Add(StartDelayName, GetStartDelay(container.Version).ExportYAML(container));
			node.Add(MoveWithTransformName, (int)MoveWithTransform);
			node.Add(MoveWithCustomTransformName, MoveWithCustomTransform.ExportYAML(container));
			node.Add(ScalingModeName, (int)GetScalingMode(container.Version));
			node.Add(RandomSeedName, RandomSeed);
			node.Add(InitialModuleName, InitialModule.ExportYAML(container));
			node.Add(ShapeModuleName, ShapeModule.ExportYAML(container));
			node.Add(EmissionModuleName, EmissionModule.ExportYAML(container));
			node.Add(SizeModuleName, SizeModule.ExportYAML(container));
			node.Add(RotationModuleName, RotationModule.ExportYAML(container));
			node.Add(ColorModuleName, ColorModule.ExportYAML(container));
			node.Add(UVModuleName, UVModule.ExportYAML(container));
			node.Add(VelocityModuleName, VelocityModule.ExportYAML(container));
			node.Add(InheritVelocityModuleName, GetInheritVelocityModule(container.Version).ExportYAML(container));
			node.Add(ForceModuleName, ForceModule.ExportYAML(container));
			node.Add(ExternalForcesModuleName, GetExternalForcesModule(container.Version).ExportYAML(container));
			node.Add(ClampVelocityModuleName, ClampVelocityModule.ExportYAML(container));
			node.Add(NoiseModuleName, GetNoiseModule(container.Version).ExportYAML(container));
			node.Add(SizeBySpeedModuleName, SizeBySpeedModule.ExportYAML(container));
			node.Add(RotationBySpeedModuleName, RotationBySpeedModule.ExportYAML(container));
			node.Add(ColorBySpeedModuleName, ColorBySpeedModule.ExportYAML(container));
			node.Add(CollisionModuleName, CollisionModule.ExportYAML(container));
			node.Add(TriggerModuleName, GetTriggerModule(container.Version).ExportYAML(container));
			node.Add(SubModuleName, SubModule.ExportYAML(container));
			node.Add(LightsModuleName, GetLightsModule(container.Version).ExportYAML(container));
			node.Add(TrailModuleName, GetTrailModule(container.Version).ExportYAML(container));
			node.Add(CustomDataModuleName, GetCustomDataModule(container.Version).ExportYAML(container));
			return node;
		}

		private bool GetAutoRandomSeed(Version version)
		{
			return IsReadAutoRandomSeed(version) ? AutoRandomSeed : true;
		}
		public bool GetUseRigidbodyForVelocity(Version version)
		{
			return IsReadUseRigidbodyForVelocity(version) ? UseRigidbodyForVelocity : true;
		}
		private MinMaxCurve GetStartDelay(Version version)
		{
			return IsReadStartDelaySingle(version) ? new MinMaxCurve(StartDelaySingle) : StartDelay;
		}
		private ParticleSystemScalingMode GetScalingMode(Version version)
		{
			return IsReadScalingMode(version) ? ScalingMode : ParticleSystemScalingMode.Shape;
		}
		private InheritVelocityModule GetInheritVelocityModule(Version version)
		{
			return IsReadInheritVelocityModule(version) ? InheritVelocityModule : new InheritVelocityModule(InitialModule.InheritVelocity);
		}
		private ExternalForcesModule GetExternalForcesModule(Version version)
		{
			return IsReadExternalForcesModule(version) ? ExternalForcesModule : new ExternalForcesModule(true);
		}
		public NoiseModule GetNoiseModule(Version version)
		{
			return IsReadNoiseModule(version) ? NoiseModule : new NoiseModule(true);
		}
		public TriggerModule GetTriggerModule(Version version)
		{
			return IsReadTriggerModule(version) ? TriggerModule : new TriggerModule(true);
		}
		public LightsModule GetLightsModule(Version version)
		{
			return IsReadLightsModule(version) ? LightsModule : new LightsModule(true);
		}
		public TrailModule GetTrailModule(Version version)
		{
			return IsReadLightsModule(version) ? TrailModule : new TrailModule(true);
		}
		public CustomDataModule GetCustomDataModule(Version version)
		{
			return IsReadCustomDataModule(version) ? CustomDataModule : new CustomDataModule(true);
		}

		public float LengthInSec { get; private set; }
		public float StartDelaySingle { get; private set; }
		/// <summary>
		/// Speed previously
		/// </summary>
		public float SimulationSpeed { get; private set; }
		public ParticleSystemStopAction StopAction { get; private set; }
		public ParticleSystemCullingMode CullingMode { get; private set; }
		public ParticleSystemRingBufferMode RingBufferMode { get; private set; }
		public Vector2f RingBufferLoopRange { get; private set; }
		public bool Looping { get; private set; }
		public bool Prewarm { get; private set; }
		public bool PlayOnAwake { get; private set; }
		public bool UseUnscaledTime { get; private set; }
		public bool AutoRandomSeed { get; private set; }
		public bool UseRigidbodyForVelocity { get; private set; }
		public ParticleSystemSimulationSpace MoveWithTransform { get; private set; }
		public ParticleSystemScalingMode ScalingMode { get; private set; }
		public int RandomSeed { get; private set; }
		public InitialModule InitialModule { get; } = new InitialModule();
		public ShapeModule ShapeModule { get; } = new ShapeModule();
		public EmissionModule EmissionModule { get; } = new EmissionModule();
		public SizeModule SizeModule { get; } = new SizeModule();
		public RotationModule RotationModule { get; } = new RotationModule();
		public ColorModule ColorModule { get; } = new ColorModule();
		public UVModule UVModule { get; } = new UVModule();
		public VelocityModule VelocityModule { get; } = new VelocityModule();
		public InheritVelocityModule InheritVelocityModule { get; } = new InheritVelocityModule();
		public ForceModule ForceModule { get; } = new ForceModule();
		public ExternalForcesModule ExternalForcesModule { get; } = new ExternalForcesModule();
		public ClampVelocityModule ClampVelocityModule { get; } = new ClampVelocityModule();
		public NoiseModule NoiseModule { get; } = new NoiseModule();
		public SizeBySpeedModule SizeBySpeedModule { get; } = new SizeBySpeedModule();
		public RotationBySpeedModule RotationBySpeedModule { get; } = new RotationBySpeedModule();
		public ColorBySpeedModule ColorBySpeedModule { get; } = new ColorBySpeedModule();
		public CollisionModule CollisionModule { get; } = new CollisionModule();
		public TriggerModule TriggerModule { get; } = new TriggerModule();
		public SubModule SubModule { get; } = new SubModule();
		public LightsModule LightsModule { get; } = new LightsModule();
		public TrailModule TrailModule { get; } = new TrailModule();
		public CustomDataModule CustomDataModule { get; } = new CustomDataModule();

		public const string LengthInSecName = "lengthInSec";
		public const string SimulationSpeedName = "simulationSpeed";
		public const string StopActionName = "stopAction";
		public const string LoopingName = "looping";
		public const string PrewarmName = "prewarm";
		public const string PlayOnAwakeName = "playOnAwake";
		public const string UseUnscaledTimeName = "useUnscaledTime";
		public const string AutoRandomSeedName = "autoRandomSeed";
		public const string UseRigidbodyForVelocityName = "useRigidbodyForVelocity";
		public const string StartDelayName = "startDelay";
		public const string MoveWithTransformName = "moveWithTransform";
		public const string MoveWithCustomTransformName = "moveWithCustomTransform";
		public const string ScalingModeName = "scalingMode";
		public const string RandomSeedName = "randomSeed";
		public const string InitialModuleName = "InitialModule";
		public const string ShapeModuleName = "ShapeModule";
		public const string EmissionModuleName = "EmissionModule";
		public const string SizeModuleName = "SizeModule";
		public const string RotationModuleName = "RotationModule";
		public const string ColorModuleName = "ColorModule";
		public const string UVModuleName = "UVModule";
		public const string VelocityModuleName = "VelocityModule";
		public const string InheritVelocityModuleName = "InheritVelocityModule";
		public const string ForceModuleName = "ForceModule";
		public const string ExternalForcesModuleName = "ExternalForcesModule";
		public const string ClampVelocityModuleName = "ClampVelocityModule";
		public const string NoiseModuleName = "NoiseModule";
		public const string SizeBySpeedModuleName = "SizeBySpeedModule";
		public const string RotationBySpeedModuleName = "RotationBySpeedModule";
		public const string ColorBySpeedModuleName = "ColorBySpeedModule";
		public const string CollisionModuleName = "CollisionModule";
		public const string TriggerModuleName = "TriggerModule";
		public const string SubModuleName = "SubModule";
		public const string LightsModuleName = "LightsModule";
		public const string TrailModuleName = "TrailModule";
		public const string CustomDataModuleName = "CustomDataModule";

		public MinMaxCurve StartDelay;
		public PPtr<Transform> MoveWithCustomTransform;
	}
}
