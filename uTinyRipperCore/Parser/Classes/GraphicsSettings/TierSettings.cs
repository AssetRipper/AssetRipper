using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.GraphicsSettingss
{
	/// <summary>
	/// BuildTargetShaderSettings previously
	/// </summary>
	public struct TierSettings : IAssetReadable, IYAMLExportable
	{
		public TierSettings(PlatformShaderSettings settings, Platform platfrom, GraphicsTier tier, Version version, TransferInstructionFlags flags)
		{
			BuildTarget = platfrom.PlatformToBuildGroup();
			Tier = tier;
			Settings = new TierGraphicsSettingsEditor(settings, version, flags);
			Automatic = false;
		}

		public TierSettings(TierGraphicsSettings settings, Platform platfrom, GraphicsTier tier)
		{
			BuildTarget = platfrom.PlatformToBuildGroup();
			Tier = tier;
			Settings = new TierGraphicsSettingsEditor(settings);
			Automatic = false;
		}

		public static int ToSerializedVersion(Version version)
		{
			// changed default value for Settings.Prefer32BitShadowMaps to platform specific
			if (version.IsGreaterEqual(2017))
			{
				return 5;
			}
			// changed default value for Settings.RealtimeGICPUUsage to platform specific
			if (version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 7))
			{
				return 4;
			}
			// changed default value for Settings.Hdr to platform specific
			/*if (version.IsGreaterEqual(5, 6, 0, some alpha or beta))
			{
				return 3;
			}*/
			// BuildTarget converted to enum
			if (version.IsGreaterEqual(5, 5))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasTier(Version version) => version.IsGreaterEqual(5, 4);

		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		private static bool HasBuildTargetString(Version version) => version.IsLess(5, 5);
		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		private static bool HasPlatfromSettings(Version version) => version.IsLess(5, 5);

		public void Read(AssetReader reader)
		{
			if (HasBuildTargetString(reader.Version))
			{
				string buildTarget = reader.ReadString();
				BuildTarget = StringToBuildGroup(buildTarget);
			}
			else
			{
				BuildTarget = (BuildTargetGroup)reader.ReadInt32();
			}
			if (HasTier(reader.Version))
			{
				Tier = (GraphicsTier)reader.ReadInt32();
			}

			if (HasPlatfromSettings(reader.Version))
			{
				PlatformShaderSettings settings = reader.ReadAsset<PlatformShaderSettings>();
				Settings = new TierGraphicsSettingsEditor(settings, reader.Version, reader.Flags);
			}
			else
			{
				Settings.Read(reader);
			}

			Automatic = reader.ReadBoolean();
			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(BuildTargetName, (int)BuildTarget);
			node.Add(TierName, (int)GetTier(container.Version));
			node.Add(SettingsName, Settings.ExportYAML(container));
			node.Add(AutomaticName, Automatic);
			return node;
		}

		private GraphicsTier GetTier(Version version)
		{
			return HasTier(version) ? Tier : GraphicsTier.Tier1;
		}

		private static BuildTargetGroup StringToBuildGroup(string group)
		{
			switch(group)
			{
				case "Standalone":
					return BuildTargetGroup.Standalone;
				case "Web":
					return BuildTargetGroup.WebPlayer;
				case "iPhone":
					return BuildTargetGroup.iPhone;
				case "Android":
					return BuildTargetGroup.Android;
				case "WebGL":
					return BuildTargetGroup.WebGL;
				case "Windows Store Apps":
					return BuildTargetGroup.WSA;
				case "Tizen":
					return BuildTargetGroup.Tizen;
				case "PSP2":
					return BuildTargetGroup.PSP2;
				case "PS4":
					return BuildTargetGroup.PS4;
				case "PSM":
					return BuildTargetGroup.PSM;
				case "XboxOne":
					return BuildTargetGroup.XboxOne;
				case "Samsung TV":
					return BuildTargetGroup.SamsungTV;
				case "Nintendo 3DS":
					return BuildTargetGroup.N3DS;
				case "WiiU":
					return BuildTargetGroup.WiiU;
				case "tvOS":
					return BuildTargetGroup.tvOS;

				default:
					return BuildTargetGroup.Standalone;
			}
		}

		public BuildTargetGroup BuildTarget { get; set; }
		public GraphicsTier Tier { get; set; }
		public bool Automatic { get; set; }

		public const string BuildTargetName = "m_BuildTarget";
		public const string TierName = "m_Tier";
		public const string SettingsName = "m_Settings";
		public const string AutomaticName = "m_Automatic";

		/// <summary>
		/// ShaderSettings previously
		/// </summary>
		public TierGraphicsSettingsEditor Settings;
	}
}
