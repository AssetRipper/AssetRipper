using System.Collections.Generic;
using uTinyRipper.Classes.LightingDataAssets;
using uTinyRipper.Classes.LightmapSettingss;
using uTinyRipper.Classes.Lights;
using uTinyRipper.Classes.OcclusionCullingDatas;
using uTinyRipper.Classes.RenderSettingss;
using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper;

namespace uTinyRipper.Classes
{
	/// <summary>
	/// 5.0.0 - first introduction as LightmapSnapshot
	/// 5.3.0 - renamed to LightingDataAsset
	/// </summary>
	public sealed class LightingDataAsset : NamedObject
	{
		public LightingDataAsset(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
		{
			if (version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 2))
			{
				return 4;
			}
			return 3;
			/*if (version.IsGreaterEqual())
			{
				return 3;
			}
			if (version.IsGreaterEqual())
			{
				return 2;
			}
			return 1;*/
		}

		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasAOTextures(Version version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasLightmapsCacheFiles(Version version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasBakedReflectionProbeCubemapCacheFiles(Version version) => version.IsGreaterEqual(2018, 2);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Scene.Read(reader);

			Lightmaps = reader.ReadAssetArray<LightmapData>();
			reader.AlignStream();

			if (HasAOTextures(reader.Version))
			{
				AOTextures = reader.ReadAssetArray<PPtr<Texture2D>>();
			}
			if (HasLightmapsCacheFiles(reader.Version))
			{
				LightmapsCacheFiles = reader.ReadStringArray();
			}

			LightProbes.Read(reader);
			LightmapsMode = reader.ReadInt32();
			BakedAmbientProbeInLinear.Read(reader);
			LightmappedRendererData = reader.ReadAssetArray<RendererData>();
			reader.AlignStream();

			LightmappedRendererDataIDs = reader.ReadAssetArray<SceneObjectIdentifier>();
			reader.AlignStream();

			EnlightenSceneMapping.Read(reader);
			EnlightenSceneMappingRendererIDs = reader.ReadAssetArray<SceneObjectIdentifier>();
			reader.AlignStream();

			Lights = reader.ReadAssetArray<SceneObjectIdentifier>();
			reader.AlignStream();

			LightBakingOutputs = reader.ReadAssetArray<LightBakingOutput>();
			reader.AlignStream();

			if (HasBakedReflectionProbeCubemapCacheFiles(reader.Version))
			{
				BakedReflectionProbeCubemapCacheFiles = reader.ReadStringArray();
			}
			BakedReflectionProbeCubemaps = reader.ReadAssetArray<PPtr<Texture>>();
			BakedReflectionProbes = reader.ReadAssetArray<SceneObjectIdentifier>();
			reader.AlignStream();

			EnlightenData = reader.ReadByteArray();
			reader.AlignStream();

			EnlightenDataVersion = reader.ReadInt32();
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(Scene, SceneName);
			foreach (PPtr<Object> asset in context.FetchDependencies(Lightmaps, LightmapsName))
			{
				yield return asset;
			}
			yield return context.FetchDependency(LightProbes, LightProbesName);
			foreach (PPtr<Object> asset in context.FetchDependencies(LightmappedRendererData, LightmappedRendererDataName))
			{
				yield return asset;
			}
			foreach (PPtr<Object> asset in context.FetchDependencies(EnlightenSceneMapping, EnlightenSceneMappingName))
			{
				yield return asset;
			}
			foreach (PPtr<Object> asset in context.FetchDependencies(BakedReflectionProbeCubemaps, BakedReflectionProbeCubemapsName))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(SceneName, Scene.ExportYAML(container));
			node.Add(LightmapsName, Lightmaps.ExportYAML(container));
			if (HasAOTextures(container.ExportVersion))
			{
				node.Add(AOTexturesName, GetAOTextures(container.Version).ExportYAML(container));
			}
			if (HasLightmapsCacheFiles(container.Version))
			{
				node.Add(LightmapsCacheFilesName, GetLightmapsCacheFiles(container.Version).ExportYAML());
			}
			node.Add(LightProbesName, LightProbes.ExportYAML(container));
			node.Add(LightmapsModeName, LightmapsMode);
			node.Add(BakedAmbientProbeInLinearName, BakedAmbientProbeInLinear.ExportYAML(container));
			node.Add(LightmappedRendererDataName, LightmappedRendererData.ExportYAML(container));
			node.Add(LightmappedRendererDataIDsName, LightmappedRendererDataIDs.ExportYAML(container));
			node.Add(EnlightenSceneMappingName, EnlightenSceneMapping.ExportYAML(container));
			node.Add(EnlightenSceneMappingRendererIDsName, EnlightenSceneMappingRendererIDs.ExportYAML(container));
			node.Add(LightsName, Lights.ExportYAML(container));
			node.Add(LightBakingOutputsName, LightBakingOutputs.ExportYAML(container));
			if (HasBakedReflectionProbeCubemapCacheFiles(container.ExportVersion))
			{
				node.Add(BakedReflectionProbeCubemapCacheFilesName, GetBakedReflectionProbeCubemapCacheFiles(container.Version).ExportYAML());
			}
			node.Add(BakedReflectionProbeCubemapsName, BakedReflectionProbeCubemaps.ExportYAML(container));
			node.Add(BakedReflectionProbesName, BakedReflectionProbes.ExportYAML(container));
			node.Add(EnlightenDataName, EnlightenData.ExportYAML());
			node.Add(EnlightenDataVersionName, EnlightenDataVersion);
			return node;
		}

		private IReadOnlyList<PPtr<Texture2D>> GetAOTextures(Version version)
		{
			return HasAOTextures(version) ? AOTextures : System.Array.Empty<PPtr<Texture2D>>();
		}
		private IReadOnlyList<string> GetLightmapsCacheFiles(Version version)
		{
			return HasLightmapsCacheFiles(version) ? LightmapsCacheFiles : System.Array.Empty<string>();
		}
		private IReadOnlyList<string> GetBakedReflectionProbeCubemapCacheFiles(Version version)
		{
			return HasBakedReflectionProbeCubemapCacheFiles(version) ? BakedReflectionProbeCubemapCacheFiles : System.Array.Empty<string>();
		}

		public LightmapData[] Lightmaps { get; set; }
		public PPtr<Texture2D>[] AOTextures { get; set; }
		public string[] LightmapsCacheFiles { get; set; }
		public int LightmapsMode { get; set; }
		public RendererData[] LightmappedRendererData { get; set; }
		public SceneObjectIdentifier[] LightmappedRendererDataIDs { get; set; }
		public SceneObjectIdentifier[] EnlightenSceneMappingRendererIDs { get; set; }
		public SceneObjectIdentifier[] Lights { get; set; }
		public LightBakingOutput[] LightBakingOutputs { get; set; }
		public string[] BakedReflectionProbeCubemapCacheFiles { get; set; }
		public PPtr<Texture>[] BakedReflectionProbeCubemaps { get; set; }
		public SceneObjectIdentifier[] BakedReflectionProbes { get; set; }
		public byte[] EnlightenData { get; set; }
		public int EnlightenDataVersion { get; set; }

		public const string SceneName = "m_Scene";
		public const string LightmapsName = "m_Lightmaps";
		public const string AOTexturesName = "m_AOTextures";
		public const string LightmapsCacheFilesName = "m_LightmapsCacheFiles";
		public const string LightProbesName = "m_LightProbes";
		public const string LightmapsModeName = "m_LightmapsMode";
		public const string BakedAmbientProbeInLinearName = "m_BakedAmbientProbeInLinear";
		public const string LightmappedRendererDataName = "m_LightmappedRendererData";
		public const string LightmappedRendererDataIDsName = "m_LightmappedRendererDataIDs";
		public const string EnlightenSceneMappingName = "m_EnlightenSceneMapping";
		public const string EnlightenSceneMappingRendererIDsName = "m_EnlightenSceneMappingRendererIDs";
		public const string LightsName = "m_Lights";
		public const string LightBakingOutputsName = "m_LightBakingOutputs";
		public const string BakedReflectionProbeCubemapCacheFilesName = "m_BakedReflectionProbeCubemapCacheFiles";
		public const string BakedReflectionProbeCubemapsName = "m_BakedReflectionProbeCubemaps";
		public const string BakedReflectionProbesName = "m_BakedReflectionProbes";
		public const string EnlightenDataName = "m_EnlightenData";
		public const string EnlightenDataVersionName = "m_EnlightenDataVersion";

		public PPtr<SceneAsset> Scene;
		public PPtr<LightProbes> LightProbes;
		public SphericalHarmonicsL2 BakedAmbientProbeInLinear;
		public EnlightenSceneMapping EnlightenSceneMapping;
	}
}
