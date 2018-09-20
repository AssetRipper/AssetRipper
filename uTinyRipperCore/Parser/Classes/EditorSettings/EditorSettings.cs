using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.EditorSettingss;
using uTinyRipper.Exporter.YAML;
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
			EnableTextureStreamingInPlayMode = true;
		}

		public static EditorSettings CreateVirtualInstance(VirtualSerializedFile virtualFile)
		{
			return virtualFile.CreateAsset((assetInfo) => new EditorSettings(assetInfo, true));
		}

		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		private static bool IsReadExternalVersionControl(Version version)
		{
			return version.IsGreaterEqual(4);
		}
		/// <summary>
		/// 5.4.x and less
		/// </summary>
		private static bool IsReadWebSecurityEmulationEnabled(Version version)
		{
			return version.IsLessEqual(5, 4);
		}
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		private static bool IsReadLineEndingsForNewScripts(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		private static bool IsReadDefaultBehaviorMode(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		private static bool IsReadSpritePackerPaddingPower(Version version)
		{
			return version.IsGreaterEqual(5, 1);
		}
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		private static bool IsReadEtcTextureCompressorBehavior(Version version)
		{
			return version.IsGreaterEqual(2017, 2);
		}
		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		private static bool IsReadProjectGenerationIncludedExtensions(Version version)
		{
			return version.IsGreaterEqual(5, 1);
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		private static bool IsReadUserGeneratedProjectSuffix(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool IsReadCollabEditorSettings(Version version)
		{
			return version.IsGreaterEqual(2017, 1);
		}
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		private static bool IsReadEnableTextureStreamingInPlayMode(Version version)
		{
			return version.IsGreaterEqual(2018, 2);
		}
		
		private static int GetSerializedVersion(Version version)
		{
			// LineEndingsForNewScripts default value changed (Unix to OSNative)
			if (Config.IsExportTopmostSerializedVersion || version.IsGreaterEqual(2017, 3))
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
			if (IsReadEnableTextureStreamingInPlayMode(reader.Version))
			{
				EnableTextureStreamingInPlayMode = reader.ReadBoolean();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_ExternalVersionControlSupport", ExternalVersionControlSupport);
			node.Add("m_SerializationMode", (int)SerializationMode);
			node.Add("m_LineEndingsForNewScripts", (int)LineEndingsForNewScripts);
			node.Add("m_DefaultBehaviorMode", (int)DefaultBehaviorMode);
			node.Add("m_SpritePackerMode", (int)SpritePackerMode);
			node.Add("m_SpritePackerPaddingPower", GetSpritePackerPaddingPower(container.Version));
			node.Add("m_EtcTextureCompressorBehavior", GetEtcTextureCompressorBehavior(container.Version));
			node.Add("m_EtcTextureFastCompressor", GetEtcTextureFastCompressor(container.Version));
			node.Add("m_EtcTextureNormalCompressor", GetEtcTextureNormalCompressor(container.Version));
			node.Add("m_EtcTextureBestCompressor", GetEtcTextureBestCompressor(container.Version));
			node.Add("m_ProjectGenerationIncludedExtensions", GetProjectGenerationIncludedExtensions(container.Version));
			node.Add("m_ProjectGenerationRootNamespace", GetProjectGenerationRootNamespace(container.Version));
			node.Add("m_UserGeneratedProjectSuffix", GetUserGeneratedProjectSuffix(container.Version));
			node.Add("m_CollabEditorSettings", GetCollabEditorSettings(container.Version).ExportYAML(container));
#warning TODO: 2018
			//node.Add("m_EnableTextureStreamingInPlayMode", GetEnableTextureStreamingInPlayMode(container.Version));
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
			return IsReadProjectGenerationIncludedExtensions(version) ? ProjectGenerationIncludedExtensions : DefaultExtensions;
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
		private bool GetEnableTextureStreamingInPlayMode(Version version)
		{
			return IsReadEnableTextureStreamingInPlayMode(version) ? EnableTextureStreamingInPlayMode : true;
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
		public bool EnableTextureStreamingInPlayMode { get; private set; }

		public CollabEditorSettings CollabEditorSettings;

		private const string DefaultExtensions = "txt;xml;fnt;cd;asmdef;rsp";
		private const string HiddenMeta = "Hidden Meta Files";
		private const string VisibleMeta = "Visible Meta Files";
	}
}
