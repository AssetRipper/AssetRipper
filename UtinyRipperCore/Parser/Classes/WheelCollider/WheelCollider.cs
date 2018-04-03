using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.PhysicMaterials;
using UtinyRipper.Classes.WheelColliders;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
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

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			Center.Read(stream);
			Radius = stream.ReadSingle();
			if (IsReadSuspensionSpringFirst(stream.Version))
			{
				SuspensionSpring.Read(stream);
			}
			SuspensionDistance = stream.ReadSingle();
			if (!IsReadSuspensionSpringFirst(stream.Version))
			{
				SuspensionSpring.Read(stream);
			}
			if (IsReadForceAppPointDistance(stream.Version))
			{
				ForceAppPointDistance = stream.ReadSingle();
			}
			Mass = stream.ReadSingle();
			if (IsReadWheelDampingRate(stream.Version))
			{
				WheelDampingRate = stream.ReadSingle();
			}
			ForwardFriction.Read(stream);
			SidewaysFriction.Read(stream);
			if (IsReadEnabled(stream.Version))
			{
				Enabled = stream.ReadBoolean();
				stream.AlignStream(AlignType.Align4);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.Add("m_Center", Center.ExportYAML(exporter));
			node.Add("m_Radius", Radius);
			node.Add("m_SuspensionSpring", SuspensionSpring.ExportYAML(exporter));
			node.Add("m_SuspensionDistance", SuspensionDistance);
			node.Add("m_ForceAppPointDistance", ForceAppPointDistance);
			node.Add("m_Mass", Mass);
			node.Add("m_WheelDampingRate", WheelDampingRate);
			node.Add("m_ForwardFriction", ForwardFriction.ExportYAML(exporter));
			node.Add("m_SidewaysFriction", SidewaysFriction.ExportYAML(exporter));
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
