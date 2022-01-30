using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.LightmapSettings
{
	public sealed class LightmapSettings : LevelGameManager
	{
		public LightmapSettings(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			// NOTE: unknown version
			// ShadowMaskMode has been replaced by UseShadowmask?
			if (version.IsGreaterEqual(2017, 0, 0, UnityVersionType.Beta))
			{
				return 11;
			}
			// NOTE: unknown conversion
			if (version.IsGreaterEqual(2017))
			{
				return 10;
			}
			// NOTE: unknown conversion
			if (version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 2))
			{
				return 9;
			}
			// NOTE: unknown conversion
			if (version.IsGreaterEqual(5, 4))
			{
				return 7;
			}
			// LightmapSnapshot has been renamed to LightingDataAsset
			if (version.IsGreaterEqual(5, 3))
			{
				return 6;
			}
			// NOTE: unknown version
			// NOTE: unknown conversion
			if (version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final))
			{
				return 5;
			}

			// NOTE: unknown version
			// NOTE: unknown conversion
			// return 4;

			// NOTE: unknown version
			// NOTE: unknown conversion
			if (version.IsGreaterEqual(5, 0, 0, UnityVersionType.Beta))
			{
				return 3;
			}
			// NOTE: unknown conversion
			if (version.IsGreaterEqual(5))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 5.0.0bx (NOTE: unknown version)
		/// </summary>
		public static bool HasLightProbesLegacy(UnityVersion version) => version.IsEqual(5, 0, 0, UnityVersionType.Beta);
		/// <summary>
		/// 5.0.0f1 and greater and Release
		/// </summary>
		public static bool HasEnlightenSceneMapping(UnityVersion version, TransferInstructionFlags flags)
		{
			// NOTE: unknown version
			return version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final) && flags.IsRelease();
		}
		/// <summary>
		/// 5.0.0bx or (5.0.0 and greater and Not Release)
		/// </summary>
		public static bool HasGIWorkflowMode(UnityVersion version, TransferInstructionFlags flags)
		{
			if (version.IsGreaterEqual(5))
			{
				if (flags.IsRelease())
				{
					// NOTE: unknown version
					return version.IsEqual(5, 0, 0, UnityVersionType.Beta);
				}
				return true;
			}
			return false;
		}
		/// <summary>
		/// (3.5.0 to 5.0.0f1 exclusive) or (3.5.0 and greater and Release or Resource)
		/// </summary>
		public static bool HasLightProbes(UnityVersion version, TransferInstructionFlags flags)
		{
			if (version.IsGreaterEqual(3, 5))
			{
				if (flags.IsRelease() || flags.IsBuiltinResources())
				{
					return true;
				}
				// NOTE: unknown version
				if (version.IsLess(5, 0, 0, UnityVersionType.Final))
				{
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// Less than 5.0.0f1 or Release or Resource
		/// </summary>
		public static bool HasLightmaps(UnityVersion version, TransferInstructionFlags flags)
		{
			if (flags.IsRelease() || flags.IsBuiltinResources())
			{
				return true;
			}
			// NOTE: unknown version
			if (version.IsLess(5, 0, 0, UnityVersionType.Final))
			{
				return true;
			}
			return false;
		}
		/// <summary>
		/// 5.0.0bx (NOTE: unknown version)
		/// </summary>
		public static bool HasLightmapsModeLegacy(UnityVersion version) => version.IsEqual(5, 0, 0, UnityVersionType.Beta);
		/// <summary>
		/// (3.0.0 to 5.4.0 exclusive) or (5.4.0 and greater and Release)
		/// </summary>
		public static bool HasLightmapsMode(UnityVersion version, TransferInstructionFlags flags)
		{
			if (version.IsGreaterEqual(5, 4))
			{
				return flags.IsRelease();
			}
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 3.2.0 to 5.0.0f1 exclusive
		/// </summary>
		public static bool HasUseDualLightmapsInForward(UnityVersion version) => version.IsGreaterEqual(3, 2) && version.IsLess(5, 0, 0, UnityVersionType.Final);
		/// <summary>
		/// 3.5.0 to 5.0.0f1 exclusive
		/// </summary>
		public static bool HasBakedColorSpace(UnityVersion version) => version.IsGreaterEqual(3, 5) && version.IsLess(5, 0, 0, UnityVersionType.Final);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasGISettings(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 3.0.0 and greater and Not Release
		/// </summary>
		public static bool HasLightmapEditorSettings(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(3);
		/// <summary>
		/// 5.0.0 and greater and Not Release
		/// </summary>
		public static bool HasLightingDataAsset(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(5);
		/// <summary>
		/// 5.0.0 to 5.6.0b6
		/// </summary>
		public static bool HasRuntimeCPUUsage(UnityVersion version) => version.IsGreaterEqual(5) && version.IsLessEqual(5, 6, 0, UnityVersionType.Beta, 6);
		/// <summary>
		/// 5.6.0b2 and greater but less than 2020
		/// </summary>
		public static bool HasUseShadowmask(UnityVersion version) => version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 2) && version.IsLess(2020);
		/// <summary>
		/// 2020 and greater
		/// </summary>
		public static bool HasLightingSettings(UnityVersion version) => version.IsGreaterEqual(2020);

		/// <summary>
		/// 2017.1 and (Release or Resource)
		/// </summary>
		private static bool IsAlign1(UnityVersion version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(2017) && (flags.IsRelease() || flags.IsBuiltinResources());
		}
		/// <summary>
		/// 3.2.0 and greater
		/// </summary>
		private static bool IsAlign2(UnityVersion version) => version.IsGreaterEqual(3, 2);

		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool IsBoolShadowmask(UnityVersion version) => version.IsGreaterEqual(2017);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasLightProbesLegacy(reader.Version))
			{
				LightProbesLegacy.Read(reader);
			}
			if (HasEnlightenSceneMapping(reader.Version, reader.Flags))
			{
				EnlightenSceneMapping.Read(reader);
			}
			if (HasGIWorkflowMode(reader.Version, reader.Flags))
			{
				GIWorkflowMode = (GIWorkflowMode)reader.ReadInt32();
			}
			if (HasLightProbes(reader.Version, reader.Flags))
			{
				LightProbes.Read(reader);
			}
			if (HasLightmaps(reader.Version, reader.Flags))
			{
				Lightmaps = reader.ReadAssetArray<LightmapData>();
			}
			if (IsAlign1(reader.Version, reader.Flags))
			{
				reader.AlignStream();
			}

			if (HasLightmapsModeLegacy(reader.Version))
			{
				LightmapsModeLegacy = (LightmapsMode)reader.ReadInt32();
			}
			if (HasLightmapsMode(reader.Version, reader.Flags))
			{
				LightmapsMode = (LightmapsMode)reader.ReadInt32();
			}
			if (HasBakedColorSpace(reader.Version))
			{
				BakedColorSpace = (ColorSpace)reader.ReadInt32();
			}
			if (HasUseDualLightmapsInForward(reader.Version))
			{
				UseDualLightmapsInForward = reader.ReadBoolean();
			}
			if (IsAlign2(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasGISettings(reader.Version))
			{
				GISettings.Read(reader);
			}

			if (HasRuntimeCPUUsage(reader.Version))
			{
				RuntimeCPUUsage = reader.ReadInt32();
			}
			if (HasUseShadowmask(reader.Version))
			{
				if (IsBoolShadowmask(reader.Version))
				{
					UseShadowmask = reader.ReadBoolean();
				}
				else
				{
					ShadowMaskMode = reader.ReadInt32();
				}
			}

			if (HasLightingSettings(reader.Version))
			{
				LightingSettings.Read(reader);
			}
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			if (HasLightProbesLegacy(writer.Version))
			{
				LightProbesLegacy.Write(writer);
			}
			if (HasEnlightenSceneMapping(writer.Version, writer.Flags))
			{
				EnlightenSceneMapping.Write(writer);
			}
			if (HasGIWorkflowMode(writer.Version, writer.Flags))
			{
				writer.Write((int)GIWorkflowMode);
			}
			if (HasLightProbes(writer.Version, writer.Flags))
			{
				LightProbes.Write(writer);
			}
			if (HasLightmaps(writer.Version, writer.Flags))
			{
				Lightmaps.Write(writer);
			}
			if (IsAlign1(writer.Version, writer.Flags))
			{
				writer.AlignStream();
			}

			if (HasLightmapsModeLegacy(writer.Version))
			{
				writer.Write((int)LightmapsModeLegacy);
			}
			if (HasLightmapsMode(writer.Version, writer.Flags))
			{
				writer.Write((int)LightmapsMode);
			}
			if (HasBakedColorSpace(writer.Version))
			{
				writer.Write((int)BakedColorSpace);
			}
			if (HasUseDualLightmapsInForward(writer.Version))
			{
				writer.Write(UseDualLightmapsInForward);
			}
			if (IsAlign2(writer.Version))
			{
				writer.AlignStream();
			}

			if (HasGISettings(writer.Version))
			{
				GISettings.Write(writer);
			}

			if (HasRuntimeCPUUsage(writer.Version))
			{
				writer.Write(RuntimeCPUUsage);
			}
			if (HasUseShadowmask(writer.Version))
			{
				if (IsBoolShadowmask(writer.Version))
				{
					writer.Write(UseShadowmask);
				}
				else
				{
					writer.Write(ShadowMaskMode);
				}
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			if (HasLightProbesLegacy(context.Version))
			{
				yield return context.FetchDependency(LightProbesLegacy, LightProbesLegacyName);
			}
			if (HasEnlightenSceneMapping(context.Version, context.Flags))
			{
				foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromDependent(EnlightenSceneMapping, EnlightenSceneMappingName))
				{
					yield return asset;
				}
			}
			if (HasLightProbes(context.Version, context.Flags))
			{
				yield return context.FetchDependency(LightProbes, LightProbesName);
				foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromArray(Lightmaps, LightmapsName))
				{
					yield return asset;
				}
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));


			if (HasLightProbesLegacy(container.ExportVersion))
			{
				node.Add(LightProbesLegacyName, LightProbesLegacy.ExportYAML(container));
			}
			if (HasGIWorkflowMode(container.ExportVersion, container.ExportFlags))
			{
				node.Add(GIWorkflowModeName, (int)GetExportGIWorkflowMode(container.Version, container.Flags));
			}
			if (HasLightProbes(container.ExportVersion, container.ExportFlags))
			{
				node.Add(LightProbesName, LightProbes.ExportYAML(container));
			}
			if (HasLightmaps(container.ExportVersion, container.ExportFlags))
			{
				node.Add(LightmapsName, Lightmaps.ExportYAML(container));
			}

			if (HasLightmapsModeLegacy(container.ExportVersion))
			{
				node.Add(LightmapsModeLegacyName, (int)LightmapsModeLegacy);
			}
			if (HasLightmapsMode(container.ExportVersion, container.ExportFlags))
			{
				node.Add(LightmapsModeName, (int)LightmapsMode);
			}
			if (HasBakedColorSpace(container.ExportVersion))
			{
				node.Add(BakedColorSpaceName, (int)BakedColorSpace);
			}
			if (HasUseDualLightmapsInForward(container.ExportVersion))
			{
				node.Add(UseDualLightmapsInForwardName, UseDualLightmapsInForward);
			}

			if (HasGISettings(container.ExportVersion))
			{
				node.Add(GISettingsName, GetExportGISettings(container.Version).ExportYAML(container));
			}

			if (HasRuntimeCPUUsage(container.ExportVersion))
			{
				node.Add(RuntimeCPUUsageName, RuntimeCPUUsage);
			}
			if (HasUseShadowmask(container.ExportVersion))
			{
				if (IsBoolShadowmask(container.ExportVersion))
				{
					node.Add(UseShadowmaskName, GetExportUseShadowmask(container.Version));
				}
				else
				{
					node.Add(ShadowMaskModeName, ShadowMaskMode);
				}
			}
			return node;
		}

		private GIWorkflowMode GetExportGIWorkflowMode(UnityVersion version, TransferInstructionFlags flags)
		{
			return GIWorkflowMode.OnDemand;
		}
		private GISettings.GISettings GetExportGISettings(UnityVersion version)
		{
			return HasGISettings(version) ? GISettings : new GISettings.GISettings(true);
		}
		private LightmapEditorSettings GetExportLightmapEditorSettings(IExportContainer container)
		{
			return new LightmapEditorSettings(container.ExportVersion);
		}
		private PPtr<LightingDataAsset.LightingDataAsset> GetLightingDataAsset(UnityVersion version, TransferInstructionFlags flags)
		{
			return new();
		}
		private bool GetExportUseShadowmask(UnityVersion version)
		{
			return HasUseShadowmask(version) ? UseShadowmask : true;
		}

		public GIWorkflowMode GIWorkflowMode { get; set; }
		public LightmapData[] Lightmaps { get; set; }
		public LightmapsMode LightmapsModeLegacy { get; set; }
		public LightmapsMode LightmapsMode { get; set; }
		public ColorSpace BakedColorSpace { get; set; }
		public bool UseDualLightmapsInForward { get; set; }
		public int RuntimeCPUUsage { get; set; }
		public PPtr<LightingSettings> LightingSettings { get; set; } = new();
		public bool UseShadowmask
		{
			get => ShadowMaskMode != 0;
			set => ShadowMaskMode = value ? 1 : 0;
		}

		/// <summary>
		/// 2017.1 - replaced by UseShadowmask
		/// </summary>
		public int ShadowMaskMode { get; set; }

		public const string EnlightenSceneMappingName = "m_EnlightenSceneMapping";
		public const string LightProbesLegacyName = "m_LightProbesLegacy";
		public const string GIWorkflowModeName = "m_GIWorkflowMode";
		public const string LightmapsModeLegacyName = "m_LightmapsModeLegacy";
		public const string LightmapsModeName = "m_LightmapsMode";
		public const string BakedColorSpaceName = "m_BakedColorSpace";
		public const string UseDualLightmapsInForwardName = "m_UseDualLightmapsInForward";
		public const string GISettingsName = "m_GISettings";
		public const string LightmapEditorSettingsName = "m_LightmapEditorSettings";
		public const string LightingDataAssetName = "m_LightingDataAsset";
		public const string RuntimeCPUUsageName = "m_RuntimeCPUUsage";
		public const string LightmapSnapshotName = "m_LightmapSnapshot";
		public const string LightProbesName = "m_LightProbes";
		public const string LightmapsName = "m_Lightmaps";
		public const string UseShadowmaskName = "m_UseShadowmask";
		public const string ShadowMaskModeName = "m_ShadowMaskMode";

#warning TODO: PPtr<LightProbesLegacy>
		public PPtr<Object.Object> LightProbesLegacy = new();
		public EnlightenSceneMapping EnlightenSceneMapping = new();
		public PPtr<LightProbes.LightProbes> LightProbes = new();
		public GISettings.GISettings GISettings = new();
	}
}
