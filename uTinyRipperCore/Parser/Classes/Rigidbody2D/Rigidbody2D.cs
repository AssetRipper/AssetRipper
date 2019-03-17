using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Rigidbody2Ds;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class Rigidbody2D : Component
	{
		public Rigidbody2D(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadBodyType(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadUseAutoMass(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadMaterial(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadInterpolate(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}

		/// <summary>
		/// Less than 5.1.0
		/// </summary>
		private static bool IsReadFixedAngle(Version version)
		{
			return version.IsLess(5, 1);
		}
		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		private static bool IsReadIsKinematic(Version version)
		{
			return version.IsLess(5, 5);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 4;
			}

			if (version.IsGreaterEqual(5, 5))
			{
				return 4;
			}
			// there is no 3rd version
			if (version.IsGreaterEqual(5, 1))
			{
				return 2;
			}
			return 4;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadBodyType(reader.Version))
			{
				BodyType = (RigidbodyType2D)reader.ReadInt32();
				Simulated = reader.ReadBoolean();
				UseFullKinematicContacts = reader.ReadBoolean();
			}
			if (IsReadUseAutoMass(reader.Version))
			{
				UseAutoMass = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
			
			Mass = reader.ReadSingle();
			LinearDrag = reader.ReadSingle();
			AngularDrag = reader.ReadSingle();
			GravityScale = reader.ReadSingle();
			if (IsReadMaterial(reader.Version))
			{
				Material.Read(reader);
			}

			if (IsReadFixedAngle(reader.Version))
			{
				bool fixedAngle = reader.ReadBoolean();
				Constraints = fixedAngle ? RigidbodyConstraints2D.FreezeRotation : RigidbodyConstraints2D.None;
			}
			if (IsReadIsKinematic(reader.Version))
			{
				bool isKinematic = reader.ReadBoolean();
				BodyType = isKinematic ? RigidbodyType2D.Kinematic : RigidbodyType2D.Static;
				Interpolate = (RigidbodyInterpolation2D)reader.ReadByte();
				SleepingMode = (RigidbodySleepMode2D)reader.ReadByte();
				CollisionDetection = (CollisionDetectionMode2D)reader.ReadByte();
				reader.AlignStream(AlignType.Align4);
			}
			
			if (IsReadInterpolate(reader.Version))
			{
				Interpolate = (RigidbodyInterpolation2D)reader.ReadInt32();
				SleepingMode = (RigidbodySleepMode2D)reader.ReadInt32();
				CollisionDetection = (CollisionDetectionMode2D)reader.ReadInt32();
			}
			if (!IsReadFixedAngle(reader.Version))
			{
				Constraints = (RigidbodyConstraints2D)reader.ReadInt32();
			}
		}
		
		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
			
			yield return Material.FetchDependency(file, isLog, ToLogString, "m_Material");
		}
		
		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.InsertSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_BodyType", (int)BodyType);
			node.Add("m_Simulated", Simulated);
			node.Add("m_UseFullKinematicContacts", UseFullKinematicContacts);
			node.Add("m_UseAutoMass", UseAutoMass);
			node.Add("m_Mass", Mass);
			node.Add("m_LinearDrag", LinearDrag);
			node.Add("m_AngularDrag", AngularDrag);
			node.Add("m_GravityScale", GravityScale);
			node.Add("m_Material", Material.ExportYAML(container));
			node.Add("m_Interpolate", (int)Interpolate);
			node.Add("m_SleepingMode", (int)SleepingMode);
			node.Add("m_CollisionDetection", (int)CollisionDetection);
			node.Add("m_Constraints", (int)Constraints);
			return node;
		}

		public RigidbodyType2D BodyType { get; private set; }
		public bool Simulated { get; private set; }
		public bool UseFullKinematicContacts { get; private set; }
		public bool UseAutoMass { get; private set; }
		public float Mass  { get; private set; }
		public float LinearDrag { get; private set; }
		public float AngularDrag { get; private set; }
		public float GravityScale { get; private set; }
		public RigidbodyInterpolation2D Interpolate { get; private set; }
		public RigidbodySleepMode2D SleepingMode { get; private set; }
		public CollisionDetectionMode2D CollisionDetection { get; private set; }
		public RigidbodyConstraints2D Constraints { get; private set; }

		public PPtr<PhysicsMaterial2D> Material;
	}
}
