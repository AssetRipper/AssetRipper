using uTinyRipper.Classes.EditorSettingss;
using uTinyRipper.Converters;
using uTinyRipper.SerializedFiles;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	/// <summary>
	/// First introduced in 2.6.0
	/// </summary>
	public sealed class EditorSettings : Object
	{
		public EditorSettings(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		private EditorSettings(AssetInfo assetInfo, bool _) :
			base(assetInfo)
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

		public static int ToSerializedVersion(Version version)
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
		public static bool HasExternalVersionControl(Version version) => version.IsLess(4);
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasSerializationMode(Version version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 5.4.x and less
		/// </summary>
		public static bool HasWebSecurityEmulationEnabled(Version version) => version.IsLessEqual(5, 4);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasLineEndingsForNewScripts(Version version) => version.IsGreaterEqual(2017, 3);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasDefaultBehaviorMode(Version version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasPrefabRegularEnvironment(Version version) => version.IsGreaterEqual(2018, 3);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasSpritePackerMode(Version version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		public static bool HasSpritePackerPaddingPower(Version version) => version.IsGreaterEqual(5, 1);
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool HasEtcTextureCompressorBehavior(Version version) => version.IsGreaterEqual(2017, 2);
		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		public static bool HasProjectGenerationIncludedExtensions(Version version) => version.IsGreaterEqual(5, 1);
		/// <summary>
		/// 5.5.0 to 2018.3 exclusive
		/// </summary>
		public static bool HasUserGeneratedProjectSuffix(Version version) => version.IsLess(2018, 3) && version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool HasCollabEditorSettings(Version version) => version.IsGreaterEqual(2017, 1);
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasEnableTextureStreamingInEditMode(Version version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasEnableTextureStreamingInPlayMode(Version version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 2019.1.0b6 and greater
		/// </summary>
		public static bool HasAsyncShaderCompilation(Version version) => version.IsGreaterEqual(2019, 1, 0, VersionType.Beta, 6);
		/// <summary>
		/// 2019.3 and greater
		/// </summary>
		public static bool HasEnterPlayModeOptions(Version version) => version.IsGreaterEqual(2019, 3);
		/// <summary>
		/// 2019.2 and greater
		/// </summary>
		public static bool HasShowLightmapResolutionOverlay(Version version) => version.IsGreaterEqual(2019, 2);
		/// <summary>
		/// 2019.3 and greater
		/// </summary>
		public static bool HasUseLegacyProbeSampleCount(Version version) => version.IsGreaterEqual(2019, 3);
		/// <summary>
		/// 2019.3.0b7 and greater
		/// </summary>
		public static bool HasAssetPipelineMode(Version version) => version.IsGreaterEqual(2019, 3, 0, VersionType.Beta, 7);

		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		private static bool IsAlign1(Version version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 2019.2 and greater
		/// </summary>
		private static bool IsAlign2(Version version) => version.IsGreaterEqual(2019, 2);

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
			else
			{
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
			if (HasAssetPipelineMode(reader.Version))
			{
				AssetPipelineMode = (AssetPipelineMode)reader.ReadInt32();
				CacheServerMode = (CacheServerMode)reader.ReadInt32();
				CacheServerEndpoint = reader.ReadString();
				CacheServerNamespacePrefix = reader.ReadString();
				CacheServerEnableDownload = reader.ReadBoolean();
				CacheServerEnableUpload = reader.ReadBoolean();
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
			}
			return node;
		}

		private int GetSpritePackerPaddingPower(Version version)
		{
			return HasSpritePackerPaddingPower(version) ? SpritePackerPaddingPower : 1;
		}
		private int GetEtcTextureCompressorBehavior(Version version)
		{
			return HasEtcTextureCompressorBehavior(version) ? EtcTextureCompressorBehavior : 1;
		}
		private int GetEtcTextureFastCompressor(Version version)
		{
			return HasEtcTextureCompressorBehavior(version) ? EtcTextureFastCompressor : 1;
		}
		private int GetEtcTextureNormalCompressor(Version version)
		{
			return HasEtcTextureCompressorBehavior(version) ? EtcTextureNormalCompressor : 2;
		}
		private int GetEtcTextureBestCompressor(Version version)
		{
			return HasEtcTextureCompressorBehavior(version) ? EtcTextureBestCompressor : 4;
		}
		private string GetProjectGenerationIncludedExtensions(Version version)
		{
			string exts = HasProjectGenerationIncludedExtensions(version) ? ProjectGenerationIncludedExtensions : DefaultExtensions;
			return ToSerializedVersion(version) < 8 ? exts : $"{exts};{AsmrefExtension}";
		}
		private string GetProjectGenerationRootNamespace(Version version)
		{
			return HasProjectGenerationIncludedExtensions(version) ? ProjectGenerationRootNamespace : string.Empty;
		}
		private string GetUserGeneratedProjectSuffix(Version version)
		{
			return HasUserGeneratedProjectSuffix(version) ? UserGeneratedProjectSuffix : string.Empty;
		}
		private CollabEditorSettings GetCollabEditorSettings(Version version)
		{
			return HasCollabEditorSettings(version) ? CollabEditorSettings : new CollabEditorSettings(true);
		}
		private bool GetEnableTextureStreamingInEditMode(Version version)
		{
			return HasEnableTextureStreamingInEditMode(version) ? EnableTextureStreamingInEditMode : true;
		}
		private bool GetEnableTextureStreamingInPlayMode(Version version)
		{
			return HasEnableTextureStreamingInPlayMode(version) ? EnableTextureStreamingInPlayMode : true;
		}
		private bool GetAsyncShaderCompilation(Version version)
		{
			return HasAsyncShaderCompilation(version) ? AsyncShaderCompilation : true;
		}
		public EnterPlayModeOptions GetEnterPlayModeOptions(Version version)
		{
			return HasEnterPlayModeOptions(version) ? EnterPlayModeOptions : EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
		}
		private bool GetShowLightmapResolutionOverlay(Version version)
		{
			return HasShowLightmapResolutionOverlay(version) ? ShowLightmapResolutionOverlay : true;
		}
		private int GetUseLegacyProbeSampleCount(Version version)
		{
			return HasUseLegacyProbeSampleCount(version) ? UseLegacyProbeSampleCount : 1;
		}
		private string GetCacheServerEndpoint(Version version)
		{
			return HasAssetPipelineMode(version) ? CacheServerEndpoint : string.Empty;
		}
		private string GetCacheServerNamespacePrefix(Version version)
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
		public bool EnterPlayModeOptionsEnabled { get; set; }
		public EnterPlayModeOptions EnterPlayModeOptions { get; set; }
		public bool ShowLightmapResolutionOverlay { get; set; }
		public int UseLegacyProbeSampleCount { get; set; }
		public AssetPipelineMode AssetPipelineMode { get; set; }
		public CacheServerMode CacheServerMode { get; set; }
		public string CacheServerEndpoint { get; set; }
		public string CacheServerNamespacePrefix { get; set; }
		public bool CacheServerEnableDownload { get; set; }
		public bool CacheServerEnableUpload { get; set; }

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

		public PPtr<SceneAsset> PrefabRegularEnvironment;
		public PPtr<SceneAsset> PrefabUIEnvironment;
		public CollabEditorSettings CollabEditorSettings;

		private const string DefaultExtensions = "txt;xml;fnt;cd;asmdef;rsp";
		private const string AsmrefExtension = "asmref";
		private const string HiddenMeta = "Hidden Meta Files";
		private const string VisibleMeta = "Visible Meta Files";
	}
}
