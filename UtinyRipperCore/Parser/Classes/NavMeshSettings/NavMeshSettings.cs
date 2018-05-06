using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.NavMeshDatas;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
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
			return !flags.IsSerializeGameRelease();
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			if (IsReadBuildSettings(stream.Flags))
			{
				BuildSettings.Read(stream);
			}
			NavMeshData.Read(stream);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_BuildSettings", GetExportNavMeshBuildSettings(container).ExportYAML(container));
			node.Add("m_NavMeshData", NavMeshData.ExportYAML(container));
			return node;
		}

		private NavMeshBuildSettings GetExportNavMeshBuildSettings(IExportContainer container)
		{
			if(IsReadBuildSettings(container.Flags))
			{
				return BuildSettings;
			}
			else
			{
				NavMeshData data = NavMeshData.FindObject(container);
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

		public NavMeshBuildSettings BuildSettings;
		public PPtr<NavMeshData> NavMeshData;
	}
}
