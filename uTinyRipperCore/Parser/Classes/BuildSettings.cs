using System.Collections.Generic;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class BuildSettings : GlobalGameManager
	{
		public BuildSettings(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
		{
			// min version is 2nd
			return 2;
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasPreloadPlugin(Version version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasEnabledVRDevices(Version version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasBuildTags(Version version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// 5.6.0b3 and greater
		/// </summary>
		public static bool HasBuildGUID(Version version) => version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 3);
		/// <summary>
		/// Less than 5.3.0
		/// </summary>
		public static bool HasHasRenderTexture(Version version) => version.IsLess(5, 3);
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasIsNoWatermarkBuild(Version version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 3.1.0 and greater
		/// </summary>
		public static bool HasIsEducationalBuild(Version version) => version.IsGreaterEqual(3, 1);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasIsEmbedded(Version version) => version.IsGreaterEqual(4);
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool HasHasShadows(Version version) => version.IsGreaterEqual(2);
		/// <summary>
		/// 4.2.0 and greater
		/// </summary>
		public static bool HasHasSoftShadows(Version version) => version.IsGreaterEqual(4, 2);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasHasAdvancedVersion(Version version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasUsesOnMouseEvents(Version version) => version.IsGreater(4);
		/// <summary>
		/// 5.0.0 to 5.3.0 exclusive
		/// </summary>
		public static bool HasEnableMultipleDisplays(Version version) => version.IsGreater(5) && version.IsLess(5, 3);
		/// <summary>
		/// 4.6.2 to 5.0.0 exclusive
		/// </summary>
		public static bool HasHasOculusPlugin(Version version) => version.IsGreater(4, 6, 2) && version.IsLess(5);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasHasClusterRendering(Version version) => version.IsGreater(5);
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool HasVersion(Version version) => version.IsGreaterEqual(2);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasAuthToken(Version version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasRuntimeClassHashes(Version version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool IsRuntimeClassHashesUInt32(Version version) => version.IsLess(5);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasScriptHashes(Version version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		public static bool HasGraphicsAPIs(Version version) => version.IsGreaterEqual(5, 1);

		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		private static bool IsAlignBools(Version version) => version.IsGreaterEqual(3);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Scenes = reader.ReadStringArray();
			if (HasPreloadPlugin(reader.Version))
			{
				PreloadedPlugins = reader.ReadStringArray();
			}

			if (HasEnabledVRDevices(reader.Version))
			{
				EnabledVRDevices = reader.ReadStringArray();
			}
			if (HasBuildTags(reader.Version))
			{
				BuildTags = reader.ReadStringArray();
			}
			if (HasBuildGUID(reader.Version))
			{
				BuildGUID.Read(reader);
			}

			if (HasHasRenderTexture(reader.Version))
			{
				HasRenderTexture = reader.ReadBoolean();
			}
			HasPROVersion = reader.ReadBoolean();
			if (HasIsNoWatermarkBuild(reader.Version))
			{
				IsNoWatermarkBuild = reader.ReadBoolean();
				IsPrototypingBuild = reader.ReadBoolean();
			}
			if (HasIsEducationalBuild(reader.Version))
			{
				IsEducationalBuild = reader.ReadBoolean();
			}
			if (HasIsEmbedded(reader.Version))
			{
				IsEmbedded = reader.ReadBoolean();
			}
			HasPublishingRights = reader.ReadBoolean();
			if (HasHasShadows(reader.Version))
			{
				HasShadows = reader.ReadBoolean();
			}
			if (HasHasSoftShadows(reader.Version))
			{
				HasSoftShadows = reader.ReadBoolean();
				HasLocalLightShadows = reader.ReadBoolean();
			}
			if (HasHasAdvancedVersion(reader.Version))
			{
				HasAdvancedVersion = reader.ReadBoolean();
				EnableDynamicBatching = reader.ReadBoolean();
				IsDebugBuild = reader.ReadBoolean();
			}
			if (HasUsesOnMouseEvents(reader.Version))
			{
				UsesOnMouseEvents = reader.ReadBoolean();
			}
			if (HasEnableMultipleDisplays(reader.Version))
			{
				EnableMultipleDisplays =  reader.ReadBoolean();
			}
			if (HasHasOculusPlugin(reader.Version))
			{
				HasOculusPlugin = reader.ReadBoolean();
			}
			if (HasHasClusterRendering(reader.Version))
			{
				HasClusterRendering = reader.ReadBoolean();
			}
			if (IsAlignBools(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasVersion(reader.Version))
			{
				Version = reader.ReadString();
			}
			if (HasAuthToken(reader.Version))
			{
				AuthToken = reader.ReadString();
			}

			if (HasRuntimeClassHashes(reader.Version))
			{
				RuntimeClassHashes = new Dictionary<int, Hash128>();
				if (IsRuntimeClassHashesUInt32(reader.Version))
				{
					Dictionary<int, uint> runtimeClassHashes = new Dictionary<int, uint>();
					runtimeClassHashes.Read(reader);
					foreach (KeyValuePair<int, uint> kvp in runtimeClassHashes)
					{
						RuntimeClassHashes.Add(kvp.Key, new Hash128(kvp.Value));
					}
				}
				else
				{
					RuntimeClassHashes.Read(reader);
				}
			}
			if (HasScriptHashes(reader.Version))
			{
				ScriptHashes = new Dictionary<Hash128, Hash128>();
				ScriptHashes.Read(reader);
			}
			if (HasGraphicsAPIs(reader.Version))
			{
				GraphicsAPIs = reader.ReadInt32Array();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Levels previously
		/// </summary>
		public string[] Scenes { get; set; }
		public string[] PreloadedPlugins { get; set; }
		public string[] EnabledVRDevices { get; set; }
		public string[] BuildTags { get; set; }
		public bool HasRenderTexture { get; set; }
		public bool HasPROVersion { get; set; }
		public bool IsNoWatermarkBuild { get; set; }
		public bool IsPrototypingBuild { get; set; }
		public bool IsEducationalBuild { get; set; }
		public bool IsEmbedded { get; set; }
		public bool HasPublishingRights { get; set; }
		public bool HasShadows { get; set; }
		public bool HasSoftShadows { get; set; }
		public bool HasLocalLightShadows { get; set; }
		public bool HasAdvancedVersion { get; set; }
		public bool EnableDynamicBatching { get; set; }
		public bool IsDebugBuild { get; set; }
		public bool UsesOnMouseEvents { get; set; }
		public bool EnableMultipleDisplays { get; set; }
		public bool HasOculusPlugin { get; set; }
		public bool HasClusterRendering { get; set; }
		public string Version { get; set; } = string.Empty;
		public string AuthToken { get; set; } = string.Empty;
		public Dictionary<int, Hash128> RuntimeClassHashes { get; set; }
		public Dictionary<Hash128, Hash128> ScriptHashes { get; set; }
		public int[] GraphicsAPIs { get; set; }

		public UnityGUID BuildGUID;
	}
}
