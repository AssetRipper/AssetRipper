using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Classes.Misc;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class CollisionModule : ParticleSystemModule, IDependent
	{
		public static int ToSerializedVersion(Version version)
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

		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasCollisionMode(Version version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool HasColliderForce(Version version) => version.IsGreaterEqual(2017);
		/// <summary>
		/// Less than 5.3.0
		/// </summary>
		public static bool HasDampenSingle(Version version) => version.IsLess(5, 3);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasMaxKillSpeed(Version version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasRadiusScale(Version version) => version.IsGreaterEqual(4);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasQuality(Version version) => version.IsGreaterEqual(4);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasMaxCollisionShapes(Version version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 4.2.0 and greater
		/// </summary>
		public static bool HasCollisionMessages(Version version) => version.IsGreaterEqual(4, 2);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasCollidesWithDynamic(Version version) => version.IsGreaterEqual(5, 3);
		
		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			Type = (ParticleSystemCollisionType)reader.ReadInt32();
			if (HasCollisionMode(reader.Version))
			{
				CollisionMode = (ParticleSystemCollisionMode)reader.ReadInt32();
			}
			if (HasColliderForce(reader.Version))
			{
				ColliderForce = reader.ReadSingle();
				MultiplyColliderForceByParticleSize = reader.ReadBoolean();
				MultiplyColliderForceByParticleSpeed = reader.ReadBoolean();
				MultiplyColliderForceByCollisionAngle = reader.ReadBoolean();
				reader.AlignStream();
			}
			
			Plane0.Read(reader);
			Plane1.Read(reader);
			Plane2.Read(reader);
			Plane3.Read(reader);
			Plane4.Read(reader);
			Plane5.Read(reader);

			if (HasDampenSingle(reader.Version))
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
			if (HasMaxKillSpeed(reader.Version))
			{
				MaxKillSpeed = reader.ReadSingle();
			}
			if (HasRadiusScale(reader.Version))
			{
				RadiusScale = reader.ReadSingle();
				CollidesWith.Read(reader);
			}
			if (HasMaxCollisionShapes(reader.Version))
			{
				MaxCollisionShapes = reader.ReadInt32();
			}
			if (HasQuality(reader.Version))
			{
				Quality = (ParticleSystemCollisionQuality)reader.ReadInt32();
				VoxelSize = reader.ReadSingle();
			}
			if (HasCollisionMessages(reader.Version))
			{
				CollisionMessages = reader.ReadBoolean();
			}
			if (HasCollidesWithDynamic(reader.Version))
			{
				CollidesWithDynamic = reader.ReadBoolean();
				InteriorCollisions = reader.ReadBoolean();
				reader.AlignStream();
			}
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
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
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
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
			return HasColliderForce(version) ? MultiplyColliderForceByCollisionAngle : true;
		}
		private float GetExportMaxKillSpeed(Version version)
		{
			return HasMaxKillSpeed(version) ? MaxKillSpeed : 10000;
		}
		private float GetExportRadiusScale(Version version)
		{
			return HasRadiusScale(version) ? RadiusScale : 1.0f;
		}
		private BitField GetExportCollidesWith(Version version)
		{
			return HasRadiusScale(version) ? CollidesWith : new BitField(uint.MaxValue);
		}
		private int GetExportMaxCollisionShapes(Version version)
		{
			return HasMaxCollisionShapes(version) ? MaxCollisionShapes : 256;
		}
		private float GetExportVoxelSize(Version version)
		{
			return HasQuality(version) ? VoxelSize : 0.5f;
		}
		private bool GetExportCollidesWithDynamic(Version version)
		{
			return HasCollidesWithDynamic(version) ? CollidesWithDynamic : true;
		}

		public ParticleSystemCollisionType Type { get; set; }
		public ParticleSystemCollisionMode CollisionMode { get; set; }
		public float ColliderForce { get; set; }
		public bool MultiplyColliderForceByParticleSize { get; set; }
		public bool MultiplyColliderForceByParticleSpeed { get; set; }
		public bool MultiplyColliderForceByCollisionAngle { get; set; }
		public float MinKillSpeed { get; set; }
		public float MaxKillSpeed { get; set; }
		/// <summary>
		/// ParticleRadius previously
		/// </summary>
		public float RadiusScale { get; set; }
		public int MaxCollisionShapes { get; set; }
		public ParticleSystemCollisionQuality Quality { get; set; }
		public float VoxelSize { get; set; }
		public bool CollisionMessages { get; set; }
		public bool CollidesWithDynamic { get; set; }
		public bool InteriorCollisions { get; set; }

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
