using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.NavMeshData;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.NavMeshSettings
{
	public sealed class NavMeshSettings : LevelGameManager
	{
		public NavMeshSettings(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// Not Release
		/// </summary>
		public static bool HasBuildSettings(TransferInstructionFlags flags)
		{
			return !flags.IsRelease();
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasNavMeshData(UnityVersion version) => version.IsGreaterEqual(5);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasBuildSettings(reader.Flags))
			{
				BuildSettings.Read(reader);
			}
			m_navMeshData.Read(reader);
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(m_navMeshData, NavMeshDataName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(BuildSettingsName, GetExportNavMeshBuildSettings(container).ExportYAML(container));
			node.Add(NavMeshDataName, HasNavMeshData(container.Version) ? NavMeshData.ExportYAML(container) : NavMesh.ExportYAML(container));
			return node;
		}

		private NavMeshBuildSettings GetExportNavMeshBuildSettings(IExportContainer container)
		{
			if (HasBuildSettings(container.Flags))
			{
				return BuildSettings;
			}
			else
			{
				NavMeshData.NavMeshData data = HasNavMeshData(container.Version) ? NavMeshData.FindAsset(container) : null;
				if (data == null)
				{
					return new NavMeshBuildSettings(true);
				}
				else
				{
					if (Classes.NavMeshData.NavMeshData.HasNavMeshParams(container.Version))
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
		public PPtr<NavMeshData.NavMeshData> NavMeshData => m_navMeshData.CastTo<NavMeshData.NavMeshData>();

		public NavMeshBuildSettings BuildSettings = new();

		private PPtr<NamedObject> m_navMeshData = new();
	}
}
