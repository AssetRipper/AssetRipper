using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct CollisionModule : IAssetReadable, IYAMLExportable
	{
		/*private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}*/

		public void Read(AssetStream stream)
		{
			Enabled = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
			
			Type = stream.ReadInt32();
			CollisionMode = stream.ReadInt32();
			ColliderForce = stream.ReadSingle();
			MultiplyColliderForceByParticleSize = stream.ReadBoolean();
			MultiplyColliderForceByParticleSpeed = stream.ReadBoolean();
			MultiplyColliderForceByCollisionAngle = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
			
			Plane0.Read(stream);
			Plane1.Read(stream);
			Plane2.Read(stream);
			Plane3.Read(stream);
			Plane4.Read(stream);
			Plane5.Read(stream);
			Dampen.Read(stream);
			Bounce.Read(stream);
			EnergyLossOnCollision.Read(stream);
			MinKillSpeed = stream.ReadSingle();
			MaxKillSpeed = stream.ReadSingle();
			RadiusScale = stream.ReadSingle();
			CollidesWith.Read(stream);
			MaxCollisionShapes = stream.ReadInt32();
			Quality = stream.ReadInt32();
			VoxelSize = stream.ReadSingle();
			CollisionMessages = stream.ReadBoolean();
			CollidesWithDynamic = stream.ReadBoolean();
			InteriorCollisions = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
			
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			//node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("enabled", Enabled);
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

		public bool Enabled { get; private set; }
		public int Type { get; private set; }
		public int CollisionMode { get; private set; }
		public float ColliderForce { get; private set; }
		public bool MultiplyColliderForceByParticleSize { get; private set; }
		public bool MultiplyColliderForceByParticleSpeed { get; private set; }
		public bool MultiplyColliderForceByCollisionAngle { get; private set; }
		public float MinKillSpeed { get; private set; }
		public float MaxKillSpeed { get; private set; }
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
