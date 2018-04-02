using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public class CollisionModule : ParticleSystemModule
	{
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadCollisionMode(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool IsReadColliderForce(Version version)
		{
			return version.IsGreaterEqual(2017);
		}
		/// <summary>
		/// Less than 5.3.0
		/// </summary>
		public static bool IsReadDampenSingle(Version version)
		{
			return version.IsLess(5, 3);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadMaxKillSpeed(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadRadiusScale(Version version)
		{
			return version.IsGreaterEqual(4);
		}
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadQuality(Version version)
		{
			return version.IsGreaterEqual(4);
		}
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadMaxCollisionShapes(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		/// <summary>
		/// 4.2.0 and greater
		/// </summary>
		public static bool IsReadCollisionMessages(Version version)
		{
			return version.IsGreaterEqual(4, 2);
		}
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadCollidesWithDynamic(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 3;
			}

			if (version.IsGreaterEqual(5, 4))
			{
				return 3;
			}
			if (version.IsGreaterEqual(5, 3))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			
			Type = stream.ReadInt32();
			if (IsReadCollisionMode(stream.Version))
			{
				CollisionMode = stream.ReadInt32();
			}
			if (IsReadColliderForce(stream.Version))
			{
				ColliderForce = stream.ReadSingle();
				MultiplyColliderForceByParticleSize = stream.ReadBoolean();
				MultiplyColliderForceByParticleSpeed = stream.ReadBoolean();
				MultiplyColliderForceByCollisionAngle = stream.ReadBoolean();
				stream.AlignStream(AlignType.Align4);
			}
			
			Plane0.Read(stream);
			Plane1.Read(stream);
			Plane2.Read(stream);
			Plane3.Read(stream);
			Plane4.Read(stream);
			Plane5.Read(stream);

			if (IsReadDampenSingle(stream.Version))
			{
				DampenSingle = stream.ReadSingle();
				BounceSingle = stream.ReadSingle();
				EnergyLossOnCollisionSingle = stream.ReadSingle();
			}
			else
			{
				Dampen.Read(stream);
				Bounce.Read(stream);
				EnergyLossOnCollision.Read(stream);
			}

			MinKillSpeed = stream.ReadSingle();
			if (IsReadMaxKillSpeed(stream.Version))
			{
				MaxKillSpeed = stream.ReadSingle();
			}
			if (IsReadRadiusScale(stream.Version))
			{
				RadiusScale = stream.ReadSingle();
				CollidesWith.Read(stream);
			}
			if (IsReadMaxCollisionShapes(stream.Version))
			{
				MaxCollisionShapes = stream.ReadInt32();
			}
			if (IsReadQuality(stream.Version))
			{
				Quality = stream.ReadInt32();
				VoxelSize = stream.ReadSingle();
			}
			if (IsReadCollisionMessages(stream.Version))
			{
				CollisionMessages = stream.ReadBoolean();
			}
			if (IsReadCollidesWithDynamic(stream.Version))
			{
				CollidesWithDynamic = stream.ReadBoolean();
				InteriorCollisions = stream.ReadBoolean();
				stream.AlignStream(AlignType.Align4);
			}
		}

		public override YAMLNode ExportYAML(IAssetsExporter exporter)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(exporter);
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("type", Type);
			node.Add("collisionMode", CollisionMode);
			node.Add("colliderForce", ColliderForce);
			node.Add("multiplyColliderForceByParticleSize", MultiplyColliderForceByParticleSize);
			node.Add("multiplyColliderForceByParticleSpeed", MultiplyColliderForceByParticleSpeed);
			node.Add("multiplyColliderForceByCollisionAngle", MultiplyColliderForceByCollisionAngle);
			node.Add("plane0", Plane0.ExportYAML(exporter));
			node.Add("plane1", Plane1.ExportYAML(exporter));
			node.Add("plane2", Plane2.ExportYAML(exporter));
			node.Add("plane3", Plane3.ExportYAML(exporter));
			node.Add("plane4", Plane4.ExportYAML(exporter));
			node.Add("plane5", Plane5.ExportYAML(exporter));
			node.Add("m_Dampen", Dampen.ExportYAML(exporter));
			node.Add("m_Bounce", Bounce.ExportYAML(exporter));
			node.Add("m_EnergyLossOnCollision", EnergyLossOnCollision.ExportYAML(exporter));
			node.Add("minKillSpeed", MinKillSpeed);
			node.Add("maxKillSpeed", MaxKillSpeed);
			node.Add("radiusScale", RadiusScale);
			node.Add("collidesWith", CollidesWith.ExportYAML(exporter));
			node.Add("maxCollisionShapes", MaxCollisionShapes);
			node.Add("quality", Quality);
			node.Add("voxelSize", VoxelSize);
			node.Add("collisionMessages", CollisionMessages);
			node.Add("collidesWithDynamic", CollidesWithDynamic);
			node.Add("interiorCollisions", InteriorCollisions);
			return node;
		}

		public int Type { get; private set; }
		public int CollisionMode { get; private set; }
		public float ColliderForce { get; private set; }
		public bool MultiplyColliderForceByParticleSize { get; private set; }
		public bool MultiplyColliderForceByParticleSpeed { get; private set; }
		public bool MultiplyColliderForceByCollisionAngle { get; private set; }
		public float DampenSingle { get; private set; }
		public float BounceSingle { get; private set; }
		public float EnergyLossOnCollisionSingle { get; private set; }
		public float MinKillSpeed { get; private set; }
		public float MaxKillSpeed { get; private set; }
		/// <summary>
		/// ParticleRadius previously
		/// </summary>
		public float RadiusScale { get; private set; }
		public int MaxCollisionShapes { get; private set; }
		public int Quality { get; private set; }
		public float VoxelSize { get; private set; }
		public bool CollisionMessages { get; private set; }
		public bool CollidesWithDynamic { get; private set; }
		public bool InteriorCollisions { get; private set; }

		public PPtr<Transform> Plane0;
		public PPtr<Transform> Plane1;
		public PPtr<Transform> Plane2;
		public PPtr<Transform> Plane3;
		public PPtr<Transform> Plane4;
		public PPtr<Transform> Plane5;
		public MinMaxCurve Dampen;
		public MinMaxCurve Bounce;
		public MinMaxCurve EnergyLossOnCollision;
		public BitField CollidesWith;
	}
}
