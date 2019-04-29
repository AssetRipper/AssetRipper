using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.LightmapSettingss;
using uTinyRipper.Classes.Textures;
using uTinyRipper.SerializedFiles;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class LightmapSettings : LevelGameManager
	{
		public LightmapSettings(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// (5.0.0 and greater) and Release
		/// </summary>
		public static bool IsReadEnlightenSceneMapping(Version version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(5) && flags.IsRelease();
		}
		/// <summary>
		/// 5.0.0 and greater and Not Release
		/// </summary>
		public static bool IsReadGIWorkflowMode(Version version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(5) && !flags.IsRelease();
		}
		/// <summary>
		/// 3.5.0 and greater and (Release or Resource)
		/// </summary>
		public static bool IsReadLightProbes(Version version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(3, 5) && (flags.IsRelease() || flags.IsBuiltinResources());
		}
		/// <summary>
		/// Release or Resource
		/// </summary>
		public static bool IsReadLightmaps(TransferInstructionFlags flags)
		{
			return flags.IsRelease() || flags.IsBuiltinResources();
		}
		/// <summary>
		/// 3.0.0 and greater and Release
		/// </summary>
		public static bool IsReadLightmapsMode(Version version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(3) && flags.IsRelease();
		}
		/// <summary>
		/// 3.2.0 to 5.0.0 exclusive
		/// </summary>
		public static bool IsReadUseDualLightmapsInForward(Version version)
		{
			return version.IsGreaterEqual(3, 2) && version.IsLess(5);
		}
		/// <summary>
		/// 3.5.0 to 5.0.0 exclusive
		/// </summary>
		public static bool IsReadBakedColorSpace(Version version)
		{
			return version.IsGreaterEqual(3, 5) && version.IsLess(5);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadGISettings(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// Not Release
		/// </summary>
		public static bool IsReadLightmapEditorSettings(Version version, TransferInstructionFlags flags)
		{
#warning unknown version (random)
			return version.IsGreaterEqual(2017) && !flags.IsRelease();
		}
		/// <summary>
		/// 5.0.0 and greater and Not Release
		/// </summary>
		public static bool IsReadLightingDataAsset(Version version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(5) && !flags.IsRelease();
		}
		/// <summary>
		/// 5.0.0 to 5.6.0b6
		/// </summary>
		public static bool IsReadRuntimeCPUUsage(Version version)
		{
			return version.IsGreaterEqual(5) && version.IsLessEqual(5, 6, 0, VersionType.Beta, 6);
		}
		/// <summary>
		/// 5.6.0b2 and greater
		/// </summary>
		public static bool IsReadUseShadowmask(Version version)
		{
			return version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 2);
		}

		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool IsBoolShadowmask(Version version)
		{
			return version.IsGreaterEqual(2017);
		}

		/// <summary>
		/// 2017.1 and (Release or Resource)
		/// </summary>
		private static bool IsAlign1(Version version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(2017) && (flags.IsRelease() || flags.IsBuiltinResources());
		}
		/// <summary>
		/// 3.2.0 and greater
		/// </summary>
		private static bool IsAlign2(Version version)
		{
			return version.IsGreaterEqual(3, 2);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(2017))
			{
				return 11;
			}
			// unknown (alpha) version
			//return 10;
			if (version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 2))
			{
				return 9;
			}
			// unknown (5.6.0b1-2) version
			//return 8;
			if (version.IsGreaterEqual(5, 4))
			{
				return 7;
			}
			if (version.IsGreaterEqual(5, 3))
			{
				return 6;
			}
			// unknown version
			if (version.IsGreaterEqual(5, 0, 0, VersionType.Final))
			{
				return 5;
			}
			// unknown (beta) version
			// return 4;
			if (version.IsGreaterEqual(5))
			{
				return 3;
			}
			// unknown (alpha) version
			// return 2;

			return 1;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadEnlightenSceneMapping(reader.Version, reader.Flags))
			{
				EnlightenSceneMapping.Read(reader);
			}
#if UNIVERSAL
			if (IsReadGIWorkflowMode(reader.Version, reader.Flags))
			{
				GIWorkflowMode = (GIWorkflowMode)reader.ReadInt32();
			}
#endif

			if (IsReadLightProbes(reader.Version, reader.Flags))
			{
				LightProbes.Read(reader);
			}
			if (IsReadLightmaps(reader.Flags))
			{
				m_lightmaps = reader.ReadAssetArray<LightmapData>();
			}
			if (IsAlign1(reader.Version, reader.Flags))
			{
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadLightmapsMode(reader.Version, reader.Flags))
			{
				LightmapsMode = (LightmapsMode)reader.ReadInt32();
			}
			if (IsReadBakedColorSpace(reader.Version))
			{
				BakedColorSpace = (ColorSpace)reader.ReadInt32();
			}
			if (IsReadUseDualLightmapsInForward(reader.Version))
			{
				UseDualLightmapsInForward = reader.ReadBoolean();
			}
			if (IsAlign2(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadGISettings(reader.Version))
			{
				GISettings.Read(reader);
			}

#if UNIVERSAL
			if (IsReadLightmapEditorSettings(reader.Version, reader.Flags))
			{
				LightmapEditorSettings.Read(reader);
			}
			if (IsReadLightingDataAsset(reader.Version, reader.Flags))
			{
				LightingDataAsset.Read(reader);
			}
#endif
			if (IsReadRuntimeCPUUsage(reader.Version))
			{
				RuntimeCPUUsage = reader.ReadInt32();
			}
			if (IsReadUseShadowmask(reader.Version))
			{
				if (IsBoolShadowmask(reader.Version))
				{
					UseShadowmask = reader.ReadBoolean();
				}
				else
				{
					UseShadowmask = reader.ReadInt32() == 0 ? false : true;
				}
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			if (IsReadEnlightenSceneMapping(file.Version, file.Flags))
			{
				foreach (Object asset in EnlightenSceneMapping.FetchDependencies(file, isLog))
				{
					yield return asset;
				}
			}
			if (IsReadLightProbes(file.Version, file.Flags))
			{
				yield return LightProbes.FetchDependency(file, isLog, ToLogString, "m_LightProbes");
				foreach (LightmapData lightmap in Lightmaps)
				{
					foreach (Object asset in lightmap.FetchDependencies(file, isLog))
					{
						yield return asset;
					}
				}
			}
#if UNIVERSAL
			if (IsReadLightmapEditorSettings(file.Version, file.Flags))
			{
				foreach (Object asset in LightmapEditorSettings.FetchDependencies(file, isLog))
				{
					yield return asset;
				}
			}
			if (IsReadLightingDataAsset(file.Version, file.Flags))
			{
				yield return LightingDataAsset.FetchDependency(file, isLog, ToLogString, LightingDataAssetName);
			}
#endif
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(GIWorkflowModeName, (int)GetExportGIWorkflowMode(container.Version, container.Flags));
			node.Add(GISettingsName, GetExportGISettings(container.Version).ExportYAML(container));
			node.Add(LightmapEditorSettingsName, GetExportLightmapEditorSettings(container.Version, container.Flags).ExportYAML(container));
#warning is it possible to somehow create LightingDataAsset with Release data?
			node.Add(LightingDataAssetName, GetLightingDataAsset(container.Version, container.Flags).ExportYAML(container));
			node.Add(UseShadowmaskName, GetExportUseShadowmask(container.Version));
			return node;
		}

		private GIWorkflowMode GetExportGIWorkflowMode(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadGIWorkflowMode(version, flags))
			{
				return GIWorkflowMode;
			}
#endif
			return GIWorkflowMode.OnDemand;
		}
		private GISettings GetExportGISettings(Version version)
		{
			return IsReadGISettings(version) ? GISettings : new GISettings(true);
		}
		private LightmapEditorSettings GetExportLightmapEditorSettings(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadLightmapEditorSettings(version, flags))
			{
				return LightmapEditorSettings;
			}
#endif
			return new LightmapEditorSettings(true);
		}
		private PPtr<LightingDataAsset> GetLightingDataAsset(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadLightingDataAsset(version, flags))
			{
				return LightingDataAsset;
			}
