using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Classes.Misc;

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

		public IEnumerable<Object> FetchDependencies(IDependencyContext context)
		{
			yield return context.FetchDependency(Plane0, Plane0Name);
			yield return context.FetchDependency(Plane1, Plane1Name);
			yield return context.FetchDependency(Plane2, Plane2Name);
			yield return context.FetchDependency(Plane3, Plane3Name);
			yield return context.FetchDependency(Plane4, Plane4Name);
			yield return context.FetchDependency(Plane5, Plane5Name);
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(TypeName, (int)Type);
			node.Add(CollisionModeName, (int)CollisionMode);
			node.Add(ColliderForceName, ColliderForce);
			node.Add(MultiplyColliderForceByParticleSizeName, MultiplyColliderForceByParticleSize);
			node.Add(MultiplyColliderForceByParticleSpeedName, MultiplyColliderForceByParticleSpeed);
			node.Add(MultiplyColliderForceByCollisionAngleName, GetExportMultiplyColliderForceByCollisionAngle(container.Version));
			node.Add(Plane0Name, Plane0.ExportYAML(container));
			node.Add(Plane1Name, Plane1.ExportYAML(container));
			node.Add(Plane2Name, Plane2.ExportYAML(container));
			node.Add(Plane3Name, Plane3.ExportYAML(container));
			node.Add(Plane4Name, Plane4.ExportYAML(container));
			node.Add(Plane5Name, Plane5.ExportYAML(container));
			node.Add(DampenName, Dampen.ExportYAML(container));
			node.Add(BounceName, Bounce.ExportYAML(container));
			node.Add(EnergyLossOnCollisionName, EnergyLossOnCollision.ExportYAML(container));
			node.Add(MinKillSpeedName, MinKillSpeed);
			node.Add(MaxKillSpeedName, GetExportMaxKillSpeed(container.Version));
			node.Add(RadiusScaleName, GetExportRadiusScale(container.Version));
			node.Add(CollidesWithName, GetExportCollidesWith(container.Version).ExportYAML(container));
			node.Add(MaxCollisionShapesName, GetExportMaxCollisionShapes(container.Version));
			node.Add(QualityName, (int)Quality);
			node.Add(VoxelSizeName, GetExportVoxelSize(container.Version));
			node.Add(CollisionMessagesName, CollisionMessages);
			node.Add(CollidesWithDynamicName, GetExportCollidesWithDynamic(container.Version));
			node.Add(InteriorCollisionsName, InteriorCollisions);
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

		public const string TypeName = "type";
		public const string CollisionModeName = "collisionMode";
		public const string ColliderForceName = "colliderForce";
		public const string MultiplyColliderForceByParticleSizeName = "multiplyColliderForceByParticleSize";
		public const string MultiplyColliderForceByParticleSpeedName = "multiplyColliderForceByParticleSpeed";
		public const string MultiplyColliderForceByCollisionAngleName = "multiplyColliderForceByCollisionAngle";
		public const string Plane0Name = "plane0";
		public const string Plane1Name = "plane1";
		public const string Plane2Name = "plane2";
		public const string Plane3Name = "plane3";
		public const string Plane4Name = "plane4";
		public const string Plane5Name = "plane5";
		public const string DampenName = "m_Dampen";
		public const string BounceName = "m_Bounce";
		public const string EnergyLossOnCollisionName = "m_EnergyLossOnCollision";
		public const string MinKillSpeedName = "minKillSpeed";
		public const string MaxKillSpeedName = "maxKillSpeed";
		public const string RadiusScaleName = "radiusScale";
		public const string CollidesWithName = "collidesWith";
		public const string MaxCollisionShapesName = "maxCollisionShapes";
		public const string QualityName = "quality";
		public const string VoxelSizeName = "voxelSize";
		public const string CollisionMessagesName = "collisionMessages";
		public const string CollidesWithDynamicName = "collidesWithDynamic";
		public const string InteriorCollisionsName = "interiorCollisions";

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
