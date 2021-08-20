using AssetRipper.Core.Project;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Utils.Extensions;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.EditorSettings
{
	/// <summary>
	/// First introduced in 2.6.0
	/// </summary>
	public sealed class EditorSettings : Object.Object
	{
		public EditorSettings(AssetInfo assetInfo) : base(assetInfo) { }

		private EditorSettings(AssetInfo assetInfo, bool _) : base(assetInfo)
		{
			ExternalVersionControlSupport = VisibleMeta;
			SerializationMode = SerializationMode.ForceText;
			SpritePackerPaddingPower = 1;
			EtcTextureCompressorBehavior = 1;
			EtcTextureFastCompressor = 1;
			EtcTextureNormalCompressor = 2;
			EtcTextureBestCompressor = 4;
			ProjectGenerationIncludedExtensions = DefaultExtensions;
			ProjectGenerationRootNamespace = string.Empty;
			UserGeneratedProjectSuffix = string.Empty;
			CollabEditorSettings = new CollabEditorSettings(true);
			EnableTextureStreamingInEditMode = true;
			EnableTextureStreamingInPlayMode = true;
			AsyncShaderCompilation = true;
			AssetPipelineMode = AssetPipelineMode.Version1;
			CacheServerMode = CacheServerMode.AsPreferences;
			CacheServerEndpoint = string.Empty;
			CacheServerNamespacePrefix = "default";
			CacheServerEnableDownload = false;
			CacheServerEnableUpload = false;
		}

		public static EditorSettings CreateVirtualInstance(VirtualSerializedFile virtualFile)
		{
			return virtualFile.CreateAsset((assetInfo) => new EditorSettings(assetInfo, true));
		}

		public static int ToSerializedVersion(UnityVersion version)
		{
			// UseLegacyProbeSampleCount default value has been changed from 1 to custom?
			if (version.IsGreaterEqual(2019, 3))
			{
				return 9;
			}
			// 'asmref' has been added to default ProjectGenerationIncludedExtensions
			if (version.IsGreaterEqual(2019, 2))
			{
				return 8;
			}
			// LineEndingsForNewScripts default value changed (Unix to OSNative)
			if (version.IsGreaterEqual(2017, 3))
			{
				return 7;
			}

			// EtcTexture default values changed
			/*if (version.IsGreaterEqual(unknown version))
			{
				return 6;
			}*/

			// EtcTexture default values changed
			if (version.IsGreaterEqual(2017, 2))
			{
				return 5;
			}
			// Asset Server deprecated so ExternalVersionControlSupport replaced
			if (version.IsGreaterEqual(2017))
			{
				return 4;
			}
			// ExternalVersionControlSupport string values changed
			if (version.IsGreaterEqual(4, 3))
			{
				return 3;
			}
			// ExternalVersionControlSupport enum has converted to string 
			if (version.IsGreaterEqual(4))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		public static bool HasExternalVersionControl(UnityVersion version) => version.IsLess(4);
		/// <summary>
		/// Less than 2020
		/// </summary>
		public static bool HasExternalVersionControlSupport(UnityVersion version) => version.IsLess(2020);
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasSerializationMode(UnityVersion version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 5.4.x and less
		/// </summary>
		public static bool HasWebSecurityEmulationEnabled(UnityVersion version) => version.IsLessEqual(5, 4);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasLineEndingsForNewScripts(UnityVersion version) => version.IsGreaterEqual(2017, 3);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasDefaultBehaviorMode(UnityVersion version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasPrefabRegularEnvironment(UnityVersion version) => version.IsGreaterEqual(2018, 3);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasSpritePackerMode(UnityVersion version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		public static bool HasSpritePackerPaddingPower(UnityVersion version) => version.IsGreaterEqual(5, 1);
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool HasEtcTextureCompressorBehavior(UnityVersion version) => version.IsGreaterEqual(2017, 2);
		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		public static bool HasProjectGenerationIncludedExtensions(UnityVersion version) => version.IsGreaterEqual(5, 1);
		/// <summary>
		/// 5.5.0 to 2018.3 exclusive
		/// </summary>
		public static bool HasUserGeneratedProjectSuffix(UnityVersion version) => version.IsLess(2018, 3) && version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 2017.1 and up but less than 2020
		/// </summary>
		public static bool HasCollabEditorSettings(UnityVersion version) => version.IsGreaterEqual(2017, 1) && version.IsLess(2020);
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasEnableTextureStreamingInEditMode(UnityVersion version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasEnableTextureStreamingInPlayMode(UnityVersion version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 2019.1.0b6 and greater
		/// </summary>
		public static bool HasAsyncShaderCompilation(UnityVersion version) => version.IsGreaterEqual(2019, 1, 0, UnityVersionType.Beta, 6);
		/// <summary>
		/// 2020 and greater
		/// </summary>
		public static bool HasCachingShaderPreprocessor(UnityVersion version) => version.IsGreaterEqual(2020);
		/// <summary>
		/// 2020.2 and greater
		/// </summary>
		public static bool HasPrefabModeAllowAutoSave(UnityVersion version) => version.IsGreaterEqual(2020, 2);
		/// <summary>
		/// 2019.3 and greater
		/// </summary>
		public static bool HasEnterPlayModeOptions(UnityVersion version) => version.IsGreaterEqual(2019, 3);
		/// <summary>
		/// 2020 and greater
		/// </summary>
		public static bool HasGameObjectAndAssetNamingOptions(UnityVersion version) => version.IsGreaterEqual(2020);
		/// <summary>
		/// 2019.2 and greater but less than 2020
		/// </summary>
		public static bool HasShowLightmapResolutionOverlay(UnityVersion version) => version.IsGreaterEqual(2019, 2) && version.IsLess(2020);
		/// <summary>
		/// 2019.3 and greater
		/// </summary>
		public static bool HasUseLegacyProbeSampleCount(UnityVersion version) => version.IsGreaterEqual(2019, 3);
		/// <summary>
		/// 2020 and greater
		/// </summary>
		public static bool HasSerializedMappingsOneLineAndDisableCookiesInLightmapper(UnityVersion version) => version.IsGreaterEqual(2020);
		/// <summary>
		/// 2019.3.0b7 and greater
		/// </summary>
		public static bool HasAssetPipelineMode(UnityVersion version) => version.IsGreaterEqual(2019, 3, 0, UnityVersionType.Beta, 7);
		/// <summary>
		/// 2020 and greater
		/// </summary>
		public static bool HasCacheServerAuthAndTls(UnityVersion version) => version.IsGreaterEqual(2020);

		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		private static bool IsAlign1(UnityVersion version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 2019.2 and greater
		/// </summary>
		private static bool IsAlign2(UnityVersion version) => version.IsGreaterEqual(2019, 2);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasExternalVersionControl(reader.Version))
			{
				ExternalVersionControl support = (ExternalVersionControl)reader.ReadInt32();
				switch (support)
				{
					case ExternalVersionControl.AutoDetect:
						ExternalVersionControlSupport = "Auto detect";
						break;
					case ExternalVersionControl.Disabled:
						ExternalVersionControlSupport = HiddenMeta;
						break;
					case ExternalVersionControl.Generic:
					case ExternalVersionControl.AssetServer:
						ExternalVersionControlSupport = VisibleMeta;
						break;
					case ExternalVersionControl.Subversion:
					case ExternalVersionControl.Perforce:
						ExternalVersionControlSupport = support.ToString();
						break;

					default:
						ExternalVersionControlSupport = HiddenMeta;
						break;
				}
			}
			else if(HasExternalVersionControlSupport(reader.Version))
			{
				//Removed in 2020.
				ExternalVersionControlSupport = reader.ReadString();
				switch (ExternalVersionControlSupport)
				{
					case "Disabled":
						ExternalVersionControlSupport = HiddenMeta;
						break;
					case "Meta Files":
					case "Asset Server":
						ExternalVersionControlSupport = VisibleMeta;
						break;
				}
			}
			if (HasSerializationMode(reader.Version))
			{
				SerializationMode = (SerializationMode)reader.ReadInt32();
			}
			if (HasWebSecurityEmulationEnabled(reader.Version))
			{
				WebSecurityEmulationEnabled = reader.ReadInt32();
				WebSecurityEmulationHostUrl = reader.ReadString();
			}
			reader.AlignStream();

			if (HasLineEndingsForNewScripts(reader.Version))
			{
				LineEndingsForNewScripts = (LineEndingsMode)reader.ReadInt32();
			}
			if (HasDefaultBehaviorMode(reader.Version))
			{
				DefaultBehaviorMode = (EditorBehaviorMode)reader.ReadInt32();
			}
			if (HasPrefabRegularEnvironment(reader.Version))
			{
				PrefabRegularEnvironment.Read(reader);
				PrefabUIEnvironment.Read(reader);
			}
			if (HasSpritePackerMode(reader.Version))
			{
				SpritePackerMode = (SpritePackerMode)reader.ReadInt32();
			}

			if (HasSpritePackerPaddingPower(reader.Version))
			{
				SpritePackerPaddingPower = reader.ReadInt32();
			}
			if (HasEtcTextureCompressorBehavior(reader.Version))
			{
				EtcTextureCompressorBehavior = reader.ReadInt32();
				EtcTextureFastCompressor = reader.ReadInt32();
				EtcTextureNormalCompressor = reader.ReadInt32();
				EtcTextureBestCompressor = reader.ReadInt32();
			}
			if (HasProjectGenerationIncludedExtensions(reader.Version))
			{
				ProjectGenerationIncludedExtensions = reader.ReadString();
				ProjectGenerationRootNamespace = reader.ReadString();
			}
			if (HasUserGeneratedProjectSuffix(reader.Version))
			{
				UserGeneratedProjectSuffix = reader.ReadString();
			}
			if (HasCollabEditorSettings(reader.Version))
			{
				CollabEditorSettings.Read(reader);
			}
			if (HasEnableTextureStreamingInEditMode(reader.Version))
			{
				EnableTextureStreamingInEditMode = reader.ReadBoolean();
			}
			if (HasEnableTextureStreamingInPlayMode(reader.Version))
			{
				EnableTextureStreamingInPlayMode = reader.ReadBoolean();
			}
			if (HasAsyncShaderCompilation(reader.Version))
			{
				AsyncShaderCompilation = reader.ReadBoolean();
			}

			if (HasCachingShaderPreprocessor(reader.Version))
			{
				CachingShaderPreprocessor = reader.ReadBoolean();
			}

			if (HasPrefabModeAllowAutoSave(reader.Version))
			{
				PrefabModeAllowAutoSave = reader.ReadBoolean();
			}
			if (IsAlign1(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasEnterPlayModeOptions(reader.Version))
			{
				EnterPlayModeOptionsEnabled = reader.ReadBoolean();
				reader.AlignStream();

				EnterPlayModeOptions = (EnterPlayModeOptions)reader.ReadInt32();
			}

			if (HasGameObjectAndAssetNamingOptions(reader.Version))
			{
				GameObjectNamingDigits = reader.ReadInt32();
				GameObjectNamingScheme = reader.ReadInt32();
				AssetNamingUsesSpace = reader.ReadBoolean();
			}
			if (HasShowLightmapResolutionOverlay(reader.Version))
			{
				ShowLightmapResolutionOverlay = reader.ReadBoolean();
			}
			if (IsAlign2(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasUseLegacyProbeSampleCount(reader.Version))
			{
				UseLegacyProbeSampleCount = reader.ReadInt32();
				reader.AlignStream();
			}

			if (HasSerializedMappingsOneLineAndDisableCookiesInLightmapper(reader.Version))
			{
				SerializeInlineMappingsOnOneLine = reader.ReadBoolean();
				DisableCookiesInLightmapper = reader.ReadBoolean();
			}
			
			if (HasAssetPipelineMode(reader.Version))
			{
				AssetPipelineMode = (AssetPipelineMode)reader.ReadInt32();
				CacheServerMode = (CacheServerMode)reader.ReadInt32();
				CacheServerEndpoint = reader.ReadString();
				CacheServerNamespacePrefix = reader.ReadString();
				CacheServerEnableDownload = reader.ReadBoolean();
				CacheServerEnableUpload = reader.ReadBoolean();

				if (HasCacheServerAuthAndTls(reader.Version))
				{
					CacheServerEnableAuth = reader.ReadBoolean();
					CacheServerEnableTls = reader.ReadBoolean();
				}
				
				reader.AlignStream();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(ExternalVersionControlSupportName, ExternalVersionControlSupport);
			node.Add(SerializationModeName, (int)SerializationMode);
			node.Add(LineEndingsForNewScriptsName, (int)LineEndingsForNewScripts);
			node.Add(DefaultBehaviorModeName, (int)DefaultBehaviorMode);
			if (HasPrefabRegularEnvironment(container.ExportVersion))
			{
				node.Add(PrefabRegularEnvironmentName, PrefabRegularEnvironment.ExportYAML(container));
				node.Add(PrefabUIEnvironmentName, PrefabUIEnvironment.ExportYAML(container));
			}
			node.Add(SpritePackerModeName, (int)SpritePackerMode);
			node.Add(SpritePackerPaddingPowerName, GetSpritePackerPaddingPower(container.Version));
			node.Add(EtcTextureCompressorBehaviorName, GetEtcTextureCompressorBehavior(container.Version));
			node.Add(EtcTextureFastCompressorName, GetEtcTextureFastCompressor(container.Version));
			node.Add(EtcTextureNormalCompressorName, GetEtcTextureNormalCompressor(container.Version));
			node.Add(EtcTextureBestCompressorName, GetEtcTextureBestCompressor(container.Version));
			node.Add(ProjectGenerationIncludedExtensionsName, GetProjectGenerationIncludedExtensions(container.Version));
			node.Add(ProjectGenerationRootNamespaceName, GetProjectGenerationRootNamespace(container.Version));
			if (HasUserGeneratedProjectSuffix(container.ExportVersion))
			{
				node.Add(UserGeneratedProjectSuffixName, GetUserGeneratedProjectSuffix(container.Version));
			}
			node.Add(CollabEditorSettingsName, GetCollabEditorSettings(container.Version).ExportYAML(container));
			if (HasEnableTextureStreamingInEditMode(container.ExportVersion))
			{
				node.Add(EnableTextureStreamingInEditModeName, GetEnableTextureStreamingInEditMode(container.Version));
			}
			if (HasEnableTextureStreamingInPlayMode(container.ExportVersion))
			{
				node.Add(EnableTextureStreamingInPlayModeName, GetEnableTextureStreamingInPlayMode(container.Version));
			}
			if (HasAsyncShaderCompilation(container.ExportVersion))
			{
				node.Add(AsyncShaderCompilationName, GetAsyncShaderCompilation(container.Version));
			}
			if (HasCachingShaderPreprocessor(container.ExportVersion))
			{
				node.Add(CachingShaderPreprocessorName, CachingShaderPreprocessor);
			}

			if (HasPrefabModeAllowAutoSave(container.ExportVersion))
			{
				node.Add(PrefabModeAllowAutoSaveName, PrefabModeAllowAutoSave);
			}
			if (HasEnterPlayModeOptions(container.ExportVersion))
			{
				node.Add(EnterPlayModeOptionsEnabledName, EnterPlayModeOptionsEnabled);
				node.Add(EnterPlayModeOptionsName, (int)GetEnterPlayModeOptions(container.Version));
			}
			if (HasShowLightmapResolutionOverlay(container.ExportVersion))
			{
				node.Add(ShowLightmapResolutionOverlayName, GetShowLightmapResolutionOverlay(container.Version));
			}
			if (HasUseLegacyProbeSampleCount(container.ExportVersion))
			{
				node.Add(UseLegacyProbeSampleCountName, GetUseLegacyProbeSampleCount(container.Version));
			}
			if (HasAssetPipelineMode(container.ExportVersion))
			{
				node.Add(AssetPipelineModeName, (int)AssetPipelineMode);
				node.Add(CacheServerModeName, (int)CacheServerMode);
				node.Add(CacheServerEndpointName, GetCacheServerEndpoint(container.Version));
				node.Add(CacheServerNamespacePrefixName, GetCacheServerNamespacePrefix(container.Version));
				node.Add(CacheServerEnableDownloadName, CacheServerEnableDownload);
				node.Add(CacheServerEnableUploadName, CacheServerEnableUpload);

				if (HasCacheServerAuthAndTls(container.ExportVersion))
				{
					node.Add(CacheServerEnableAuthName, CacheServerEnableAuth);
					node.Add(CacheServerEnableTlsName, CacheServerEnableTls);
				}
			}
			return node;
		}

		private int GetSpritePackerPaddingPower(UnityVersion version)
		{
			return HasSpritePackerPaddingPower(version) ? SpritePackerPaddingPower : 1;
		}
		private int GetEtcTextureCompressorBehavior(UnityVersion version)
		{
			return HasEtcTextureCompressorBehavior(version) ? EtcTextureCompressorBehavior : 1;
		}
		private int GetEtcTextureFastCompressor(UnityVersion version)
		{
			return HasEtcTextureCompressorBehavior(version) ? EtcTextureFastCompressor : 1;
		}
		private int GetEtcTextureNormalCompressor(UnityVersion version)
		{
			return HasEtcTextureCompressorBehavior(version) ? EtcTextureNormalCompressor : 2;
		}
		private int GetEtcTextureBestCompressor(UnityVersion version)
		{
			return HasEtcTextureCompressorBehavior(version) ? EtcTextureBestCompressor : 4;
		}
		private string GetProjectGenerationIncludedExtensions(UnityVersion version)
		{
			string exts = HasProjectGenerationIncludedExtensions(version) ? ProjectGenerationIncludedExtensions : DefaultExtensions;
			return ToSerializedVersion(version) < 8 ? exts : $"{exts};{AsmrefExtension}";
		}
		private string GetProjectGenerationRootNamespace(UnityVersion version)
		{
			return HasProjectGenerationIncludedExtensions(version) ? ProjectGenerationRootNamespace : string.Empty;
		}
		private string GetUserGeneratedProjectSuffix(UnityVersion version)
		{
			return HasUserGeneratedProjectSuffix(version) ? UserGeneratedProjectSuffix : string.Empty;
		}
		private CollabEditorSettings GetCollabEditorSettings(UnityVersion version)
		{
			return HasCollabEditorSettings(version) ? CollabEditorSettings : new CollabEditorSettings(true);
		}
		private bool GetEnableTextureStreamingInEditMode(UnityVersion version)
		{
			return HasEnableTextureStreamingInEditMode(version) ? EnableTextureStreamingInEditMode : true;
		}
		private bool GetEnableTextureStreamingInPlayMode(UnityVersion version)
		{
			return HasEnableTextureStreamingInPlayMode(version) ? EnableTextureStreamingInPlayMode : true;
		}
		private bool GetAsyncShaderCompilation(UnityVersion version)
		{
			return HasAsyncShaderCompilation(version) ? AsyncShaderCompilation : true;
		}
		public EnterPlayModeOptions GetEnterPlayModeOptions(UnityVersion version)
		{
			return HasEnterPlayModeOptions(version) ? EnterPlayModeOptions : EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
		}
		private bool GetShowLightmapResolutionOverlay(UnityVersion version)
		{
			return HasShowLightmapResolutionOverlay(version) ? ShowLightmapResolutionOverlay : true;
		}
		private int GetUseLegacyProbeSampleCount(UnityVersion version)
		{
			return HasUseLegacyProbeSampleCount(version) ? UseLegacyProbeSampleCount : 1;
		}
		private string GetCacheServerEndpoint(UnityVersion version)
		{
			return HasAssetPipelineMode(version) ? CacheServerEndpoint : string.Empty;
		}
		private string GetCacheServerNamespacePrefix(UnityVersion version)
		{
			return HasAssetPipelineMode(version) ? CacheServerNamespacePrefix : "default";
		}

		public string ExternalVersionControlSupport { get; set; }
		public SerializationMode SerializationMode { get; set; }
		public int WebSecurityEmulationEnabled { get; set; }
		public string WebSecurityEmulationHostUrl { get; set; }
		public LineEndingsMode LineEndingsForNewScripts { get; set; }
		public EditorBehaviorMode DefaultBehaviorMode { get; set; }
		public SpritePackerMode SpritePackerMode { get; set; }
		public int SpritePackerPaddingPower { get; set; }
		public int EtcTextureCompressorBehavior { get; set; }
		public int EtcTextureFastCompressor { get; set; }
		public int EtcTextureNormalCompressor { get; set; }
		public int EtcTextureBestCompressor { get; set; }
		public string ProjectGenerationIncludedExtensions { get; set; }
		public string ProjectGenerationRootNamespace { get; set; }
		public string UserGeneratedProjectSuffix { get; set; }
		public bool EnableTextureStreamingInEditMode { get; set; }
		public bool EnableTextureStreamingInPlayMode { get; set; }
		public bool AsyncShaderCompilation { get; set; }
		public bool CachingShaderPreprocessor { get; set; }
		public bool PrefabModeAllowAutoSave { get; set; }
		public bool EnterPlayModeOptionsEnabled { get; set; }
		public EnterPlayModeOptions EnterPlayModeOptions { get; set; }
		public int GameObjectNamingDigits { get; set; }
		public int GameObjectNamingScheme { get; set; }
		public bool AssetNamingUsesSpace { get; set; }
		public bool ShowLightmapResolutionOverlay { get; set; }
		public int UseLegacyProbeSampleCount { get; set; }
		public bool SerializeInlineMappingsOnOneLine { get; set; }
		public bool DisableCookiesInLightmapper { get; set; }
		public AssetPipelineMode AssetPipelineMode { get; set; }
		public CacheServerMode CacheServerMode { get; set; }
		public string CacheServerEndpoint { get; set; }
		public string CacheServerNamespacePrefix { get; set; }
		public bool CacheServerEnableDownload { get; set; }
		public bool CacheServerEnableUpload { get; set; }
		public bool CacheServerEnableAuth { get; set; }
		public bool CacheServerEnableTls { get; set; }

		public const string ExternalVersionControlSupportName = "m_ExternalVersionControlSupport";
		public const string SerializationModeName = "m_SerializationMode";
		public const string LineEndingsForNewScriptsName = "m_LineEndingsForNewScripts";
		public const string DefaultBehaviorModeName = "m_DefaultBehaviorMode";
		public const string PrefabRegularEnvironmentName = "m_PrefabRegularEnvironment";
		public const string PrefabUIEnvironmentName = "m_PrefabUIEnvironment";
		public const string SpritePackerModeName = "m_SpritePackerMode";
		public const string SpritePackerPaddingPowerName = "m_SpritePackerPaddingPower";
		public const string EtcTextureCompressorBehaviorName = "m_EtcTextureCompressorBehavior";
		public const string EtcTextureFastCompressorName = "m_EtcTextureFastCompressor";
		public const string EtcTextureNormalCompressorName = "m_EtcTextureNormalCompressor";
		public const string EtcTextureBestCompressorName = "m_EtcTextureBestCompressor";
		public const string ProjectGenerationIncludedExtensionsName = "m_ProjectGenerationIncludedExtensions";
		public const string ProjectGenerationRootNamespaceName = "m_ProjectGenerationRootNamespace";
		public const string UserGeneratedProjectSuffixName = "m_UserGeneratedProjectSuffix";
		public const string CollabEditorSettingsName = "m_CollabEditorSettings";
		public const string EnableTextureStreamingInEditModeName = "m_EnableTextureStreamingInEditMode";
		public const string EnableTextureStreamingInPlayModeName = "m_EnableTextureStreamingInPlayMode";
		public const string AsyncShaderCompilationName = "m_AsyncShaderCompilation";
		public const string CachingShaderPreprocessorName = "m_CachingShaderPreprocessor";
		public const string PrefabModeAllowAutoSaveName = "m_PrefabModeAllowAutoSave";
		public const string EnterPlayModeOptionsEnabledName = "m_EnterPlayModeOptionsEnabled";
		public const string EnterPlayModeOptionsName = "m_EnterPlayModeOptions";
		public const string ShowLightmapResolutionOverlayName = "m_ShowLightmapResolutionOverlay";
		public const string UseLegacyProbeSampleCountName = "m_UseLegacyProbeSampleCount";
		public const string AssetPipelineModeName = "m_AssetPipelineMode";
		public const string CacheServerModeName = "m_CacheServerMode";
		public const string CacheServerEndpointName = "m_CacheServerEndpoint";
		public const string CacheServerNamespacePrefixName = "m_CacheServerNamespacePrefix";
		public const string CacheServerEnableDownloadName = "m_CacheServerEnableDownload";
		public const string CacheServerEnableUploadName = "m_CacheServerEnableUpload";
		public const string CacheServerEnableAuthName = "m_CacheServerEnableAuth";
		public const string CacheServerEnableTlsName = "m_CacheServerEnableTls";

		public PPtr<SceneAsset> PrefabRegularEnvironment = new();
		public PPtr<SceneAsset> PrefabUIEnvironment = new();
		public CollabEditorSettings CollabEditorSettings = new();

		private const string DefaultExtensions = "txt;xml;fnt;cd;asmdef;rsp";
		private const string AsmrefExtension = "asmref";
		private const string HiddenMeta = "Hidden Meta Files";
		private const string VisibleMeta = "Visible Meta Files";
	}
}
