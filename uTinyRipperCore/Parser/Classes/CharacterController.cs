using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class CharacterController : Collider
	{
		public CharacterController(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadCollider(Version version)
		{
			return version.IsGreaterEqual(5);
		}

		private static int GetSerializedVersion(Version version)
		{
			// SlopeLimit default value has been changed to 45.0f
			if (version.IsGreaterEqual(3))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetReader reader)
		{
			if (IsReadCollider(reader.Version))
			{
				base.Read(reader);
			}
			else
			{
				ReadComponent(reader);
				Enabled = true;
			}

			Height = reader.ReadSingle();
			Radius = reader.ReadSingle();
			SlopeLimit = reader.ReadSingle();
			StepOffset = reader.ReadSingle();
			SkinWidth = reader.ReadSingle();
			MinMoveDistance = reader.ReadSingle();
			Center.Read(reader);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(HeightName, Height);
			node.Add(RadiusName, Radius);
			node.Add(SlopeLimitName, GetSlopeLimit(container.Version));
			node.Add(StepOffsetName, StepOffset);
			node.Add(SkinWidthName, SkinWidth);
			node.Add(MinMoveDistanceName, MinMoveDistance);
			node.Add(CenterName, Center.ExportYAML(container));
			return node;
		}

		private float GetSlopeLimit(Version version)
		{
			if (GetSerializedVersion(version) >= 2)
			{
				return SlopeLimit;
			}
			else
			{
				return SlopeLimit <= 45.0f ? SlopeLimit : 45.0f;
			}
		}

		public float Height { get; private set; }
		public float Radius { get; private set; }
		public float SlopeLimit { get; private set; }
		public float StepOffset { get; private set; }
		public float SkinWidth { get; private set; }
		public float MinMoveDistance { get; private set; }

		protected override bool IsReadMaterial => true;
		protected override bool IsReadIsTrigger => true;

		public const string HeightName = "m_Height";
		public const string RadiusName = "m_Radius";
		public const string SlopeLimitName = "m_SlopeLimit";
		public const string StepOffsetName = "m_StepOffset";
		public const string SkinWidthName = "m_SkinWidth";
		public const string MinMoveDistanceName = "m_MinMoveDistance";
		public const string CenterName = "m_Center";

		public Vector3f Center;
	}
}
