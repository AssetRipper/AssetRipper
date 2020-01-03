using uTinyRipper.Classes.PhysicMaterials;
using uTinyRipper.Classes.WheelColliders;
using uTinyRipper.Converters;
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
		public static bool HasForceAppPointDistance(Version version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasWheelDampingRate(Version version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasEnabled(Version version) => version.IsGreaterEqual(3, 5);

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		private static bool HasSuspensionSpringFirst(Version version)
		{
			return version.IsGreaterEqual(5);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Center.Read(reader);
			Radius = reader.ReadSingle();
			if (HasSuspensionSpringFirst(reader.Version))
			{
				SuspensionSpring.Read(reader);
			}
			SuspensionDistance = reader.ReadSingle();
			if (!HasSuspensionSpringFirst(reader.Version))
			{
				SuspensionSpring.Read(reader);
			}
			if (HasForceAppPointDistance(reader.Version))
			{
				ForceAppPointDistance = reader.ReadSingle();
			}
			Mass = reader.ReadSingle();
			if (HasWheelDampingRate(reader.Version))
			{
				WheelDampingRate = reader.ReadSingle();
			}
			ForwardFriction.Read(reader);
			SidewaysFriction.Read(reader);
			if (HasEnabled(reader.Version))
			{
				Enabled = reader.ReadBoolean();
				reader.AlignStream();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(CenterName, Center.ExportYAML(container));
			node.Add(RadiusName, Radius);
			node.Add(SuspensionSpringName, SuspensionSpring.ExportYAML(container));
			node.Add(SuspensionDistanceName, SuspensionDistance);
			node.Add(ForceAppPointDistanceName, ForceAppPointDistance);
			node.Add(MassName, Mass);
			node.Add(WheelDampingRateName, WheelDampingRate);
			node.Add(ForwardFrictionName, ForwardFriction.ExportYAML(container));
			node.Add(SidewaysFrictionName, SidewaysFriction.ExportYAML(container));
			node.Add(EnabledName, Enabled);
			return node;
		}

		public float Radius { get; set; }
		public float SuspensionDistance { get; set; }
		public float ForceAppPointDistance { get; set; }
		public float Mass { get; set; }
		public float WheelDampingRate { get; set; }
		public bool Enabled { get; set; }

		public const string CenterName = "m_Center";
		public const string RadiusName = "m_Radius";
		public const string SuspensionSpringName = "m_SuspensionSpring";
		public const string SuspensionDistanceName = "m_SuspensionDistance";
		public const string ForceAppPointDistanceName = "m_ForceAppPointDistance";
		public const string MassName = "m_Mass";
		public const string WheelDampingRateName = "m_WheelDampingRate";
		public const string ForwardFrictionName = "m_ForwardFriction";
		public const string SidewaysFrictionName = "m_SidewaysFriction";
		public const string EnabledName = "m_Enabled";

		public Vector3f Center;
		public JointSpring SuspensionSpring;
		public WheelFrictionCurve ForwardFriction;
		public WheelFrictionCurve SidewaysFriction;
	}
}