#endif
			return default;
		}
		private bool GetExportUseShadowmask(Version version)
		{
			return IsReadUseShadowmask(version) ? UseShadowmask : true;
		}

#if UNIVERSAL
		public GIWorkflowMode GIWorkflowMode { get; private set; }
#endif
		public IReadOnlyList<LightmapData> Lightmaps => m_lightmaps;
		public LightmapsMode LightmapsMode { get; private set; }
		public ColorSpace BakedColorSpace { get; private set; }
		public bool UseDualLightmapsInForward { get; private set; }
		public int RuntimeCPUUsage { get; private set; }
		/// <summary>
		/// ShadowMaskMode previously
		/// </summary>
		public bool UseShadowmask { get; private set; }

		public const string GIWorkflowModeName = "m_GIWorkflowMode";
		public const string GISettingsName = "m_GISettings";
		public const string LightmapEditorSettingsName = "m_LightmapEditorSettings";
		public const string LightingDataAssetName = "m_LightingDataAsset";
		public const string UseShadowmaskName = "m_UseShadowmask";

		public EnlightenSceneMapping EnlightenSceneMapping;
		public PPtr<LightProbes> LightProbes;
		public GISettings GISettings;
#if UNIVERSAL
		public LightmapEditorSettings LightmapEditorSettings;
		/// <summary>
		/// LightmapSnapshot previously
		/// </summary>
		public PPtr<LightingDataAsset> LightingDataAsset;
#endif

		private LightmapData[] m_lightmaps;
	}
}
