using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.ParticleSystem.Curve;
using AssetRipper.Core.Classes.ParticleSystem.Emission;
using AssetRipper.Core.Classes.ParticleSystem.InheritVelocity;
using AssetRipper.Core.Classes.ParticleSystem.Shape;
using AssetRipper.Core.Classes.ParticleSystem.SubEmitter;
using AssetRipper.Core.Classes.ParticleSystem.Trigger;
using AssetRipper.Core.Classes.ParticleSystem.UV;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.ParticleSystem
{
	public sealed class ParticleSystem : Component
	{
		public ParticleSystem(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			if (version.IsGreaterEqual(2018, 3))
			{
				return 6;
			}
			if (version.IsGreaterEqual(5, 5))
			{
				return 5;
			}
			if (version.IsGreaterEqual(5, 4, 0, UnityVersionType.Patch, 4))
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
		public static bool HasStartDelaySingle(UnityVersion version) => version.IsLess(5, 3);
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool HasStopAction(UnityVersion version) => version.IsGreaterEqual(2017, 2);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasCullingMode(UnityVersion version) => version.IsGreaterEqual(2018, 3);
		/// <summary>
		/// 2021 and greater
		/// </summary>
		public static bool HasEmitterVelocityMode(UnityVersion version) => version.IsGreaterEqual(2021);
		/// <summary>
		/// 2017.1.0b2 and greater
		/// </summary>
		public static bool HasUseUnscaledTime(UnityVersion version) => version.IsGreaterEqual(2017, 1, 0, UnityVersionType.Beta, 2);
		/// <summary>
		/// 5.4.0p4 and greater
		/// </summary>
		public static bool HasAutoRandomSeed(UnityVersion version) => version.IsGreaterEqual(5, 4, 0, UnityVersionType.Patch, 4);
		/// <summary>
		/// 2017.1.0f1 and greater but less than 2021
		/// </summary>
		public static bool HasUseRigidbodyForVelocity(UnityVersion version) => version.IsGreaterEqual(2017, 1, 0, UnityVersionType.Final) && version.IsLess(2021);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasMoveWithCustomTransform(UnityVersion version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasScalingMode(UnityVersion version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasInheritVelocityModule(UnityVersion version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasExternalForcesModule(UnityVersion version) => version.IsGreaterEqual(4);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasNoiseModule(UnityVersion version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasTriggerModule(UnityVersion version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasLightsModule(UnityVersion version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasCustomDataModule(UnityVersion version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// 2020 and greater
		/// </summary>
		private static bool HasLifetimeByEmitterSpeedModule(UnityVersion version) => version.IsGreaterEqual(2020);

		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		private static bool IsStartDelayFirst(UnityVersion version) => version.IsLess(5, 5);
		/// <summary>
		/// Less than 5.4.0p4
		/// </summary>
		private static bool IsRandomSeedFirst(UnityVersion version) => version.IsLess(5, 4, 0, UnityVersionType.Patch, 4);
		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		private static bool IsMoveWithTransformBool(UnityVersion version) => version.IsLess(5, 5);
		/// <summary>
		/// 5.4.0p4 and greater
		/// </summary>
		private static bool IsAlign(UnityVersion version) => version.IsGreaterEqual(5, 4, 0, UnityVersionType.Patch, 4);

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

			if (HasEmitterVelocityMode(reader.Version))
			{
				EmitterVelocityMode = reader.ReadInt32();
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

			if (HasLifetimeByEmitterSpeedModule(reader.Version))
			{
				LifetimeByEmitterSpeedModule.Read(reader);
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

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(MoveWithCustomTransform, MoveWithCustomTransformName);
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromDependent(CollisionModule, CollisionModuleName))
			{
				yield return asset;
			}
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromDependent(SubModule, SubModuleName))
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
			if (HasEmitterVelocityMode(container.ExportVersion))
			{
				node.Add(EmitterVelocityModeName, EmitterVelocityMode);
			}
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
			if (HasLifetimeByEmitterSpeedModule(container.ExportVersion))
			{
				node.Add(LifetimeByEmitterSpeedModuleName, LifetimeByEmitterSpeedModule.ExportYAML(container));
			}
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

		private bool GetAutoRandomSeed(UnityVersion version)
		{
			return HasAutoRandomSeed(version) ? AutoRandomSeed : true;
		}
		public bool GetUseRigidbodyForVelocity(UnityVersion version)
		{
			return HasUseRigidbodyForVelocity(version) ? UseRigidbodyForVelocity : true;
		}
		private MinMaxCurve GetStartDelay(UnityVersion version)
		{
			return HasStartDelaySingle(version) ? new MinMaxCurve(StartDelaySingle) : StartDelay;
		}
		private ParticleSystemScalingMode GetScalingMode(UnityVersion version)
		{
			return HasScalingMode(version) ? ScalingMode : ParticleSystemScalingMode.Shape;
		}
		private InheritVelocityModule GetInheritVelocityModule(UnityVersion version)
		{
			return HasInheritVelocityModule(version) ? InheritVelocityModule : new InheritVelocityModule(InitialModule.InheritVelocity);
		}
		private ExternalForcesModule GetExternalForcesModule(UnityVersion version)
		{
			return HasExternalForcesModule(version) ? ExternalForcesModule : new ExternalForcesModule(true);
		}
		public NoiseModule.NoiseModule GetNoiseModule(UnityVersion version)
		{
			return HasNoiseModule(version) ? NoiseModule : new NoiseModule.NoiseModule(true);
		}
		public TriggerModule GetTriggerModule(UnityVersion version)
		{
			return HasTriggerModule(version) ? TriggerModule : new TriggerModule(true);
		}
		public LightsModule GetLightsModule(UnityVersion version)
		{
			return HasLightsModule(version) ? LightsModule : new LightsModule(true);
		}
		public TrailModule.TrailModule GetTrailModule(UnityVersion version)
		{
			return HasLightsModule(version) ? TrailModule : new TrailModule.TrailModule(true);
		}
		public CustomDataModule.CustomDataModule GetCustomDataModule(UnityVersion version)
		{
			return HasCustomDataModule(version) ? CustomDataModule : new CustomDataModule.CustomDataModule(true);
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
		public int EmitterVelocityMode { get; set; }
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
		public LifetimeByEmitterSpeedModule LifetimeByEmitterSpeedModule { get; } = new LifetimeByEmitterSpeedModule();
		public ExternalForcesModule ExternalForcesModule { get; } = new ExternalForcesModule();
		public ClampVelocityModule ClampVelocityModule { get; } = new ClampVelocityModule();
		public NoiseModule.NoiseModule NoiseModule { get; } = new NoiseModule.NoiseModule();
		public SizeBySpeedModule SizeBySpeedModule { get; } = new SizeBySpeedModule();
		public RotationBySpeedModule RotationBySpeedModule { get; } = new RotationBySpeedModule();
		public ColorBySpeedModule ColorBySpeedModule { get; } = new ColorBySpeedModule();
		public CollisionModule.CollisionModule CollisionModule { get; } = new CollisionModule.CollisionModule();
		public TriggerModule TriggerModule { get; } = new TriggerModule();
		public SubModule SubModule { get; } = new SubModule();
		public LightsModule LightsModule { get; } = new LightsModule();
		public TrailModule.TrailModule TrailModule { get; } = new TrailModule.TrailModule();
		public CustomDataModule.CustomDataModule CustomDataModule { get; } = new CustomDataModule.CustomDataModule();

		public const string LengthInSecName = "lengthInSec";
		public const string SimulationSpeedName = "simulationSpeed";
		public const string StopActionName = "stopAction";
		public const string EmitterVelocityModeName = "emitterVelocityMode";
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
		public const string LifetimeByEmitterSpeedModuleName = "LifetimeByEmitterSpeedModule";
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

		public MinMaxCurve StartDelay = new();
		public PPtr<Transform> MoveWithCustomTransform = new();
	}
}
