using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.NavMeshDatas;
using uTinyRipper.Classes.NavMeshProjectSettingss;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	/// <summary>
	/// NavMeshAreas previously
	/// NavMeshLayers even earlier
	/// </summary>
	public sealed class NavMeshProjectSettings : GlobalGameManager
	{
		public NavMeshProjectSettings(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		private NavMeshProjectSettings(AssetInfo assetInfo, bool _):
			this(assetInfo)
		{
			m_areas = new NavMeshAreaData[32];
			m_areas[0] = new NavMeshAreaData("Walkable", 1, 2);
			m_areas[1] = new NavMeshAreaData("Not Walkable", 1, 0);
			m_areas[2] = new NavMeshAreaData("Jump", 2, 2);
			for (int i = 3; i < m_areas.Length; i++)
			{
				m_areas[i] = new NavMeshAreaData(string.Empty, 1, 3);
			}
		}

		public static NavMeshProjectSettings CreateVirtualInstance(VirtualSerializedFile virtualFile)
		{
			return virtualFile.CreateAsset((assetInfo) => new NavMeshProjectSettings(assetInfo, true));
		}

		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool IsReadNavMeshProjectSettings(Version version)
		{
			return version.IsGreaterEqual(3, 5);
		}

		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadLastAgentTypeID(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}

		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		private static bool IsReadStaticAreas(Version version)
		{
			return GetSerializedVersion(version) < 2;
		}

		private static int GetSerializedVersion(Version version)
		{
			// NavMeshLayerData with individual names converted to dynamic array of NavMeshAreaData
			if (version.IsGreaterEqual(5))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadStaticAreas(reader.Version))
			{
				m_areas = new NavMeshAreaData[32];
				for (int i = 0; i < 32; i++)
				{
					m_areas[i] = reader.ReadAsset<NavMeshAreaData>();
				}
			}
			else
			{
				m_areas = reader.ReadAssetArray<NavMeshAreaData>();
			}
			if (IsReadLastAgentTypeID(reader.Version))
			{
				LastAgentTypeID = reader.ReadInt32();
				m_settings = reader.ReadAssetArray<NavMeshBuildSettings>();
				m_settingNames = reader.ReadStringArray();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(AreasName, Areas.ExportYAML(container));
			node.Add(LastAgentTypeIDName, GetLastAgentTypeID(container.Version));
			node.Add(SettingsName, GetSettings(container.Version).ExportYAML(container));
			node.Add(SettingNamesName, GetSettingNames(container.Version).ExportYAML());
			return node;
		}

		private int GetLastAgentTypeID(Version version)
		{
			return IsReadLastAgentTypeID(version) ? LastAgentTypeID : -887442657;
		}
		private IReadOnlyList<NavMeshBuildSettings> GetSettings(Version version)
		{
			return IsReadLastAgentTypeID(version) ? Settings : new [] { new NavMeshBuildSettings(0.75f, 1.0f / 6.0f) };
		}
		private IReadOnlyList<string> GetSettingNames(Version version)
		{
			return IsReadLastAgentTypeID(version) ? SettingNames : new [] { "Humanoid" };
		}

		public override string ExportPath => nameof(ClassIDType.NavMeshAreas);

		public IReadOnlyList<NavMeshAreaData> Areas => m_areas;
		public int LastAgentTypeID { get; private set; }
		public IReadOnlyList<NavMeshBuildSettings> Settings => m_settings;
		public IReadOnlyList<string> SettingNames => m_settingNames;

		public const string AreasName = "areas";
		public const string LastAgentTypeIDName = "m_LastAgentTypeID";
		public const string SettingsName = "m_Settings";
		public const string SettingNamesName = "m_SettingNames";

		private NavMeshAreaData[] m_areas;
		private NavMeshBuildSettings[] m_settings;
		private string[] m_settingNames;
	}
}
