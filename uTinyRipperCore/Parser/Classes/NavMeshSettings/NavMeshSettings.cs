using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.NavMeshDatas;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class NavMeshSettings : LevelGameManager
	{
		public NavMeshSettings(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// Not Release
		/// </summary>
		public static bool IsReadBuildSettings(TransferInstructionFlags flags)
		{
			return !flags.IsRelease();
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadBuildSettings(reader.Flags))
			{
				BuildSettings.Read(reader);
			}
			NavMeshData.Read(reader);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(BuildSettingsName, GetExportNavMeshBuildSettings(container).ExportYAML(container));
			node.Add(NavMeshDataName, NavMeshData.ExportYAML(container));
			return node;
		}

		private NavMeshBuildSettings GetExportNavMeshBuildSettings(IExportContainer container)
		{
			if (IsReadBuildSettings(container.Flags))
			{
				return BuildSettings;
			}
			else
			{
				NavMeshData data = NavMeshData.FindAsset(container);
				if (data == null)
				{
					return new NavMeshBuildSettings(true);
				}
				else
				{
					if (Classes.NavMeshData.IsReadNavMeshParams(container.Version))
					{
						return new NavMeshBuildSettings(data.NavMeshParams);
					}
					else
					{
						return data.NavMeshBuildSettings;
					}
				}
			}
		}

		public const string BuildSettingsName = "m_BuildSettings";
		public const string NavMeshDataName = "m_NavMeshData";

		public NavMeshBuildSettings BuildSettings;
		public PPtr<NavMeshData> NavMeshData;
	}
}
