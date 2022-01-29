using AssetRipper.Core.Classes.ParticleSystem.Curve;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.ParticleSystem
{
	public sealed class InitialModule : ParticleSystemModule
	{
		public static int ToSerializedVersion(UnityVersion version)
		{
			if (version.IsGreaterEqual(5, 5))
			{
				return 3;
			}
			if (version.IsGreaterEqual(5, 3))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasSizeAxes(UnityVersion version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasRotationAxes(UnityVersion version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// Less than 5.3.0
		/// </summary>
		public static bool HasInheritVelocity(UnityVersion version) => version.IsLess(5, 3);
		/// <summary>
		/// 2021 and greater
		/// </summary>
		public static bool HasCustomEmitterVelocity(UnityVersion version) => version.IsGreaterEqual(2021);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasRandomizeRotationDirection(UnityVersion version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		public static bool HasGravityModifierSingle(UnityVersion version) => version.IsLess(5, 5);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasSize3D(UnityVersion version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasRotation3D(UnityVersion version) => version.IsGreaterEqual(5, 3);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			StartLifetime.Read(reader);
			StartSpeed.Read(reader);
			StartColor.Read(reader);
			StartSize.Read(reader);
			if (HasSizeAxes(reader.Version))
			{
				StartSizeY.Read(reader);
				StartSizeZ.Read(reader);
			}
			if (HasRotationAxes(reader.Version))
			{
				StartRotationX.Read(reader);
				StartRotationY.Read(reader);
			}
			StartRotation.Read(reader);

			if (HasRandomizeRotationDirection(reader.Version))
			{
				RandomizeRotationDirection = reader.ReadSingle();
			}
			if (HasGravityModifierSingle(reader.Version))
			{
				float gravityModifier = reader.ReadSingle();
				GravityModifier = new MinMaxCurve(gravityModifier);
			}
			if (HasInheritVelocity(reader.Version))
			{
				InheritVelocity = reader.ReadSingle();
			}
			MaxNumParticles = reader.ReadInt32();

			if (HasCustomEmitterVelocity(reader.Version))
			{
				CustomEmitterVelocity.Read(reader);
			}


			if (HasSize3D(reader.Version))
			{
				Size3D = reader.ReadBoolean();
			}
			if (HasRotation3D(reader.Version))
			{
				Rotation3D = reader.ReadBoolean();
				reader.AlignStream();
			}

			if (!HasGravityModifierSingle(reader.Version))
			{
				GravityModifier.Read(reader);
			}
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.InsertSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(StartLifetimeName, StartLifetime.ExportYAML(container));
			node.Add(StartSpeedName, StartSpeed.ExportYAML(container));
			node.Add(StartColorName, StartColor.ExportYAML(container));
			node.Add(StartSizeName, StartSize.ExportYAML(container));
			node.Add(StartSizeYName, GetStartSizeY(container.Version).ExportYAML(container));
			node.Add(StartSizeZName, GetStartSizeZ(container.Version).ExportYAML(container));
			node.Add(StartRotationXName, GetStartRotationX(container.Version).ExportYAML(container));
			node.Add(StartRotationYName, GetStartRotationY(container.Version).ExportYAML(container));
			node.Add(StartRotationName, StartRotation.ExportYAML(container));
			node.Add(RandomizeRotationDirectionName, RandomizeRotationDirection);
			node.Add(MaxNumParticlesName, MaxNumParticles);
			if (HasCustomEmitterVelocity(container.ExportVersion))
			{
				node.Add(CustomEmitterVelocityName, CustomEmitterVelocity.ExportYAML(container));
			}
			node.Add(Size3DName, Size3D);
			node.Add(Rotation3DName, Rotation3D);
			node.Add(GravityModifierName, GravityModifier.ExportYAML(container));
			return node;
		}

		private MinMaxCurve GetStartSizeY(UnityVersion version)
		{
			return HasSizeAxes(version) ? StartSizeY : new MinMaxCurve(1.0f);
		}
		private MinMaxCurve GetStartSizeZ(UnityVersion version)
		{
			return HasSizeAxes(version) ? StartSizeZ : new MinMaxCurve(1.0f);
		}
		private MinMaxCurve GetStartRotationX(UnityVersion version)
		{
			return HasRotationAxes(version) ? StartRotationX : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetStartRotationY(UnityVersion version)
		{
			return HasRotationAxes(version) ? StartRotationY : new MinMaxCurve(0.0f);
		}

		public float RandomizeRotationDirection { get; set; }
		public float InheritVelocity { get; set; }
		public int MaxNumParticles { get; set; }
		public Vector3f CustomEmitterVelocity { get; set; } = new();
		public bool Size3D { get; set; }
		public bool Rotation3D { get; set; }

		public const string StartLifetimeName = "startLifetime";
		public const string StartSpeedName = "startSpeed";
		public const string StartColorName = "startColor";
		public const string StartSizeName = "startSize";
		public const string StartSizeYName = "startSizeY";
		public const string StartSizeZName = "startSizeZ";
		public const string StartRotationXName = "startRotationX";
		public const string StartRotationYName = "startRotationY";
		public const string StartRotationName = "startRotation";
		public const string RandomizeRotationDirectionName = "randomizeRotationDirection";
		public const string MaxNumParticlesName = "maxNumParticles";
		public const string CustomEmitterVelocityName = "customEmitterVelocity";
		public const string Size3DName = "size3D";
		public const string Rotation3DName = "rotation3D";
		public const string GravityModifierName = "gravityModifier";

		public MinMaxCurve StartLifetime = new();
		public MinMaxCurve StartSpeed = new();
		public MinMaxGradient.MinMaxGradient StartColor = new();
		public MinMaxCurve StartSize = new();
		public MinMaxCurve StartSizeY = new();
		public MinMaxCurve StartSizeZ = new();
		public MinMaxCurve StartRotationX = new();
		public MinMaxCurve StartRotationY = new();
		public MinMaxCurve StartRotation = new();
		public MinMaxCurve GravityModifier = new();
	}
}
