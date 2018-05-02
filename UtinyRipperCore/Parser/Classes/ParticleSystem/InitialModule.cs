using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public class InitialModule : ParticleSystemModule
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
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 3;
			}

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

		private MinMaxCurve GetExportGravityModifier(Version version)
		{
			if(IsReadGravityModifierSingle(version))
			{
				return new MinMaxCurve(GravityModifierSingle);
			}
			else
			{
				return GravityModifier;
			}
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			
			StartLifetime.Read(stream);
			StartSpeed.Read(stream);
			StartColor.Read(stream);
			StartSize.Read(stream);
			if (IsReadSizeAxes(stream.Version))
			{
				StartSizeY.Read(stream);
				StartSizeZ.Read(stream);
			}
			if (IsReadRotationAxes(stream.Version))
			{
				StartRotationX.Read(stream);
				StartRotationY.Read(stream);
			}
			StartRotation.Read(stream);
			
			if (IsReadRandomizeRotationDirection(stream.Version))
			{
				RandomizeRotationDirection = stream.ReadSingle();
			}
			if (IsReadGravityModifierSingle(stream.Version))
			{
				GravityModifierSingle = stream.ReadSingle();
			}
			if (IsReadInheritVelocity(stream.Version))
			{
				InheritVelocity = stream.ReadSingle();
			}
			MaxNumParticles = stream.ReadInt32();
			if (IsReadSize3D(stream.Version))
			{
				Size3D = stream.ReadBoolean();
			}
			if (IsReadRotation3D(stream.Version))
			{
				Rotation3D = stream.ReadBoolean();
				stream.AlignStream(AlignType.Align4);
			}
			
			if (!IsReadGravityModifierSingle(stream.Version))
			{
				GravityModifier.Read(stream);
			}
		}

		public override YAMLNode ExportYAML(IAssetsExporter exporter)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(exporter);
			node.InsertSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("startLifetime", StartLifetime.ExportYAML(exporter));
			node.Add("startSpeed", StartSpeed.ExportYAML(exporter));
			node.Add("startColor", StartColor.ExportYAML(exporter));
			node.Add("startSize", StartSize.ExportYAML(exporter));
			node.Add("startSizeY", StartSizeY.ExportYAML(exporter));
			node.Add("startSizeZ", StartSizeZ.ExportYAML(exporter));
			node.Add("startRotationX", StartRotationX.ExportYAML(exporter));
			node.Add("startRotationY", StartRotationY.ExportYAML(exporter));
			node.Add("startRotation", StartRotation.ExportYAML(exporter));
			node.Add("randomizeRotationDirection", RandomizeRotationDirection);
			node.Add("maxNumParticles", MaxNumParticles);
			node.Add("size3D", Size3D);
			node.Add("rotation3D", Rotation3D);
			node.Add("gravityModifier", GravityModifier.ExportYAML(exporter));
			return node;
		}
		
		public float RandomizeRotationDirection { get; private set; }
		public float GravityModifierSingle { get; private set; }
		public float InheritVelocity { get; private set; }
		public int MaxNumParticles { get; private set; }
		public bool Size3D { get; private set; }
		public bool Rotation3D { get; private set; }

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
