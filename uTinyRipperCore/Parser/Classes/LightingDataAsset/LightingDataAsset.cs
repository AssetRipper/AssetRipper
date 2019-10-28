using System.Collections.Generic;
using uTinyRipper.Project;
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
	/// LightmapSnapshot previously
	/// </summary>
	public sealed class LightingDataAsset : NamedObject
	{
		public LightingDataAsset(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool IsReadAOTextures(Version version)
		{
			return version.IsGreaterEqual(2019);
		}
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool IsReadLightmapsCacheFiles(Version version)
		{
			return version.IsGreaterEqual(2018, 2);
		}
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool IsReadBakedReflectionProbeCubemapCacheFiles(Version version)
		{
			return version.IsGreaterEqual(2018, 2);
		}

		private static int GetSerializedVersion(Version version)
		{
			return 4;
			/*if (version.IsGreaterEqual())
			{
				return 4;
			}
			if (version.IsGreaterEqual())
			{
				return 3;
			}
			if (version.IsGreaterEqual())
			{
				return 2;
			}
			return 1;*/
		}


		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Scene.Read(reader);

			m_lightmaps = reader.ReadAssetArray<LightmapData>();
			reader.AlignStream(AlignType.Align4);

			if (IsReadAOTextures(reader.Version))
			{
				m_AOTextures = reader.ReadAssetArray<PPtr<Texture2D>>();
			}
			if (IsReadLightmapsCacheFiles(reader.Version))
			{
				m_lightmapsCacheFiles = reader.ReadStringArray();
			}

			LightProbes.Read(reader);
			LightmapsMode = reader.ReadInt32();
			BakedAmbientProbeInLinear.Read(reader);
			m_lightmappedRendererData = reader.ReadAssetArray<RendererData>();
			reader.AlignStream(AlignType.Align4);

			m_lightmappedRendererDataIDs = reader.ReadAssetArray<SceneObjectIdentifier>();
			reader.AlignStream(AlignType.Align4);

			EnlightenSceneMapping.Read(reader);
			m_enlightenSceneMappingRendererIDs = reader.ReadAssetArray<SceneObjectIdentifier>();
			reader.AlignStream(AlignType.Align4);

			m_lights = reader.ReadAssetArray<SceneObjectIdentifier>();
			reader.AlignStream(AlignType.Align4);

			m_lightBakingOutputs = reader.ReadAssetArray<LightBakingOutput>();
			reader.AlignStream(AlignType.Align4);

			if (IsReadBakedReflectionProbeCubemapCacheFiles(reader.Version))
			{
				m_bakedReflectionProbeCubemapCacheFiles = reader.ReadStringArray();
			}
			m_bakedReflectionProbeCubemaps = reader.ReadAssetArray<PPtr<Texture>>();
			m_bakedReflectionProbes = reader.ReadAssetArray<SceneObjectIdentifier>();
			reader.AlignStream(AlignType.Align4);

			m_enlightenData = reader.ReadByteArray();
			reader.AlignStream(AlignType.Align4);

			EnlightenDataVersion = reader.ReadInt32();
		}

		public override IEnumerable<Object> FetchDependencies(IDependencyContext context)
		{
			foreach (Object asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(Scene, SceneName);
			foreach (Object asset in context.FetchDependencies(Lightmaps, LightmapsName))
			{
				yield return asset;
			}
			yield return context.FetchDependency(LightProbes, LightProbesName);
			foreach (Object asset in context.FetchDependencies(LightmappedRendererData, LightmappedRendererDataName))
			{
				yield return asset;
			}
			foreach (Object asset in context.FetchDependencies(EnlightenSceneMapping, EnlightenSceneMappingName))
			{
				yield return asset;
			}
			foreach (Object asset in context.FetchDependencies(BakedReflectionProbeCubemaps, BakedReflectionProbeCubemapsName))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(SceneName, Scene.ExportYAML(container));
			node.Add(LightmapsName, Lightmaps.ExportYAML(container));
			if (IsReadAOTextures(container.ExportVersion))
			{
				node.Add(AOTexturesName, GetAOTextures(container.Version).ExportYAML(container));
			}
			if (IsReadLightmapsCacheFiles(container.Version))
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
			if (IsReadBakedReflectionProbeCubemapCacheFiles(container.ExportVersion))
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
			return IsReadAOTextures(version) ? AOTextures : System.Array.Empty<PPtr<Texture2D>>();
		}
		private IReadOnlyList<string> GetLightmapsCacheFiles(Version version)
		{
			return IsReadLightmapsCacheFiles(version) ? LightmapsCacheFiles : System.Array.Empty<string>();
		}
		private IReadOnlyList<string> GetBakedReflectionProbeCubemapCacheFiles(Version version)
		{
			return IsReadBakedReflectionProbeCubemapCacheFiles(version) ? BakedReflectionProbeCubemapCacheFiles : System.Array.Empty<string>();
		}

		public IReadOnlyList<LightmapData> Lightmaps => m_lightmaps;
		public IReadOnlyList<PPtr<Texture2D>> AOTextures => m_AOTextures;
		public IReadOnlyList<string> LightmapsCacheFiles => m_lightmapsCacheFiles;
		public int LightmapsMode { get; private set; }
		public IReadOnlyList<RendererData> LightmappedRendererData => m_lightmappedRendererData;
		public IReadOnlyList<SceneObjectIdentifier> LightmappedRendererDataIDs => m_lightmappedRendererDataIDs;
		public IReadOnlyList<SceneObjectIdentifier> EnlightenSceneMappingRendererIDs => m_enlightenSceneMappingRendererIDs;
		public IReadOnlyList<SceneObjectIdentifier> Lights => m_lights;
		public IReadOnlyList<LightBakingOutput> LightBakingOutputs => m_lightBakingOutputs;
		public IReadOnlyList<string> BakedReflectionProbeCubemapCacheFiles => m_bakedReflectionProbeCubemapCacheFiles;
		public IReadOnlyList<PPtr<Texture>> BakedReflectionProbeCubemaps => m_bakedReflectionProbeCubemaps;
		public IReadOnlyList<SceneObjectIdentifier> BakedReflectionProbes => m_bakedReflectionProbes;
		public IReadOnlyList<byte> EnlightenData => m_enlightenData;
		public int EnlightenDataVersion { get; private set; }

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

		private LightmapData[] m_lightmaps;
		private PPtr<Texture2D>[] m_AOTextures;
		private string[] m_lightmapsCacheFiles;
		private RendererData[] m_lightmappedRendererData;
		private SceneObjectIdentifier[] m_lightmappedRendererDataIDs;
		private SceneObjectIdentifier[] m_enlightenSceneMappingRendererIDs;
		private SceneObjectIdentifier[] m_lights;
		private LightBakingOutput[] m_lightBakingOutputs;
		private string[] m_bakedReflectionProbeCubemapCacheFiles;
		private PPtr<Texture>[] m_bakedReflectionProbeCubemaps;
		private SceneObjectIdentifier[] m_bakedReflectionProbes;
		private byte[] m_enlightenData;
	}
}
