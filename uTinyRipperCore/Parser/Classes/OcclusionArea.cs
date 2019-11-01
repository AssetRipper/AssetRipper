using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class OcclusionArea : Component
	{
		public OcclusionArea(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// Less than 4.3.0
		/// </summary>
		public static bool IsReadIsTargetVolume(Version version)
		{
			return version.IsLess(4, 3);
		}
		/// <summary>
		/// Less than 4.3.0
		/// </summary>
		public static bool IsReadTargetResolution(Version version)
		{
			return version.IsLess(4, 3);
		}

		private static bool IsExportIsTargetVolume(Version version, TransferInstructionFlags flags)
		{
			return flags.IsRelease() || IsReadIsTargetVolume(version);
		}

		private static int GetSerializedVersion(Version version)
		{
			return 2;
			// unknown (beta) version
			//return 1;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Size.Read(reader);
			Center.Read(reader);
			IsViewVolume = reader.ReadBoolean();
			if (IsReadIsTargetVolume(reader.Version))
			{
				IsTargetVolume = reader.ReadBoolean();
			}
			reader.AlignStream();

			if (IsReadTargetResolution(reader.Version))
			{
				TargetResolution = reader.ReadInt32();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
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

		public bool IsViewVolume { get; private set; }
		public bool IsTargetVolume { get; private set; }
		public int TargetResolution { get; private set; }

		public const string SizeName = "m_Size";
		public const string CenterName = "m_Center";
		public const string IsViewVolumeName = "m_IsViewVolume";
		public const string IsTargetVolumeName = "m_IsTargetVolume";
		public const string TargetResolutionName = "m_TargetResolution";

		public Vector3f Size;
		public Vector3f Center;
	}
}