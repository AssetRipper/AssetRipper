using uTinyRipper.Classes.Rigidbodies;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class Rigidbody : Component
	{
		public Rigidbody(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(3, 2))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 1.5.0 and greater
		/// </summary>
		public static bool HasInterpolate(Version version) => version.IsGreaterEqual(1, 5);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasCollisionDetection(Version version) => version.IsGreaterEqual(3);
		
		/// <summary>
		/// Less than 3.2.0
		/// </summary>
		private static bool HasFreezeRotation(Version version) => version.IsLess(3, 2);

		/// <summary>
		/// 3.2.0 and greater
		/// </summary>
		private static bool IsAlign(Version version) => version.IsGreaterEqual(3, 2);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Mass = reader.ReadSingle();
			Drag = reader.ReadSingle();
			AngularDrag = reader.ReadSingle();
			UseGravity = reader.ReadBoolean();
			IsKinematic = reader.ReadBoolean();
			if (HasInterpolate(reader.Version))
			{
				Interpolate = (RigidbodyInterpolation)reader.ReadByte();
				if (IsAlign(reader.Version))
				{
					reader.AlignStream();
				}
			}

			if (HasFreezeRotation(reader.Version))
			{
				bool freezeRotation = reader.ReadBoolean();
				Constraints = freezeRotation ? RigidbodyConstraints.FreezeRotation : RigidbodyConstraints.None;
			}
			else
			{
				Constraints = (RigidbodyConstraints)reader.ReadInt32();
			}
			if (HasCollisionDetection(reader.Version))
			{
				CollisionDetection = (CollisionDetectionMode)reader.ReadInt32();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(MassName, Mass);
			node.Add(DragName, Drag);
			node.Add(AngularDragName, AngularDrag);
			node.Add(UseGravityName, UseGravity);
			node.Add(IsKinematicName, IsKinematic);
			node.Add(InterpolateName, (byte)Interpolate);
			node.Add(ConstraintsName, (int)Constraints);
			node.Add(CollisionDetectionName, (int)CollisionDetection);
			return node;
		}

		public float Mass { get; set; }
		public float Drag { get; set; }
		public float AngularDrag { get; set; }
		public bool UseGravity { get; set; }
		public bool IsKinematic { get; set; }
		public RigidbodyInterpolation Interpolate { get; set; }
		public RigidbodyConstraints Constraints { get; set; }
		public CollisionDetectionMode CollisionDetection { get; set; }

		public const string MassName = "m_Mass";
		public const string DragName = "m_Drag";
		public const string AngularDragName = "m_AngularDrag";
		public const string UseGravityName = "m_UseGravity";
		public const string IsKinematicName = "m_IsKinematic";
		public const string InterpolateName = "m_Interpolate";
		public const string ConstraintsName = "m_Constraints";
		public const string CollisionDetectionName = "m_CollisionDetection";
	}
}
