using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class CollisionModule : ParticleSystemModule, IDependent
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
		
		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			Type = (ParticleSystemCollisionType)reader.ReadInt32();
			if (IsReadCollisionMode(reader.Version))
			{
				CollisionMode = (ParticleSystemCollisionMode)reader.ReadInt32();
			}
			if (IsReadColliderForce(reader.Version))
			{
				ColliderForce = reader.ReadSingle();
				MultiplyColliderForceByParticleSize = reader.ReadBoolean();
				MultiplyColliderForceByParticleSpeed = reader.ReadBoolean();
				MultiplyColliderForceByCollisionAngle = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
			
			Plane0.Read(reader);
			Plane1.Read(reader);
			Plane2.Read(reader);
			Plane3.Read(reader);
			Plane4.Read(reader);
			Plane5.Read(reader);

			if (IsReadDampenSingle(reader.Version))
			{
				float dampenSingle = reader.ReadSingle();
				float bounceSingle = reader.ReadSingle();
				float energyLossOnCollisionSingle = reader.ReadSingle();
				Dampen = new MinMaxCurve(dampenSingle);
				Bounce = new MinMaxCurve(bounceSingle);
				EnergyLossOnCollision = new MinMaxCurve(energyLossOnCollisionSingle);
			}
			else
			{
				Dampen.Read(reader);
				Bounce.Read(reader);
				EnergyLossOnCollision.Read(reader);
			}

			MinKillSpeed = reader.ReadSingle();
			if (IsReadMaxKillSpeed(reader.Version))
			{
				MaxKillSpeed = reader.ReadSingle();
			}
			if (IsReadRadiusScale(reader.Version))
			{
				RadiusScale = reader.ReadSingle();
				CollidesWith.Read(reader);
			}
			if (IsReadMaxCollisionShapes(reader.Version))
			{
				MaxCollisionShapes = reader.ReadInt32();
			}
			if (IsReadQuality(reader.Version))
			{
				Quality = (ParticleSystemCollisionQuality)reader.ReadInt32();
				VoxelSize = reader.ReadSingle();
			}
			if (IsReadCollisionMessages(reader.Version))
			{
				CollisionMessages = reader.ReadBoolean();
			}
			if (IsReadCollidesWithDynamic(reader.Version))
			{
				CollidesWithDynamic = reader.ReadBoolean();
				InteriorCollisions = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Plane0.FetchDependency(file, isLog, () => nameof(CollisionModule), "m_Plane0");
			yield return Plane1.FetchDependency(file, isLog, () => nameof(CollisionModule), "m_Plane1");
			yield return Plane2.FetchDependency(file, isLog, () => nameof(CollisionModule), "m_Plane2");
			yield return Plane3.FetchDependency(file, isLog, () => nameof(CollisionModule), "m_Plane3");
			yield return Plane4.FetchDependency(file, isLog, () => nameof(CollisionModule), "m_Plane4");
			yield return Plane5.FetchDependency(file, isLog, () => nameof(CollisionModule), "m_Plane5");
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("type", (int)Type);
			node.Add("collisionMode", (int)CollisionMode);
			node.Add("colliderForce", ColliderForce);
			node.Add("multiplyColliderForceByParticleSize", MultiplyColliderForceByParticleSize);
			node.Add("multiplyColliderForceByParticleSpeed", MultiplyColliderForceByParticleSpeed);
			node.Add("multiplyColliderForceByCollisionAngle", GetExportMultiplyColliderForceByCollisionAngle(container.Version));
			node.Add("plane0", Plane0.ExportYAML(container));
			node.Add("plane1", Plane1.ExportYAML(container));
			node.Add("plane2", Plane2.ExportYAML(container));
			node.Add("plane3", Plane3.ExportYAML(container));
			node.Add("plane4", Plane4.ExportYAML(container));
			node.Add("plane5", Plane5.ExportYAML(container));
			node.Add("m_Dampen", Dampen.ExportYAML(container));
			node.Add("m_Bounce", Bounce.ExportYAML(container));
			node.Add("m_EnergyLossOnCollision", EnergyLossOnCollision.ExportYAML(container));
			node.Add("minKillSpeed", MinKillSpeed);
			node.Add("maxKillSpeed", GetExportMaxKillSpeed(container.Version));
			node.Add("radiusScale", GetExportRadiusScale(container.Version));
			node.Add("collidesWith", GetExportCollidesWith(container.Version).ExportYAML(container));
			node.Add("maxCollisionShapes", GetExportMaxCollisionShapes(container.Version));
			node.Add("quality", (int)Quality);
			node.Add("voxelSize", GetExportVoxelSize(container.Version));
			node.Add("collisionMessages", CollisionMessages);
			node.Add("collidesWithDynamic", GetExportCollidesWithDynamic(container.Version));
			node.Add("interiorCollisions", InteriorCollisions);
			return node;
		}

		private bool GetExportMultiplyColliderForceByCollisionAngle(Version version)
		{
			return IsReadColliderForce(version) ? MultiplyColliderForceByCollisionAngle : true;
		}
		private float GetExportMaxKillSpeed(Version version)
		{
			return IsReadMaxKillSpeed(version) ? MaxKillSpeed : 10000;
		}
		private float GetExportRadiusScale(Version version)
		{
			return IsReadRadiusScale(version) ? RadiusScale : 1.0f;
		}
		private BitField GetExportCollidesWith(Version version)
		{
			return IsReadRadiusScale(version) ? CollidesWith : new BitField(uint.MaxValue);
		}
		private int GetExportMaxCollisionShapes(Version version)
		{
			return IsReadMaxCollisionShapes(version) ? MaxCollisionShapes : 256;
		}
		private float GetExportVoxelSize(Version version)
		{
			return IsReadQuality(version) ? VoxelSize : 0.5f;
		}
		private bool GetExportCollidesWithDynamic(Version version)
		{
			return IsReadCollidesWithDynamic(version) ? CollidesWithDynamic : true;
		}

		public ParticleSystemCollisionType Type { get; private set; }
		public ParticleSystemCollisionMode CollisionMode { get; private set; }
		public float ColliderForce { get; private set; }
		public bool MultiplyColliderForceByParticleSize { get; private set; }
		public bool MultiplyColliderForceByParticleSpeed { get; private set; }
		public bool MultiplyColliderForceByCollisionAngle { get; private set; }
		public float MinKillSpeed { get; private set; }
		public float MaxKillSpeed { get; private set; }
		/// <summary>
		/// ParticleRadius previously
		/// </summary>
		public float RadiusScale { get; private set; }
		public int MaxCollisionShapes { get; private set; }
		public ParticleSystemCollisionQuality Quality { get; private set; }
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
