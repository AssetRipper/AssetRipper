using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class InitialModule : ParticleSystemModule
	{
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadSizeAxes(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadRotationAxes(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		/// <summary>
		/// Less than 5.3.0
		/// </summary>
		public static bool IsReadInheritVelocity(Version version)
		{
			return version.IsLess(5, 3);
		}
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadRandomizeRotationDirection(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		public static bool IsReadGravityModifierSingle(Version version)
		{
			return version.IsLess(5, 5);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadSize3D(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadRotation3D(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		
		private static int GetSerializedVersion(Version version)
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

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			StartLifetime.Read(reader);
			StartSpeed.Read(reader);
			StartColor.Read(reader);
			StartSize.Read(reader);
			if (IsReadSizeAxes(reader.Version))
			{
				StartSizeY.Read(reader);
				StartSizeZ.Read(reader);
			}
			if (IsReadRotationAxes(reader.Version))
			{
				StartRotationX.Read(reader);
				StartRotationY.Read(reader);
			}
			StartRotation.Read(reader);
			
			if (IsReadRandomizeRotationDirection(reader.Version))
			{
				RandomizeRotationDirection = reader.ReadSingle();
			}
			if (IsReadGravityModifierSingle(reader.Version))
			{
				float gravityModifier = reader.ReadSingle();
				GravityModifier = new MinMaxCurve(gravityModifier);
			}
			if (IsReadInheritVelocity(reader.Version))
			{
				InheritVelocity = reader.ReadSingle();
			}
			MaxNumParticles = reader.ReadInt32();
			if (IsReadSize3D(reader.Version))
			{
				Size3D = reader.ReadBoolean();
			}
			if (IsReadRotation3D(reader.Version))
			{
				Rotation3D = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
			
			if (!IsReadGravityModifierSingle(reader.Version))
			{
				GravityModifier.Read(reader);
			}
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.InsertSerializedVersion(GetSerializedVersion(container.ExportVersion));
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
			return IsReadSizeAxes(version) ? StartSizeY : new MinMaxCurve(1.0f);
		}
		private MinMaxCurve GetStartSizeZ(Version version)
		{
			return IsReadSizeAxes(version) ? StartSizeZ : new MinMaxCurve(1.0f);
		}
		private MinMaxCurve GetStartRotationX(Version version)
		{
			return IsReadRotationAxes(version) ? StartRotationX : new MinMaxCurve(0.0f);
		}
		private MinMaxCurve GetStartRotationY(Version version)
		{
			return IsReadRotationAxes(version) ? StartRotationY : new MinMaxCurve(0.0f);
		}

		public float RandomizeRotationDirection { get; private set; }
		public float InheritVelocity { get; private set; }
		public int MaxNumParticles { get; private set; }
		public bool Size3D { get; private set; }
		public bool Rotation3D { get; private set; }

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
