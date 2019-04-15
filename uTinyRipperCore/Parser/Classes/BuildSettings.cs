using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
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
		public static bool IsReadVersion(Version version)
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

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			m_scenes = reader.ReadStringArray();
			if (IsReadPreloadPlugin(reader.Version))
			{
				m_preloadedPlugins = reader.ReadStringArray();
			}

			if (IsReadEnabledVRDevices(reader.Version))
			{
				m_enabledVRDevices = reader.ReadStringArray();
			}
			if(IsReadBuildTags(reader.Version))
			{
				m_buildTags = reader.ReadStringArray();
				BuildGUID.Read(reader);
			}

			if (IsReadHasRenderTexture(reader.Version))
			{
				HasRenderTexture = reader.ReadBoolean();
			}
			HasPROVersion = reader.ReadBoolean();
			if (IsReadIsNoWatermarkBuild(reader.Version))
			{
				IsNoWatermarkBuild = reader.ReadBoolean();
				IsPrototypingBuild = reader.ReadBoolean();
			}
			if (IsReadIsEducationalBuild(reader.Version))
			{
				IsEducationalBuild = reader.ReadBoolean();
			}
			if (IsReadIsEmbedded(reader.Version))
			{
				IsEmbedded = reader.ReadBoolean();
			}
			HasPublishingRights = reader.ReadBoolean();
			if (IsReadHasShadows(reader.Version))
			{
				HasShadows = reader.ReadBoolean();
			}
			if (IsReadHasSoftShadows(reader.Version))
			{
				HasSoftShadows = reader.ReadBoolean();
				HasLocalLightShadows = reader.ReadBoolean();
			}
			if (IsReadHasAdvancedVersion(reader.Version))
			{
				HasAdvancedVersion = reader.ReadBoolean();
				EnableDynamicBatching = reader.ReadBoolean();
				IsDebugBuild = reader.ReadBoolean();
			}
			if (IsReadUsesOnMouseEvents(reader.Version))
			{
				UsesOnMouseEvents = reader.ReadBoolean();
			}
			if (IsReadEnableMultipleDisplays(reader.Version))
			{
				EnableMultipleDisplays =  reader.ReadBoolean();
			}
			if (IsReadHasOculusPlugin(reader.Version))
			{
				HasOculusPlugin = reader.ReadBoolean();
			}
			if (IsReadHasClusterRendering(reader.Version))
			{
				HasClusterRendering = reader.ReadBoolean();
			}
			if (IsAlignBools(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadVersion(reader.Version))
			{
				Version = reader.ReadString();
			}
			if (IsReadAuthToken(reader.Version))
			{
				AuthToken = reader.ReadString();
			}

			if (IsReadRuntimeClassHashes(reader.Version))
			{
				m_runtimeClassHashes = new Dictionary<int, Hash128>();
				if (IsRuntimeClassHashesUInt32(reader.Version))
				{
					Dictionary<int, uint> runtimeClassHashes = new Dictionary<int, uint>();
					runtimeClassHashes.Read(reader);
					foreach (KeyValuePair<int, uint> kvp in runtimeClassHashes)
					{
						m_runtimeClassHashes.Add(kvp.Key, new Hash128(kvp.Value));
					}
				}
				else
				{
					m_runtimeClassHashes.Read(reader);
				}
			}
			if (IsReadScriptHashes(reader.Version))
			{
				m_scriptHashes = new Dictionary<Hash128, Hash128>();
				m_scriptHashes.Read(reader);
			}
			if (IsReadGraphicsAPIs(reader.Version))
			{
				m_graphicsAPIs = reader.ReadInt32Array();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
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
		public string Version { get; private set; } = string.Empty;
		public string AuthToken { get; private set; } = string.Empty;
		public IReadOnlyDictionary<int, Hash128> RuntimeClassHashes => m_runtimeClassHashes;
		public IReadOnlyDictionary<Hash128, Hash128> ScriptHashes => m_scriptHashes;
		public IReadOnlyList<int> GraphicsAPIs => m_graphicsAPIs;

		public EngineGUID BuildGUID;

		private string[] m_scenes;
		private string[] m_preloadedPlugins;
		private string[] m_enabledVRDevices;
		private string[] m_buildTags;
		private Dictionary<int, Hash128> m_runtimeClassHashes;
		private Dictionary<Hash128, Hash128> m_scriptHashes;
		private int[] m_graphicsAPIs;
	}
}
