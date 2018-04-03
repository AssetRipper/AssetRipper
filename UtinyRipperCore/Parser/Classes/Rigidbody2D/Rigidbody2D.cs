using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.Rigidbody2Ds;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
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

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			if (IsReadBodyType(stream.Version))
			{
				BodyType = (RigidbodyType2D)stream.ReadInt32();
				Simulated = stream.ReadBoolean();
				UseFullKinematicContacts = stream.ReadBoolean();
			}
			if (IsReadUseAutoMass(stream.Version))
			{
				UseAutoMass = stream.ReadBoolean();
				stream.AlignStream(AlignType.Align4);
			}
			
			Mass = stream.ReadSingle();
			LinearDrag = stream.ReadSingle();
			AngularDrag = stream.ReadSingle();
			GravityScale = stream.ReadSingle();
			if (IsReadMaterial(stream.Version))
			{
				Material.Read(stream);
			}

			if (IsReadFixedAngle(stream.Version))
			{
				bool fixedAngle = stream.ReadBoolean();
				Constraints = fixedAngle ? RigidbodyConstraints2D.FreezeRotation : RigidbodyConstraints2D.None;
			}
			if (IsReadIsKinematic(stream.Version))
			{
				bool isKinematic = stream.ReadBoolean();
				BodyType = isKinematic ? RigidbodyType2D.Kinematic : RigidbodyType2D.Static;
				Interpolate = (RigidbodyInterpolation2D)stream.ReadByte();
				SleepingMode = (RigidbodySleepMode2D)stream.ReadByte();
				CollisionDetection = (CollisionDetectionMode2D)stream.ReadByte();
				stream.AlignStream(AlignType.Align4);
			}
			
			if (IsReadInterpolate(stream.Version))
			{
				Interpolate = (RigidbodyInterpolation2D)stream.ReadInt32();
				SleepingMode = (RigidbodySleepMode2D)stream.ReadInt32();
				CollisionDetection = (CollisionDetectionMode2D)stream.ReadInt32();
			}
			if (!IsReadFixedAngle(stream.Version))
			{
				Constraints = (RigidbodyConstraints2D)stream.ReadInt32();
			}
		}
		
		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.InsertSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("m_BodyType", (int)BodyType);
			node.Add("m_Simulated", Simulated);
			node.Add("m_UseFullKinematicContacts", UseFullKinematicContacts);
			node.Add("m_UseAutoMass", UseAutoMass);
			node.Add("m_Mass", Mass);
			node.Add("m_LinearDrag", LinearDrag);
			node.Add("m_AngularDrag", AngularDrag);
			node.Add("m_GravityScale", GravityScale);
			node.Add("m_Material", Material.ExportYAML(exporter));
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
