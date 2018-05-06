using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public sealed class BuildSettings : GlobalGameManager
	{
		public BuildSettings(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadPreloadPlugin(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadEnabledVRDevices(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadBuildTags(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
		/// <summary>
		/// Less than 5.3.0
		/// </summary>
		public static bool IsReadHasRenderTexture(Version version)
		{
			return version.IsLess(5, 3);
		}
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool IsReadIsNoWatermarkBuild(Version version)
		{
			return version.IsGreaterEqual(3, 5);
		}
		/// <summary>
		/// 3.1.0 and greater
		/// </summary>
		public static bool IsReadIsEducationalBuild(Version version)
		{
			return version.IsGreaterEqual(3, 1);
		}
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadIsEmbedded(Version version)
		{
			return version.IsGreaterEqual(4);
		}
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool IsReadHasShadows(Version version)
		{
			return version.IsGreaterEqual(2);
		}
		/// <summary>
		/// 4.2.0 and greater
		/// </summary>
		public static bool IsReadHasSoftShadows(Version version)
		{
			return version.IsGreaterEqual(4, 2);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadHasAdvancedVersion(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadUsesOnMouseEvents(Version version)
		{
			return version.IsGreater(4);
		}
		/// <summary>
		/// 5.0.0 to 5.3.0 exclusive
		/// </summary>
		public static bool IsReadEnableMultipleDisplays(Version version)
		{
			return version.IsGreater(5) && version.IsLess(5, 3);
		}
		/// <summary>
		/// 4.6.2 to 5.0.0 exclusive
		/// </summary>
		public static bool IsReadHasOculusPlugin(Version version)
		{
			return version.IsGreater(4, 6, 2) && version.IsLess(5);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadHasClusterRendering(Version version)
		{
			return version.IsGreater(5);
		}
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool IsReadBSVersion(Version version)
		{
			return version.IsGreaterEqual(2);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadAuthToken(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool IsReadRuntimeClassHashes(Version version)
		{
			return version.IsGreaterEqual(3, 5);
		}
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool IsRuntimeClassHashesUInt32(Version version)
		{
			return version.IsLess(5);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadScriptHashes(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		public static bool IsReadGraphicsAPIs(Version version)
		{
			return version.IsGreaterEqual(5, 1);
		}

		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		private static bool IsAlignBools(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		
		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

			// min version is 2nd
			return 2;
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			m_scenes = stream.ReadStringArray();
			if (IsReadPreloadPlugin(stream.Version))
			{
				m_preloadedPlugins = stream.ReadStringArray();
			}

			if (IsReadEnabledVRDevices(stream.Version))
			{
				m_enabledVRDevices = stream.ReadStringArray();
			}
			if(IsReadBuildTags(stream.Version))
			{
				m_buildTags = stream.ReadStringArray();
				BuildGUID.Read(stream);
			}

			if (IsReadHasRenderTexture(stream.Version))
			{
				HasRenderTexture = stream.ReadBoolean();
			}
			HasPROVersion = stream.ReadBoolean();
			if (IsReadIsNoWatermarkBuild(stream.Version))
			{
				IsNoWatermarkBuild = stream.ReadBoolean();
				IsPrototypingBuild = stream.ReadBoolean();
			}
			if (IsReadIsEducationalBuild(stream.Version))
			{
				IsEducationalBuild = stream.ReadBoolean();
			}
			if (IsReadIsEmbedded(stream.Version))
			{
				IsEmbedded = stream.ReadBoolean();
			}
			HasPublishingRights = stream.ReadBoolean();
			if (IsReadHasShadows(stream.Version))
			{
				HasShadows = stream.ReadBoolean();
			}
			if (IsReadHasSoftShadows(stream.Version))
			{
				HasSoftShadows = stream.ReadBoolean();
				HasLocalLightShadows = stream.ReadBoolean();
			}
			if (IsReadHasAdvancedVersion(stream.Version))
			{
				HasAdvancedVersion = stream.ReadBoolean();
				EnableDynamicBatching = stream.ReadBoolean();
				IsDebugBuild = stream.ReadBoolean();
			}
			if (IsReadUsesOnMouseEvents(stream.Version))
			{
				UsesOnMouseEvents = stream.ReadBoolean();
			}
			if (IsReadEnableMultipleDisplays(stream.Version))
			{
				EnableMultipleDisplays =  stream.ReadBoolean();
			}
			if (IsReadHasOculusPlugin(stream.Version))
			{
				HasOculusPlugin = stream.ReadBoolean();
			}
			if (IsReadHasClusterRendering(stream.Version))
			{
				HasClusterRendering = stream.ReadBoolean();
			}
			if (IsAlignBools(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}

			if (IsReadBSVersion(stream.Version))
			{
				BSVersion = stream.ReadStringAligned();
			}
			if (IsReadAuthToken(stream.Version))
			{
				AuthToken = stream.ReadStringAligned();
			}

			if (IsReadRuntimeClassHashes(stream.Version))
			{
				if (IsRuntimeClassHashesUInt32(stream.Version))
				{
					m_runtimeClassHashesUInt32 = new Dictionary<int, uint>();
					m_runtimeClassHashesUInt32.Read(stream);
				}
				else
				{
					m_runtimeClassHashes.Read(stream);
				}
			}
			if (IsReadScriptHashes(stream.Version))
			{
				m_scriptHashes.Read(stream);
			}
			if (IsReadGraphicsAPIs(stream.Version))
			{
				m_graphicsAPIs = stream.ReadInt32Array();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(container);

#warning TODO:
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Levels previously
		/// </summary>
		public IReadOnlyList<string> Scenes => m_scenes;
		public IReadOnlyList<string> PreloadedPlugins => m_preloadedPlugins;
		public IReadOnlyList<string> EnabledVRDevices => m_enabledVRDevices;
		public IReadOnlyList<string> BuildTags => m_buildTags;
		public bool HasRenderTexture { get; private set; }
		public bool HasPROVersion { get; private set; }
		public bool IsNoWatermarkBuild { get; private set; }
		public bool IsPrototypingBuild { get; private set; }
		public bool IsEducationalBuild { get; private set; }
		public bool IsEmbedded { get; private set; }
		public bool HasPublishingRights { get; private set; }
		public bool HasShadows { get; private set; }
		public bool HasSoftShadows { get; private set; }
		public bool HasLocalLightShadows { get; private set; }
		public bool HasAdvancedVersion { get; private set; }
		public bool EnableDynamicBatching { get; private set; }
		public bool IsDebugBuild { get; private set; }
		public bool UsesOnMouseEvents { get; private set; }
		public bool EnableMultipleDisplays { get; private set; }
		public bool HasOculusPlugin { get; private set; }
		public bool HasClusterRendering { get; private set; }
		public string BSVersion { get; private set; } = string.Empty;
		public string AuthToken { get; private set; } = string.Empty;
		public IReadOnlyDictionary<int, Hash128> RuntimeClassHashes => m_runtimeClassHashes;
		public IReadOnlyDictionary<Hash128, Hash128> ScriptHashes => m_scriptHashes;
		public IReadOnlyList<int> GraphicsAPIs => m_graphicsAPIs;

		public UtinyGUID BuildGUID;
		
		private readonly Dictionary<int, Hash128> m_runtimeClassHashes = new Dictionary<int, Hash128>();
		private readonly Dictionary<Hash128, Hash128> m_scriptHashes = new Dictionary<Hash128, Hash128>();

		private string[] m_scenes;
		private string[] m_preloadedPlugins;
		private string[] m_enabledVRDevices;
		private string[] m_buildTags;
		private Dictionary<int, uint> m_runtimeClassHashesUInt32;
		private int[] m_graphicsAPIs;
	}
}
