using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes
{
	public sealed class OcclusionArea : Component
	{
		public OcclusionArea(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			return 2;
			// NOTE: unknown version
			//return 1;
		}

		/// <summary>
		/// Less than 4.3.0
		/// </summary>
		public static bool HasIsTargetVolume(UnityVersion version) => version.IsLess(4, 3);
		/// <summary>
		/// Less than 4.3.0
		/// </summary>
		public static bool HasTargetResolution(UnityVersion version) => version.IsLess(4, 3);

		private static bool IsExportIsTargetVolume(UnityVersion version, TransferInstructionFlags flags) => flags.IsRelease() || HasIsTargetVolume(version);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Size.Read(reader);
			Center.Read(reader);
			IsViewVolume = reader.ReadBoolean();
			if (HasIsTargetVolume(reader.Version))
			{
				IsTargetVolume = reader.ReadBoolean();
			}
			reader.AlignStream();

			if (HasTargetResolution(reader.Version))
			{
				TargetResolution = reader.ReadInt32();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(SizeName, Size.ExportYAML(container));
			node.Add(CenterName, Center.ExportYAML(container));
			node.Add(IsViewVolumeName, IsViewVolume);
			if (IsExportIsTargetVolume(container.ExportVersion, container.ExportFlags))
			{
				node.Add(IsTargetVolumeName, IsViewVolume);
				node.Add(TargetResolutionName, TargetResolution);
			}
			return node;
		}

		public bool IsViewVolume { get; set; }
		public bool IsTargetVolume { get; set; }
		public int TargetResolution { get; set; }

		public const string SizeName = "m_Size";
		public const string CenterName = "m_Center";
		public const string IsViewVolumeName = "m_IsViewVolume";
		public const string IsTargetVolumeName = "m_IsTargetVolume";
		public const string TargetResolutionName = "m_TargetResolution";

		public Vector3f Size = new();
		public Vector3f Center = new();
	}
}
