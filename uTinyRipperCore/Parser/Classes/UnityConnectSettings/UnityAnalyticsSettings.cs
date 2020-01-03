using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.UnityConnectSettingss
{
	public struct UnityAnalyticsSettings : IAssetReadable, IYAMLExportable
	{
		public UnityAnalyticsSettings(bool _) :
			this()
		{
			InitializeOnStartup = true;
			TestEventUrl = string.Empty;
			TestConfigUrl = string.Empty;
		}

		/// <summary>
		/// Less than 2018.3
		/// </summary>
		public static bool HasTestEventUrl(Version version) => version.IsLess(2018, 3);

		public void Read(AssetReader reader)
		{
			Enabled = reader.ReadBoolean();
			InitializeOnStartup = reader.ReadBoolean();
			TestMode = reader.ReadBoolean();
			reader.AlignStream();

			if (HasTestEventUrl(reader.Version))
			{
				TestEventUrl = reader.ReadString();
				TestConfigUrl = reader.ReadString();
				reader.AlignStream();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(EnabledName, Enabled);
			if (HasTestEventUrl(container.ExportVersion))
			{
				node.Add(InitializeOnStartupName, InitializeOnStartup);
				node.Add(TestModeName, TestMode);
				node.Add(TestEventUrlName, GetTestEventUrl(container.Version));
				node.Add(TestConfigUrlName, GetTestConfigUrl(container.Version));
			}
			else
			{
				node.Add(TestModeName, TestMode);
				node.Add(InitializeOnStartupName, InitializeOnStartup);
			}
			return node;
		}

		private string GetTestEventUrl(Version version)
		{
			return HasTestEventUrl(version) ? TestEventUrl : string.Empty;
		}
		private string GetTestConfigUrl(Version version)
		{
			return HasTestEventUrl(version) ? TestConfigUrl : string.Empty;
		}

		public bool Enabled { get; set; }
		public bool InitializeOnStartup { get; set; }
		public bool TestMode { get; set; }
		public string TestEventUrl { get; set; }
		public string TestConfigUrl { get; set; }

		public const string EnabledName = "m_Enabled";
		public const string InitializeOnStartupName = "m_InitializeOnStartup";
		public const string TestModeName = "m_TestMode";
		public const string TestEventUrlName = "m_TestEventUrl";
		public const string TestConfigUrlName = "m_TestConfigUrl";
	}
}
