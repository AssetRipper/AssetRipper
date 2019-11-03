using System.Collections.Generic;
using uTinyRipper.Classes.ParticleSystems;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	public sealed class ParticleSystem : Component
	{
		public ParticleSystem(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
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

		/// <summary>
		/// Less than 5.3.0
		/// </summary>
		public static bool HasStartDelaySingle(Version version) => version.IsLess(5, 3);
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool HasStopAction(Version version) => version.IsGreaterEqual(2017, 2);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasCullingMode(Version version) => version.IsGreaterEqual(2018, 3);
		/// <summary>
		/// 2017.1.0b2 and greater
		/// </summary>
		public static bool HasUseUnscaledTime(Version version) => version.IsGreaterEqual(2017, 1, 0, VersionType.Beta, 2);
		/// <summary>
		/// 5.4.0p4 and greater
		/// </summary>
		public static bool HasAutoRandomSeed(Version version) => version.IsGreaterEqual(5, 4, 0, VersionType.Patch, 4);
		/// <summary>
		/// 2017.1.0f1 and greater
		/// </summary>
		public static bool HasUseRigidbodyForVelocity(Version version) => version.IsGreaterEqual(2017, 1, 0, VersionType.Final);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasMoveWithCustomTransform(Version version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasScalingMode(Version version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasInheritVelocityModule(Version version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasExternalForcesModule(Version version) => version.IsGreaterEqual(4);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasNoiseModule(Version version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasTriggerModule(Version version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasLightsModule(Version version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasCustomDataModule(Version version) => version.IsGreaterEqual(5, 6);
		
		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		private static bool IsStartDelayFirst(Version version) => version.IsLess(5, 5);
		/// <summary>
		/// Less than 5.4.0p4
		/// </summary>
		private static bool IsRandomSeedFirst(Version version) => version.IsLess(5, 4, 0, VersionType.Patch, 4);
		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		private static bool IsMoveWithTransformBool(Version version) => version.IsLess(5, 5);
		/// <summary>
		/// 5.4.0p4 and greater
		/// </summary>
		private static bool IsAlign(Version version) => version.IsGreaterEqual(5, 4, 0, VersionType.Patch, 4);
		
		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			LengthInSec = reader.ReadSingle();
			if (IsStartDelayFirst(reader.Version))
			{
				if (HasStartDelaySingle(reader.Version))
				{
					StartDelaySingle = reader.ReadSingle();
				}
				else
				{
					StartDelay.Read(reader);
				}
			}
			
			SimulationSpeed = reader.ReadSingle();
			if (HasStopAction(reader.Version))
			{
				StopAction = (ParticleSystemStopAction)reader.ReadInt32();
			}

			if (IsRandomSeedFirst(reader.Version))
			{
				RandomSeed = unchecked((int)reader.ReadUInt32());
			}
			
			if (HasCullingMode(reader.Version))
			{
				CullingMode = (ParticleSystemCullingMode)reader.ReadInt32();
				RingBufferMode = (ParticleSystemRingBufferMode)reader.ReadInt32();
				RingBufferLoopRange.Read(reader);
			}

			Looping = reader.ReadBoolean();
			Prewarm = reader.ReadBoolean();
			PlayOnAwake = reader.ReadBoolean();
			if (HasUseUnscaledTime(reader.Version))
			{
				UseUnscaledTime = reader.ReadBoolean();
			}
			if (IsMoveWithTransformBool(reader.Version))
			{
				MoveWithTransform = reader.ReadBoolean() ? ParticleSystemSimulationSpace.Local : ParticleSystemSimulationSpace.World;
			}
			if (HasAutoRandomSeed(reader.Version))
			{
				AutoRandomSeed = reader.ReadBoolean();
			}
			if (HasUseRigidbodyForVelocity(reader.Version))
			{
				UseRigidbodyForVelocity = reader.ReadBoolean();
			}
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}

			if (!IsStartDelayFirst(reader.Version))
			{
				StartDelay.Read(reader);
				reader.AlignStream();
			}
			if (!IsMoveWithTransformBool(reader.Version))
			{
				MoveWithTransform = (ParticleSystemSimulationSpace)reader.ReadInt32();
				reader.AlignStream();
			}

			if (HasMoveWithCustomTransform(reader.Version))
			{
				MoveWithCustomTransform.Read(reader);
			}
			if (HasScalingMode(reader.Version))
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
			if (HasInheritVelocityModule(reader.Version))
			{
				InheritVelocityModule.Read(reader);
			}
			ForceModule.Read(reader);
			if (HasExternalForcesModule(reader.Version))
			{
				ExternalForcesModule.Read(reader);
			}
			ClampVelocityModule.Read(reader);
			if (HasNoiseModule(reader.Version))
			{
				NoiseModule.Read(reader);
			}
			SizeBySpeedModule.Read(reader);
			RotationBySpeedModule.Read(reader);
			ColorBySpeedModule.Read(reader);
			CollisionModule.Read(reader);
			if (HasTriggerModule(reader.Version))
			{
				TriggerModule.Read(reader);
			}
			SubModule.Read(reader);
			if (HasLightsModule(reader.Version))
			{
				LightsModule.Read(reader);
				TrailModule.Read(reader);
			}
			if (HasCustomDataModule(reader.Version))
			{
				CustomDataModule.Read(reader);
			}
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}
			
			yield return context.FetchDependency(MoveWithCustomTransform, MoveWithCustomTransformName);
			foreach (PPtr<Object> asset in context.FetchDependencies(CollisionModule, CollisionModuleName))
			{
				yield return asset;
			}
			foreach (PPtr<Object> asset in context.FetchDependencies(SubModule, SubModuleName))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
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
			return HasAutoRandomSeed(version) ? AutoRandomSeed : true;
		}
		public bool GetUseRigidbodyForVelocity(Version version)
		{
			return HasUseRigidbodyForVelocity(version) ? UseRigidbodyForVelocity : true;
		}
		private MinMaxCurve GetStartDelay(Version version)
		{
			return HasStartDelaySingle(version) ? new MinMaxCurve(StartDelaySingle) : StartDelay;
		}
		private ParticleSystemScalingMode GetScalingMode(Version version)
		{
			return HasScalingMode(version) ? ScalingMode : ParticleSystemScalingMode.Shape;
		}
		private InheritVelocityModule GetInheritVelocityModule(Version version)
		{
			return HasInheritVelocityModule(version) ? InheritVelocityModule : new InheritVelocityModule(InitialModule.InheritVelocity);
		}
		private ExternalForcesModule GetExternalForcesModule(Version version)
		{
			return HasExternalForcesModule(version) ? ExternalForcesModule : new ExternalForcesModule(true);
		}
		public NoiseModule GetNoiseModule(Version version)
		{
			return HasNoiseModule(version) ? NoiseModule : new NoiseModule(true);
		}
		public TriggerModule GetTriggerModule(Version version)
		{
			return HasTriggerModule(version) ? TriggerModule : new TriggerModule(true);
		}
		public LightsModule GetLightsModule(Version version)
		{
			return HasLightsModule(version) ? LightsModule : new LightsModule(true);
		}
		public TrailModule GetTrailModule(Version version)
		{
			return HasLightsModule(version) ? TrailModule : new TrailModule(true);
		}
		public CustomDataModule GetCustomDataModule(Version version)
		{
			return HasCustomDataModule(version) ? CustomDataModule : new CustomDataModule(true);
		}

		public float LengthInSec { get; set; }
		public float StartDelaySingle { get; set; }
		/// <summary>
		/// Speed previously
		/// </summary>
		public float SimulationSpeed { get; set; }
		public ParticleSystemStopAction StopAction { get; set; }
		public ParticleSystemCullingMode CullingMode { get; set; }
		public ParticleSystemRingBufferMode RingBufferMode { get; set; }
		public Vector2f RingBufferLoopRange { get; set; }
		public bool Looping { get; set; }
		public bool Prewarm { get; set; }
		public bool PlayOnAwake { get; set; }
		public bool UseUnscaledTime { get; set; }
		public bool AutoRandomSeed { get; set; }
		public bool UseRigidbodyForVelocity { get; set; }
		public ParticleSystemSimulationSpace MoveWithTransform { get; set; }
		public ParticleSystemScalingMode ScalingMode { get; set; }
		public int RandomSeed { get; set; }
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
