using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes
{
	public sealed class CharacterController : Collider
	{
		public CharacterController(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			// SlopeLimit default value has been changed to 45.0f
			if (version.IsGreaterEqual(3))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasCollider(UnityVersion version) => version.IsGreaterEqual(5);

		public override void Read(AssetReader reader)
		{
			if (HasCollider(reader.Version))
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

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(HeightName, Height);
			node.Add(RadiusName, Radius);
			node.Add(SlopeLimitName, GetSlopeLimit(container.Version));
			node.Add(StepOffsetName, StepOffset);
			node.Add(SkinWidthName, SkinWidth);
			node.Add(MinMoveDistanceName, MinMoveDistance);
			node.Add(CenterName, Center.ExportYaml(container));
			return node;
		}

		private float GetSlopeLimit(UnityVersion version)
		{
			if (ToSerializedVersion(version) >= 2)
			{
				return SlopeLimit;
			}
			else
			{
				return SlopeLimit <= 45.0f ? SlopeLimit : 45.0f;
			}
		}

		public float Height { get; set; }
		public float Radius { get; set; }
		public float SlopeLimit { get; set; }
		public float StepOffset { get; set; }
		public float SkinWidth { get; set; }
		public float MinMoveDistance { get; set; }

		protected override bool IncludesMaterial => true;
		protected override bool IncludesIsTrigger => true;

		public const string HeightName = "m_Height";
		public const string RadiusName = "m_Radius";
		public const string SlopeLimitName = "m_SlopeLimit";
		public const string StepOffsetName = "m_StepOffset";
		public const string SkinWidthName = "m_SkinWidth";
		public const string MinMoveDistanceName = "m_MinMoveDistance";
		public const string CenterName = "m_Center";

		public Vector3f Center = new();
	}
}
