using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.NavMeshDatas;
using uTinyRipper.SerializedFiles;
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
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadNavMeshData(Version version)
		{
			return version.IsGreaterEqual(5);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadBuildSettings(reader.Flags))
			{
				BuildSettings.Read(reader);
			}
			m_navMeshData.Read(reader);
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			yield return m_navMeshData.FetchDependency(file, isLog, ToLogString, NavMeshDataName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(BuildSettingsName, GetExportNavMeshBuildSettings(container).ExportYAML(container));
			node.Add(NavMeshDataName, IsReadNavMeshData(container.Version) ? NavMeshData.ExportYAML(container) : NavMesh.ExportYAML(container));
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
				NavMeshData data = IsReadNavMeshData(container.Version) ? NavMeshData.FindAsset(container) : null;
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
		public const string NavMeshName = "m_NavMesh";
		public const string NavMeshDataName = "m_NavMeshData";

		public PPtr<NavMeshObsolete> NavMesh => m_navMeshData.CastTo<NavMeshObsolete>();
		public PPtr<NavMeshData> NavMeshData => m_navMeshData.CastTo<NavMeshData>();

		public NavMeshBuildSettings BuildSettings;

		private PPtr<NamedObject> m_navMeshData;
	}
}
