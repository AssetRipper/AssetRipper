using System;
using System.Collections.Generic;
using System.IO;
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
			return !flags.IsForInspector() || (((int)flags & 0x10) != 0);
		}
		/// <summary>
		/// Not Inspector
		/// </summary>
		public static bool IsReadEnlightenData(TransferInstructionFlags flags)
		{
			return !flags.IsForInspector();
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


		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Scene.Read(reader);
			if(IsReadLightmaps(reader.Flags))
			{
				m_lightmaps = reader.ReadArray<LightmapData>();
				reader.AlignStream(AlignType.Align4);

				LightProbes.Read(reader);
				LightmapsMode = reader.ReadInt32();
				BakedAmbientProbeInLinear.Read(reader);
				m_lightmappedRendererData = reader.ReadArray<RendererData>();
				reader.AlignStream(AlignType.Align4);

				m_lightmappedRendererDataIDs = reader.ReadArray<SceneObjectIdentifier>();
				reader.AlignStream(AlignType.Align4);

				EnlightenSceneMapping.Read(reader);
				m_enlightenSceneMappingRendererIDs = reader.ReadArray<SceneObjectIdentifier>();
				reader.AlignStream(AlignType.Align4);

				m_lights = reader.ReadArray<SceneObjectIdentifier>();
				reader.AlignStream(AlignType.Align4);

				m_lightBakingOutputs = reader.ReadArray<LightBakingOutput>();
				reader.AlignStream(AlignType.Align4);

				m_bakedReflectionProbeCubemaps = reader.ReadArray<PPtr<Texture>>();
				m_bakedReflectionProbes = reader.ReadArray<SceneObjectIdentifier>();
				reader.AlignStream(AlignType.Align4);

				if (IsReadEnlightenData(reader.Flags))
				{
					m_enlightenData = reader.ReadByteArray();
					reader.AlignStream(AlignType.Align4);
				}

				EnlightenDataVersion = reader.ReadInt32();
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

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_Scene", Scene.ExportYAML(container));
			node.Add("m_Lightmaps", Lightmaps.ExportYAML(container));
			node.Add("m_LightProbes", LightProbes.ExportYAML(container));
			node.Add("m_LightmapsMode", LightmapsMode);
			node.Add("m_BakedAmbientProbeInLinear", BakedAmbientProbeInLinear.ExportYAML(container));
			node.Add("m_LightmappedRendererData", LightmappedRendererData.ExportYAML(container));
			node.Add("m_LightmappedRendererDataIDs", LightmappedRendererDataIDs.ExportYAML(container));
			node.Add("m_EnlightenSceneMapping", EnlightenSceneMapping.ExportYAML(container));
			node.Add("m_EnlightenSceneMappingRendererIDs", EnlightenSceneMappingRendererIDs.ExportYAML(container));
			node.Add("m_Lights", Lights.ExportYAML(container));
			node.Add("m_LightBakingOutputs", LightBakingOutputs.ExportYAML(container));
			node.Add("m_BakedReflectionProbeCubemaps", BakedReflectionProbeCubemaps.ExportYAML(container));
			node.Add("m_BakedReflectionProbes", BakedReflectionProbes.ExportYAML(container));
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
