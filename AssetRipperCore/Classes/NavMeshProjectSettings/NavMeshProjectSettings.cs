using AssetRipper.Core.Classes.NavMeshData;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;

using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.NavMeshProjectSettings
{
	/// <summary>
	/// NavMeshAreas previously
	/// NavMeshLayers even earlier
	/// </summary>
	public sealed class NavMeshProjectSettings : GlobalGameManager
	{
		public NavMeshProjectSettings(AssetInfo assetInfo) : base(assetInfo) { }

		private NavMeshProjectSettings(AssetInfo assetInfo, bool _) : this(assetInfo)
		{
			Areas = new NavMeshAreaData[32];
			Areas[0] = new NavMeshAreaData("Walkable", 1, 2);
			Areas[1] = new NavMeshAreaData("Not Walkable", 1, 0);
			Areas[2] = new NavMeshAreaData("Jump", 2, 2);
			for (int i = 3; i < Areas.Length; i++)
			{
				Areas[i] = new NavMeshAreaData(string.Empty, 1, 3);
			}
		}

		public static NavMeshProjectSettings CreateVirtualInstance(VirtualSerializedFile virtualFile)
		{
			return virtualFile.CreateAsset((assetInfo) => new NavMeshProjectSettings(assetInfo, true));
		}

		public static int ToSerializedVersion(UnityVersion version)
		{
			// NavMeshLayerData with individual names converted to dynamic array of NavMeshAreaData
			if (version.IsGreaterEqual(5))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasNavMeshProjectSettings(UnityVersion version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasLastAgentTypeID(UnityVersion version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		private static bool HasStaticAreas(UnityVersion version) => version.IsLess(5);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasStaticAreas(reader.Version))
			{
				Areas = new NavMeshAreaData[32];
				for (int i = 0; i < 32; i++)
				{
					Areas[i] = reader.ReadAsset<NavMeshAreaData>();
				}
			}
			else
			{
				Areas = reader.ReadAssetArray<NavMeshAreaData>();
			}
			if (HasLastAgentTypeID(reader.Version))
			{
				LastAgentTypeID = reader.ReadInt32();
				Settings = reader.ReadAssetArray<NavMeshBuildSettings>();
				SettingNames = reader.ReadStringArray();
			}
		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(AreasName, Areas.ExportYaml(container));
			node.Add(LastAgentTypeIDName, GetLastAgentTypeID(container.Version));
			node.Add(SettingsName, GetSettings(container.Version).ExportYaml(container));
			node.Add(SettingNamesName, GetSettingNames(container.Version).ExportYaml());
			return node;
		}

		private int GetLastAgentTypeID(UnityVersion version)
		{
			return HasLastAgentTypeID(version) ? LastAgentTypeID : -887442657;
		}
		private IReadOnlyList<NavMeshBuildSettings> GetSettings(UnityVersion version)
		{
			return HasLastAgentTypeID(version) ? Settings : new[] { new NavMeshBuildSettings(0.75f, 1.0f / 6.0f) };
		}
		private IReadOnlyList<string> GetSettingNames(UnityVersion version)
		{
			return HasLastAgentTypeID(version) ? SettingNames : new[] { "Humanoid" };
		}

		public override string ExportPath => "NavMeshAreas";

		public NavMeshAreaData[] Areas { get; set; }
		public int LastAgentTypeID { get; set; }
		public NavMeshBuildSettings[] Settings { get; set; }
		public string[] SettingNames { get; set; }

		public const string AreasName = "areas";
		public const string LastAgentTypeIDName = "m_LastAgentTypeID";
		public const string SettingsName = "m_Settings";
		public const string SettingNamesName = "m_SettingNames";
	}
}
