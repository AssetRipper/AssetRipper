using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.EditorSettingss;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class EditorSettings : Object
	{
		public EditorSettings(AssetInfo assetInfo):
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
		}

		public static EditorSettings CreateVirtualInstance(VirtualSerializedFile virtualFile)
		{
			return virtualFile.CreateAsset((assetInfo) => new EditorSettings(assetInfo, true));
		}

		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadExternalVersionControl(Version version)
		{
			return version.IsGreaterEqual(4);
		}
		/// <summary>
		/// 5.4.x and less
		/// </summary>
		public static bool IsReadWebSecurityEmulationEnabled(Version version)
		{
			return version.IsLessEqual(5, 4);
		}
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsReadLineEndingsForNewScripts(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadDefaultBehaviorMode(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadPrefabRegularEnvironment(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadSpritePackerMode(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		public static bool IsReadSpritePackerPaddingPower(Version version)
		{
			return version.IsGreaterEqual(5, 1);
		}
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool IsReadEtcTextureCompressorBehavior(Version version)
		{
			return version.IsGreaterEqual(2017, 2);
		}
		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		public static bool IsReadProjectGenerationIncludedExtensions(Version version)
		{
			return version.IsGreaterEqual(5, 1);
		}
		/// <summary>
		/// 5.5.0 to 2018.3 exclusive
		/// </summary>
		public static bool IsReadUserGeneratedProjectSuffix(Version version)
		{
			return version.IsGreaterEqual(5, 5) && version.IsLess(2018, 3);
		}
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool IsReadCollabEditorSettings(Version version)
		{
			return version.IsGreaterEqual(2017, 1);
		}
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool IsReadEnableTextureStreamingInEditMode(Version version)
		{
			return version.IsGreaterEqual(2019);
		}
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool IsReadEnableTextureStreamingInPlayMode(Version version)
		{
			return version.IsGreaterEqual(2018, 2);
		}
		/// <summary>
		/// 2019.1.0b6 and greater
		/// </summary>
		public static bool IsReadAsyncShaderCompilation(Version version)
		{
			return version.IsGreaterEqual(2019, 1, 0, VersionType.Beta, 6);
		}
		/// <summary>
		/// 2019.2 and greater
		/// </summary>
		public static bool IsReadShowLightmapResolutionOverlay(Version version)
		{
			return version.IsGreaterEqual(2019, 2);
		}
		
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		private static bool IsAlign1(Version version)
		{
			return version.IsGreaterEqual(2018, 2);
		}
		/// <summary>
		/// 2019.2 and greater
		/// </summary>
		private static bool IsAlign2(Version version)
		{
			return version.IsGreaterEqual(2019, 2);
		}

		private static int GetSerializedVersion(Version version)
		{
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
			// ExternalVersionControlSupport enum converted to string
			if (version.IsGreaterEqual(4))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if(IsReadExternalVersionControl(reader.Version))
			{
				ExternalVersionControl support = (ExternalVersionControl)reader.ReadInt32();
				switch(support)
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
			SerializationMode = (SerializationMode)reader.ReadInt32();
			if(IsReadWebSecurityEmulationEnabled(reader.Version))
			{
				WebSecurityEmulationEnabled = reader.ReadInt32();
				WebSecurityEmulationHostUrl = reader.ReadString();
			}
			reader.AlignStream(AlignType.Align4);

			if (IsReadLineEndingsForNewScripts(reader.Version))
			{
				LineEndingsForNewScripts = (LineEndingsMode)reader.ReadInt32();
			}
			if (IsReadDefaultBehaviorMode(reader.Version))
			{
				DefaultBehaviorMode = (EditorBehaviorMode)reader.ReadInt32();
			}
			if (IsReadPrefabRegularEnvironment(reader.Version))
			{
				PrefabRegularEnvironment.Read(reader);
				PrefabUIEnvironment.Read(reader);
			}
			if (IsReadSpritePackerMode(reader.Version))
			{
				SpritePackerMode = (SpritePackerMode)reader.ReadInt32();
			}

			if (IsReadSpritePackerPaddingPower(reader.Version))
			{
				SpritePackerPaddingPower = reader.ReadInt32();
			}
			if (IsReadEtcTextureCompressorBehavior(reader.Version))
			{
				EtcTextureCompressorBehavior = reader.ReadInt32();
				EtcTextureFastCompressor = reader.ReadInt32();
				EtcTextureNormalCompressor = reader.ReadInt32();
				EtcTextureBestCompressor = reader.ReadInt32();
			}
			if (IsReadProjectGenerationIncludedExtensions(reader.Version))
			{
				ProjectGenerationIncludedExtensions = reader.ReadString();
				ProjectGenerationRootNamespace = reader.ReadString();
			}
			if (IsReadUserGeneratedProjectSuffix(reader.Version))
			{
				UserGeneratedProjectSuffix = reader.ReadString();
			}
			if (IsReadCollabEditorSettings(reader.Version))
			{
				CollabEditorSettings.Read(reader);
			}
			if (IsReadEnableTextureStreamingInEditMode(reader.Version))
			{
				EnableTextureStreamingInEditMode = reader.ReadBoolean();
			}
			if (IsReadEnableTextureStreamingInPlayMode(reader.Version))
			{
				EnableTextureStreamingInPlayMode = reader.ReadBoolean();
			}
			if (IsReadAsyncShaderCompilation(reader.Version))
			{
				AsyncShaderCompilation = reader.ReadBoolean();
			}
			if (IsAlign1(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadShowLightmapResolutionOverlay(reader.Version))
			{
				ShowLightmapResolutionOverlay = reader.ReadBoolean();
			}
			if (IsAlign2(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(ExternalVersionControlSupportName, ExternalVersionControlSupport);
			node.Add(SerializationModeName, (int)SerializationMode);
			node.Add(LineEndingsForNewScriptsName, (int)LineEndingsForNewScripts);
			node.Add(DefaultBehaviorModeName, (int)DefaultBehaviorMode);
			if (IsReadPrefabRegularEnvironment(container.ExportVersion))
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
			if (IsReadUserGeneratedProjectSuffix(container.ExportVersion))
			{
				node.Add(UserGeneratedProjectSuffixName, GetUserGeneratedProjectSuffix(container.Version));
			}
			node.Add(CollabEditorSettingsName, GetCollabEditorSettings(container.Version).ExportYAML(container));
			if (IsReadEnableTextureStreamingInEditMode(container.ExportVersion))
			{
				node.Add(EnableTextureStreamingInEditModeName, GetEnableTextureStreamingInEditMode(container.Version));
			}
			if (IsReadEnableTextureStreamingInPlayMode(container.ExportVersion))
			{
				node.Add(EnableTextureStreamingInPlayModeName, GetEnableTextureStreamingInPlayMode(container.Version));
			}
			if (IsReadAsyncShaderCompilation(container.ExportVersion))
			{
				node.Add(AsyncShaderCompilationName, GetAsyncShaderCompilation(container.Version));
			}
			if (IsReadShowLightmapResolutionOverlay(container.ExportVersion))
			{
				node.Add(ShowLightmapResolutionOverlayName, GetShowLightmapResolutionOverlay(container.Version));
			}
			return node;
		}

		private int GetSpritePackerPaddingPower(Version version)
		{
			return IsReadSpritePackerPaddingPower(version) ? SpritePackerPaddingPower : 1;
		}
		private int GetEtcTextureCompressorBehavior(Version version)
		{
			return IsReadEtcTextureCompressorBehavior(version) ? EtcTextureCompressorBehavior : 1;
		}
		private int GetEtcTextureFastCompressor(Version version)
		{
			return IsReadEtcTextureCompressorBehavior(version) ? EtcTextureFastCompressor : 1;
		}
		private int GetEtcTextureNormalCompressor(Version version)
		{
			return IsReadEtcTextureCompressorBehavior(version) ? EtcTextureNormalCompressor : 2;
		}
		private int GetEtcTextureBestCompressor(Version version)
		{
			return IsReadEtcTextureCompressorBehavior(version) ? EtcTextureBestCompressor : 4;
		}
		private string GetProjectGenerationIncludedExtensions(Version version)
		{
			string exts = IsReadProjectGenerationIncludedExtensions(version) ? ProjectGenerationIncludedExtensions : DefaultExtensions;
			return GetSerializedVersion(version) < 8 ? exts : $"{exts};{AsmrefExtension}";
		}
		private string GetProjectGenerationRootNamespace(Version version)
		{
			return IsReadProjectGenerationIncludedExtensions(version) ? ProjectGenerationRootNamespace : string.Empty;
		}
		private string GetUserGeneratedProjectSuffix(Version version)
		{
			return IsReadUserGeneratedProjectSuffix(version) ? UserGeneratedProjectSuffix : string.Empty;
		}
		private CollabEditorSettings GetCollabEditorSettings(Version version)
		{
			return IsReadCollabEditorSettings(version) ? CollabEditorSettings : new CollabEditorSettings(true);
		}
		private bool GetEnableTextureStreamingInEditMode(Version version)
		{
			return IsReadEnableTextureStreamingInEditMode(version) ? EnableTextureStreamingInEditMode : true;
		}
		private bool GetEnableTextureStreamingInPlayMode(Version version)
		{
			return IsReadEnableTextureStreamingInPlayMode(version) ? EnableTextureStreamingInPlayMode : true;
		}
		private bool GetAsyncShaderCompilation(Version version)
		{
			return IsReadAsyncShaderCompilation(version) ? AsyncShaderCompilation : true;
		}
		private bool GetShowLightmapResolutionOverlay(Version version)
		{
			return IsReadShowLightmapResolutionOverlay(version) ? ShowLightmapResolutionOverlay : true;
		}

		public string ExternalVersionControlSupport { get; private set; }
		public SerializationMode SerializationMode { get; private set; }
		public int WebSecurityEmulationEnabled { get; private set; }
		public string WebSecurityEmulationHostUrl { get; private set; }
		public LineEndingsMode LineEndingsForNewScripts { get; private set; }
		public EditorBehaviorMode DefaultBehaviorMode { get; private set; }
		public SpritePackerMode SpritePackerMode { get; private set; }
		public int SpritePackerPaddingPower { get; private set; }
		public int EtcTextureCompressorBehavior { get; private set; }
		public int EtcTextureFastCompressor { get; private set; }
		public int EtcTextureNormalCompressor { get; private set; }
		public int EtcTextureBestCompressor { get; private set; }
		public string ProjectGenerationIncludedExtensions { get; private set; }
		public string ProjectGenerationRootNamespace { get; private set; }
		public string UserGeneratedProjectSuffix { get; private set; }
		public bool EnableTextureStreamingInEditMode { get; private set; }
		public bool EnableTextureStreamingInPlayMode { get; private set; }
		public bool AsyncShaderCompilation { get; private set; }
		public bool ShowLightmapResolutionOverlay { get; private set; }

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
		public const string ShowLightmapResolutionOverlayName = "m_ShowLightmapResolutionOverlay";

		public PPtr<SceneAsset> PrefabRegularEnvironment;
		public PPtr<SceneAsset> PrefabUIEnvironment;
		public CollabEditorSettings CollabEditorSettings;

		private const string DefaultExtensions = "txt;xml;fnt;cd;asmdef;rsp";
		private const string AsmrefExtension = "asmref";
		private const string HiddenMeta = "Hidden Meta Files";
		private const string VisibleMeta = "Visible Meta Files";
	}
}
