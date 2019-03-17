using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.PhysicMaterials;
using uTinyRipper.Classes.WheelColliders;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class WheelCollider : Component
	{
		public WheelCollider(AssetInfo assetInfo):
			base(assetInfo)
		{
		}
		
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadForceAppPointDistance(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadWheelDampingRate(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool IsReadEnabled(Version version)
		{
			return version.IsGreaterEqual(3, 5);
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		private static bool IsReadSuspensionSpringFirst(Version version)
		{
			return version.IsGreaterEqual(5);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Center.Read(reader);
			Radius = reader.ReadSingle();
			if (IsReadSuspensionSpringFirst(reader.Version))
			{
				SuspensionSpring.Read(reader);
			}
			SuspensionDistance = reader.ReadSingle();
			if (!IsReadSuspensionSpringFirst(reader.Version))
			{
				SuspensionSpring.Read(reader);
			}
			if (IsReadForceAppPointDistance(reader.Version))
			{
				ForceAppPointDistance = reader.ReadSingle();
			}
			Mass = reader.ReadSingle();
			if (IsReadWheelDampingRate(reader.Version))
			{
				WheelDampingRate = reader.ReadSingle();
			}
			ForwardFriction.Read(reader);
			SidewaysFriction.Read(reader);
			if (IsReadEnabled(reader.Version))
			{
				Enabled = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_Center", Center.ExportYAML(container));
			node.Add("m_Radius", Radius);
			node.Add("m_SuspensionSpring", SuspensionSpring.ExportYAML(container));
			node.Add("m_SuspensionDistance", SuspensionDistance);
			node.Add("m_ForceAppPointDistance", ForceAppPointDistance);
			node.Add("m_Mass", Mass);
			node.Add("m_WheelDampingRate", WheelDampingRate);
			node.Add("m_ForwardFriction", ForwardFriction.ExportYAML(container));
			node.Add("m_SidewaysFriction", SidewaysFriction.ExportYAML(container));
			node.Add("m_Enabled", Enabled);
			return node;
		}

		public float Radius { get; private set; }
		public float SuspensionDistance { get; private set; }
		public float ForceAppPointDistance { get; private set; }
		public float Mass { get; private set; }
		public float WheelDampingRate { get; private set; }
		public bool Enabled { get; private set; }

		public Vector3f Center;
		public JointSpring SuspensionSpring;
		public WheelFrictionCurve ForwardFriction;
		public WheelFrictionCurve SidewaysFriction;
	}
}
