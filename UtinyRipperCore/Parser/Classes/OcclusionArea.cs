using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
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

		private static bool IsExportIsTargetVolume(Version version)
		{
			return Config.IsExportTopmostSerializedVersion || IsReadIsTargetVolume(version);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

#warning unknown
			if (version.IsGreater(3, 0, 0, VersionType.Beta))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			Size.Read(stream);
			Center.Read(stream);
			IsViewVolume = stream.ReadBoolean();
			if (IsReadIsTargetVolume(stream.Version))
			{
				IsTargetVolume = stream.ReadBoolean();
			}
			stream.AlignStream(AlignType.Align4);

			if (IsReadTargetResolution(stream.Version))
			{
				TargetResolution = stream.ReadInt32();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Size", Size.ExportYAML(container));
			node.Add("m_Center", Center.ExportYAML(container));
			node.Add("m_IsViewVolume", IsViewVolume);
			if (IsExportIsTargetVolume(container.Version))
			{
				node.Add("m_IsTargetVolume", IsViewVolume);
				node.Add("m_TargetResolution", TargetResolution);
			}
			return node;
		}

		public bool IsViewVolume { get; private set; }
		public bool IsTargetVolume { get; private set; }
		public int TargetResolution { get; private set; }

		public Vector3f Size;
		public Vector3f Center;
	}
}