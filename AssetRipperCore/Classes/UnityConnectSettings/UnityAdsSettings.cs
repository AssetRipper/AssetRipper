using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.UnityConnectSettings
{
	public sealed class UnityAdsSettings : IAssetReadable, IYAMLExportable
	{
		public UnityAdsSettings()
		{
			InitializeOnStartup = true;
			IosGameId = string.Empty;
			AndroidGameId = string.Empty;
			GameId = string.Empty;
		}

		/// <summary>
		/// Less than 2017.2
		/// </summary>
		public static bool HasEnabledPlatforms(UnityVersion version) => version.IsLess(2017, 2);
		/// <summary>
		/// Less than 2017.1 or Not Release
		/// </summary>
		public static bool HasIosGameId(UnityVersion version, TransferInstructionFlags flags) => version.IsLess(2017) || !flags.IsRelease();
		/// <summary>
		/// 2017.1 and greater and Not Release
		/// </summary>
		public static bool HasGameIds(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(2017) && !flags.IsRelease();
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool HasGameId(UnityVersion version) => version.IsGreaterEqual(2017);

		public void Read(AssetReader reader)
		{
			Enabled = reader.ReadBoolean();
			InitializeOnStartup = reader.ReadBoolean();
			TestMode = reader.ReadBoolean();
			reader.AlignStream();

			if (HasEnabledPlatforms(reader.Version))
			{
				EnabledPlatforms = reader.ReadInt32();
			}
			if (HasIosGameId(reader.Version, reader.Flags))
			{
				IosGameId = reader.ReadString();
				AndroidGameId = reader.ReadString();
			}
			if (HasGameId(reader.Version))
			{
				GameId = reader.ReadString();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(EnabledName, Enabled);
			node.Add(InitializeOnStartupName, InitializeOnStartup);
			node.Add(TestModeName, TestMode);
			node.Add(IosGameIdName, GetIosGameId(container.Version, container.Flags));
			node.Add(AndroidGameIdName, GetAndroidGameId(container.Version, container.Flags));
			node.Add(GameIdsName, GetGameIds(container.Version, container.Flags).ExportYAML());
			node.Add(GameIdName, GetGameId(container.Version));
			return node;
		}

		private string GetIosGameId(UnityVersion version, TransferInstructionFlags flags)
		{
			if (HasIosGameId(version, flags))
			{
				return IosGameId;
			}
			return string.Empty;
		}
		private string GetAndroidGameId(UnityVersion version, TransferInstructionFlags flags)
		{
			if (HasIosGameId(version, flags))
			{
				return AndroidGameId;
			}
			return string.Empty;
		}
		private IReadOnlyDictionary<string, string> GetGameIds(UnityVersion version, TransferInstructionFlags flags)
		{
			return new Dictionary<string, string>();
		}
		private string GetGameId(UnityVersion version)
		{
			return HasGameId(version) ? GameId : string.Empty;
		}

		public bool Enabled { get; set; }
		public bool InitializeOnStartup { get; set; }
		public bool TestMode { get; set; }
		public int EnabledPlatforms { get; set; }
		public string IosGameId { get; set; }
		public string AndroidGameId { get; set; }
		public string GameId { get; set; }

		public const string EnabledName = "m_Enabled";
		public const string InitializeOnStartupName = "m_InitializeOnStartup";
		public const string TestModeName = "m_TestMode";
		public const string IosGameIdName = "m_IosGameId";
		public const string AndroidGameIdName = "m_AndroidGameId";
		public const string GameIdsName = "m_GameIds";
		public const string GameIdName = "m_GameId";
	}
}
