using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.Rigidbodies;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public sealed class Rigidbody : Component
	{
		public Rigidbody(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 1.5.0 and greater
		/// </summary>
		public static bool IsReadInterpolate(Version version)
		{
			return version.IsGreaterEqual(1, 5);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadCollisionDetection(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		
		/// <summary>
		/// Less than 3.2.0
		/// </summary>
		private static bool IsReadFreezeRotation(Version version)
		{
			return version.IsLess(3, 2);
		}
		/// <summary>
		/// 3.2.0 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(3, 2);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

			if (version.IsGreaterEqual(3, 2))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			Mass = stream.ReadSingle();
			Drag = stream.ReadSingle();
			AngularDrag = stream.ReadSingle();
			UseGravity = stream.ReadBoolean();
			IsKinematic = stream.ReadBoolean();
			if (IsReadInterpolate(stream.Version))
			{
				Interpolate = (RigidbodyInterpolation)stream.ReadByte();
				if (IsAlign(stream.Version))
				{
					stream.AlignStream(AlignType.Align4);
				}
			}

			if (IsReadFreezeRotation(stream.Version))
			{
				bool freezeRotation = stream.ReadBoolean();
				Constraints = freezeRotation ? RigidbodyConstraints.FreezeRotation : RigidbodyConstraints.None;
			}
			else
			{
				Constraints = (RigidbodyConstraints)stream.ReadInt32();
			}
			if (IsReadCollisionDetection(stream.Version))
			{
				CollisionDetection = (CollisionDetectionMode)stream.ReadInt32();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Mass", Mass);
			node.Add("m_Drag", Drag);
			node.Add("m_AngularDrag", AngularDrag);
			node.Add("m_UseGravity", UseGravity);
			node.Add("m_IsKinematic", IsKinematic);
			node.Add("m_Interpolate", (byte)Interpolate);
			node.Add("m_Constraints", (int)Constraints);
			node.Add("m_CollisionDetection", (int)CollisionDetection);
			return node;
		}

		public float Mass { get; private set; }
		public float Drag { get; private set; }
		public float AngularDrag { get; private set; }
		public bool UseGravity { get; private set; }
		public bool IsKinematic { get; private set; }
		public RigidbodyInterpolation Interpolate { get; private set; }
		public RigidbodyConstraints Constraints { get; private set; }
		public CollisionDetectionMode CollisionDetection { get; private set; }
	}
}
