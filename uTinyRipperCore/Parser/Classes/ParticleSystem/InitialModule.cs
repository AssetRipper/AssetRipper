using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class InitialModule : ParticleSystemModule
	{
		public static int ToSerializedVersion(Version version)
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
		public static bool HasSizeAxes(Version version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasRotationAxes(Version version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// Less than 5.3.0
		/// </summary>
		public static bool HasInheritVelocity(Version version) => version.IsLess(5, 3);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasRandomizeRotationDirection(Version version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		public static bool HasGravityModifierSingle(Version version) => version.IsLess(5, 5);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasSize3D(Version version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasRotation3D(Version version) => version.IsGreaterEqual(5, 3);

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
			node.Add(Size3DName, Size3D);
			node.Add(Rotation3DName, Rotation3D);
			node.Add(GravityModifierName, GravityModifier.ExportYAML(container));
			return node;
		}

		private MinMaxCurve GetStartSizeY(Version version)
		{
			return HasSizeAxes(version) ? StartSizeY : new MinMaxCurve(1.0f);
		}
		private MinMaxCurve GetStartSizeZ(Version version)
		{
			return HasSizeAxes(version) ? StartSizeZ : new MinMaxCurve(1.0f);
		}
		private MinMaxCurve GetStartRotationX(Version version)
		{
			return HasRotationAxes(version) ? StartRotationX : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetStartRotationY(Version version)
		{
			return HasRotationAxes(version) ? StartRotationY : new MinMaxCurve(0.0f);
		}

		public float RandomizeRotationDirection { get; set; }
		public float InheritVelocity { get; set; }
		public int MaxNumParticles { get; set; }
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
		public const string Size3DName = "size3D";
		public const string Rotation3DName = "rotation3D";
		public const string GravityModifierName = "gravityModifier";

		public MinMaxCurve StartLifetime;
		public MinMaxCurve StartSpeed;
		public MinMaxGradient StartColor;
		public MinMaxCurve StartSize;
		public MinMaxCurve StartSizeY;
		public MinMaxCurve StartSizeZ;
		public MinMaxCurve StartRotationX;
		public MinMaxCurve StartRotationY;
		public MinMaxCurve StartRotation;
		public MinMaxCurve GravityModifier;
	}
}
