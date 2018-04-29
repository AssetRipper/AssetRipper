using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.LightingDataAssets;
using UtinyRipper.Classes.LightmapSettingss;
using UtinyRipper.Classes.Lights;
using UtinyRipper.Classes.OcclusionCullingDatas;
using UtinyRipper.Classes.RenderSettingss;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public sealed class LightingDataAsset : NamedObject
	{
		public LightingDataAsset(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// Not Inspector
		/// </summary>
		public static bool IsReadLightmaps(TransferInstructionFlags flags)
		{
			return !flags.IsSerializeForInspector() || (((int)flags & 0x10) != 0);
		}
		/// <summary>
		/// Not Inspector
		/// </summary>
		public static bool IsReadEnlightenData(TransferInstructionFlags flags)
		{
			return !flags.IsSerializeForInspector();
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 4;
			}

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
			throw new NotImplementedException();
		}


		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			Scene.Read(stream);
			if(IsReadLightmaps(stream.Flags))
			{
				m_lightmaps = stream.ReadArray<LightmapData>();
				stream.AlignStream(AlignType.Align4);

				LightProbes.Read(stream);
				LightmapsMode = stream.ReadInt32();
				BakedAmbientProbeInLinear.Read(stream);
				m_lightmappedRendererData = stream.ReadArray<RendererData>();
				stream.AlignStream(AlignType.Align4);

				m_lightmappedRendererDataIDs = stream.ReadArray<SceneObjectIdentifier>();
				stream.AlignStream(AlignType.Align4);

				EnlightenSceneMapping.Read(stream);
				m_enlightenSceneMappingRendererIDs = stream.ReadArray<SceneObjectIdentifier>();
				stream.AlignStream(AlignType.Align4);

				m_lights = stream.ReadArray<SceneObjectIdentifier>();
				stream.AlignStream(AlignType.Align4);

				m_lightBakingOutputs = stream.ReadArray<LightBakingOutput>();
				stream.AlignStream(AlignType.Align4);

				m_bakedReflectionProbeCubemaps = stream.ReadArray<PPtr<Texture>>();
				m_bakedReflectionProbes = stream.ReadArray<SceneObjectIdentifier>();
				stream.AlignStream(AlignType.Align4);

				if (IsReadEnlightenData(stream.Flags))
				{
					m_enlightenData = stream.ReadByteArray();
					stream.AlignStream(AlignType.Align4);
				}

				EnlightenDataVersion = stream.ReadInt32();
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}

			yield return Scene.FetchDependency(file, isLog, ToLogString, "m_Scene");
			foreach (LightmapData lightmapData in Lightmaps)
			{
				foreach (Object @object in lightmapData.FetchDependencies(file, isLog))
				{
					yield return @object;
				}
			}
			yield return LightProbes.FetchDependency(file, isLog, ToLogString, "m_LightProbes");
			foreach (RendererData rendererData in LightmappedRendererData)
			{
				foreach (Object @object in rendererData.FetchDependencies(file, isLog))
				{
					yield return @object;
				}
			}
			foreach (Object @object in EnlightenSceneMapping.FetchDependencies(file, isLog))
			{
				yield return @object;
			}
			foreach (PPtr<Texture> cubemap in BakedReflectionProbeCubemaps)
			{
				yield return cubemap.FetchDependency(file, isLog, ToLogString, "m_BakedReflectionProbeCubemaps");
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.Add("m_Scene", Scene.ExportYAML(exporter));
			node.Add("m_Lightmaps", Lightmaps.ExportYAML(exporter));
			node.Add("m_LightProbes", LightProbes.ExportYAML(exporter));
			node.Add("m_LightmapsMode", LightmapsMode);
			node.Add("m_BakedAmbientProbeInLinear", BakedAmbientProbeInLinear.ExportYAML(exporter));
			node.Add("m_LightmappedRendererData", LightmappedRendererData.ExportYAML(exporter));
			node.Add("m_LightmappedRendererDataIDs", LightmappedRendererDataIDs.ExportYAML(exporter));
			node.Add("m_EnlightenSceneMapping", EnlightenSceneMapping.ExportYAML(exporter));
			node.Add("m_EnlightenSceneMappingRendererIDs", EnlightenSceneMappingRendererIDs.ExportYAML(exporter));
			node.Add("m_Lights", Lights.ExportYAML(exporter));
			node.Add("m_LightBakingOutputs", LightBakingOutputs.ExportYAML(exporter));
			node.Add("m_BakedReflectionProbeCubemaps", BakedReflectionProbeCubemaps.ExportYAML(exporter));
			node.Add("m_BakedReflectionProbes", BakedReflectionProbes.ExportYAML(exporter));
			node.Add("m_EnlightenData", EnlightenData.ExportYAML());
			node.Add("m_EnlightenDataVersion", EnlightenDataVersion);
			return node;
		}

		public IReadOnlyList<LightmapData> Lightmaps => m_lightmaps;
		public int LightmapsMode { get; private set; }
		public IReadOnlyList<RendererData> LightmappedRendererData => m_lightmappedRendererData;
		public IReadOnlyList<SceneObjectIdentifier> LightmappedRendererDataIDs => m_lightmappedRendererDataIDs;
		public IReadOnlyList<SceneObjectIdentifier> EnlightenSceneMappingRendererIDs => m_enlightenSceneMappingRendererIDs;
		public IReadOnlyList<SceneObjectIdentifier> Lights => m_lights;
		public IReadOnlyList<LightBakingOutput> LightBakingOutputs => m_lightBakingOutputs;
		public IReadOnlyList<PPtr<Texture>> BakedReflectionProbeCubemaps => m_bakedReflectionProbeCubemaps;
		public IReadOnlyList<SceneObjectIdentifier> BakedReflectionProbes => m_bakedReflectionProbes;
		public IReadOnlyList<byte> EnlightenData => m_enlightenData;
		public int EnlightenDataVersion { get; private set; }

		public PPtr<SceneAsset> Scene;
		public PPtr<LightProbes> LightProbes;
		public SphericalHarmonicsL2 BakedAmbientProbeInLinear;
		public EnlightenSceneMapping EnlightenSceneMapping;

		private LightmapData[] m_lightmaps;
		private RendererData[] m_lightmappedRendererData;
		private SceneObjectIdentifier[] m_lightmappedRendererDataIDs;
		private SceneObjectIdentifier[] m_enlightenSceneMappingRendererIDs;
		private SceneObjectIdentifier[] m_lights;
		private LightBakingOutput[] m_lightBakingOutputs;
		private PPtr<Texture>[] m_bakedReflectionProbeCubemaps;
		private SceneObjectIdentifier[] m_bakedReflectionProbes;
		private byte[] m_enlightenData;
	}
}
