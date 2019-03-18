using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.UnityConnectSettingss
{
	public struct UnityAdsSettings : IAssetReadable, IYAMLExportable
	{
		public UnityAdsSettings(bool _):
			this()
		{
			InitializeOnStartup = true;
			IosGameId = string.Empty;
			AndroidGameId = string.Empty;
#if UNIVERSAL
			m_gameIds = new Dictionary<string, string>();
#endif
		}

		/// <summary>
		/// Less than 2017.2
		/// </summary>
		public static bool IsReadEnabledPlatforms(Version version)
		{
			return version.IsLess(2017, 2);
		}
		/// <summary>
		/// Less than 2017.1 or Not Release
		/// </summary>
		public static bool IsReadIosGameId(Version version, TransferInstructionFlags flags)
		{
			return version.IsLess(2017) || !flags.IsRelease();
		}
		/// <summary>
		/// 2017.1 and greater and Not Release
		/// </summary>
		public static bool IsReadGameIds(Version version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(2017) && !flags.IsRelease();
		}
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool IsReadGameId(Version version)
		{
			return version.IsGreaterEqual(2017);
		}
		
		public void Read(AssetReader reader)
		{
			Enabled = reader.ReadBoolean();
			InitializeOnStartup = reader.ReadBoolean();
			TestMode = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);

			if(IsReadEnabledPlatforms(reader.Version))
			{
				EnabledPlatforms = reader.ReadInt32();
			}
			if(IsReadIosGameId(reader.Version, reader.Flags))
			{
				IosGameId = reader.ReadString();
				AndroidGameId = reader.ReadString();
			}
#if UNIVERSAL
			if (IsReadGameIds(reader.Version, reader.Flags))
			{
				m_gameIds = new Dictionary<string, string>();
				m_gameIds.Read(reader);
			}
#endif
			if(IsReadGameId(reader.Version))
			{
				GameId = reader.ReadString();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Enabled", Enabled);
			node.Add("m_InitializeOnStartup", InitializeOnStartup);
			node.Add("m_TestMode", TestMode);
			node.Add("m_IosGameId", GetIosGameId(container.Version, container.Flags));
			node.Add("m_AndroidGameId", GetAndroidGameId(container.Version, container.Flags));
			node.Add("m_GameIds", GetGameIds(container.Version, container.Flags).ExportYAML());
			node.Add("m_GameId", GetGameId(container.Version));
			return node;
		}

		private string GetIosGameId(Version version, TransferInstructionFlags flags)
		{
			if(IsReadIosGameId(version, flags))
			{
				return IosGameId;
			}
			return string.Empty;
		}
		private string GetAndroidGameId(Version version, TransferInstructionFlags flags)
		{
			if (IsReadIosGameId(version, flags))
			{
				return AndroidGameId;
			}
			return string.Empty;
		}
		private IReadOnlyDictionary<string, string> GetGameIds(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadGameIds(version, flags))
			{
				return GameIds;
			}
#endif
			return new Dictionary<string, string>();
		}
		private string GetGameId(Version version)
		{
			return IsReadGameId(version) ? GameId : string.Empty;
		}

		public bool Enabled { get; private set; }
		public bool InitializeOnStartup { get; private set; }
		public bool TestMode { get; private set; }
		public int EnabledPlatforms { get; private set; }
		public string IosGameId { get; private set; }
		public string AndroidGameId { get; private set; }
#if UNIVERSAL
		public IReadOnlyDictionary<string,string> GameIds => m_gameIds;
#endif
		public string GameId { get; private set; }

#if UNIVERSAL
		private Dictionary<string,string> m_gameIds;
#endif
	}
}
