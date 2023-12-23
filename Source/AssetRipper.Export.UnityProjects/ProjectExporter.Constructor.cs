using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.AnimatorControllers;
using AssetRipper.Export.UnityProjects.Audio;
using AssetRipper.Export.UnityProjects.AudioMixers;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Export.UnityProjects.EngineAssets;
using AssetRipper.Export.UnityProjects.Meshes;
using AssetRipper.Export.UnityProjects.Miscellaneous;
using AssetRipper.Export.UnityProjects.Models;
using AssetRipper.Export.UnityProjects.NavMeshes;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.Export.UnityProjects.RawAssets;
using AssetRipper.Export.UnityProjects.Scripts;
using AssetRipper.Export.UnityProjects.Shaders;
using AssetRipper.Export.UnityProjects.Terrains;
using AssetRipper.Export.UnityProjects.Textures;
using AssetRipper.Import.AssetCreation;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Processing.Textures;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_1032;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Classes.ClassID_116;
using AssetRipper.SourceGenerated.Classes.ClassID_117;
using AssetRipper.SourceGenerated.Classes.ClassID_128;
using AssetRipper.SourceGenerated.Classes.ClassID_141;
using AssetRipper.SourceGenerated.Classes.ClassID_142;
using AssetRipper.SourceGenerated.Classes.ClassID_147;
using AssetRipper.SourceGenerated.Classes.ClassID_150;
using AssetRipper.SourceGenerated.Classes.ClassID_152;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_187;
using AssetRipper.SourceGenerated.Classes.ClassID_188;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_238;
using AssetRipper.SourceGenerated.Classes.ClassID_240;
using AssetRipper.SourceGenerated.Classes.ClassID_244;
using AssetRipper.SourceGenerated.Classes.ClassID_27;
using AssetRipper.SourceGenerated.Classes.ClassID_272;
using AssetRipper.SourceGenerated.Classes.ClassID_273;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_290;
using AssetRipper.SourceGenerated.Classes.ClassID_3;
using AssetRipper.SourceGenerated.Classes.ClassID_329;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using AssetRipper.SourceGenerated.Classes.ClassID_6;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;
using AssetRipper.SourceGenerated.Classes.ClassID_83;
using AssetRipper.SourceGenerated.Classes.ClassID_94;

namespace AssetRipper.Export.UnityProjects;

partial class ProjectExporter
{
	public ProjectExporter(LibraryConfiguration Settings, IAssemblyManager assemblyManager)
	{
		OverrideExporter<IUnityObjectBase>(new DefaultYamlExporter(), true);

		OverrideExporter<IGlobalGameManager>(new ManagerAssetExporter(), true);

		OverrideExporter<IBuildSettings>(new BuildSettingsExporter(), true);

		OverrideExporter<IMonoBehaviour>(new ScriptableObjectExporter(), true);

		SceneYamlExporter sceneExporter = new();
		OverrideExporter<IPrefabInstance>(sceneExporter, true);
		OverrideExporter<IGameObject>(sceneExporter, true);
		OverrideExporter<IComponent>(sceneExporter, true);
		OverrideExporter<ILevelGameManager>(sceneExporter, true);

		OverrideDummyExporter<IPreloadData>(ClassIDType.PreloadData, true, false);
		OverrideDummyExporter<IAssetBundle>(ClassIDType.AssetBundle, true, false);
		OverrideDummyExporter<IAssetBundleManifest>(ClassIDType.AssetBundleManifest, true, false);
		OverrideDummyExporter<IMonoManager>(ClassIDType.MonoManager, true, false);
		OverrideDummyExporter<IResourceManager>(ClassIDType.ResourceManager, true, false);
		OverrideDummyExporter<IShaderNameRegistry>(ClassIDType.ShaderNameRegistry, true, false);

		OverrideExporter<ISceneAsset>(new SceneAssetExporter(), true);
		OverrideExporter<UnknownObject>(new UnknownObjectExporter(), false);
		OverrideExporter<UnreadableObject>(new UnreadableObjectExporter(), false);

		//Yaml Exporters
		YamlStreamedAssetExporter streamedAssetExporter = new();
		OverrideExporter<IMesh>(streamedAssetExporter);
		OverrideExporter<ITexture2D>(streamedAssetExporter);//ICubemap also by inheritance
		OverrideExporter<ITexture3D>(streamedAssetExporter);
		OverrideExporter<ITexture2DArray>(streamedAssetExporter);
		OverrideExporter<ICubemapArray>(streamedAssetExporter);

		//Miscellaneous exporters
		OverrideExporter<ITextAsset>(new TextAssetExporter(Settings));
		OverrideExporter<IMovieTexture>(new MovieTextureAssetExporter());
		OverrideExporter<IVideoClip>(new VideoClipExporter());

		//Texture exporters
		TextureAssetExporter textureExporter = new(Settings);
		OverrideExporter<ITexture2D>(textureExporter); //Texture2D and Cubemap
		OverrideExporter<ISprite>(textureExporter);
		OverrideExporter<SpriteInformationObject>(textureExporter);
		if (Settings.SpriteExportMode == SpriteExportMode.Yaml)
		{
			YamlSpriteExporter spriteExporter = new();
			OverrideExporter<ISprite>(spriteExporter);
			OverrideExporter<ISpriteAtlas>(spriteExporter);
		}

		//Texture Array exporters
		if (Settings.Version.GreaterThanOrEquals(2020, 2))
		{
			TextureArrayAssetExporter textureArrayExporter = new(Settings);
			OverrideExporter<ICubemapArray>(textureArrayExporter);
			OverrideExporter<ITexture2DArray>(textureArrayExporter);
			OverrideExporter<ITexture3D>(textureArrayExporter);
		}

		//Font exporter
		FontAssetExporter fontAssetExporter = new FontAssetExporter();
		OverrideExporter<IFont>(fontAssetExporter);
		OverrideExporter<IMaterial>(fontAssetExporter);
		OverrideExporter<ITexture>(fontAssetExporter);

		//Shader exporters
		OverrideExporter<IShader>(Settings.ShaderExportMode switch
		{
			ShaderExportMode.Yaml => new YamlShaderExporter(),
			ShaderExportMode.Disassembly => new ShaderDisassemblyExporter(),
			ShaderExportMode.Decompile => new USCShaderExporter(),
			_ => new DummyShaderTextExporter(),
		});
		OverrideExporter<IShader>(new SimpleShaderExporter());

		//Audio exporters
		OverrideExporter<IAudioClip>(new YamlAudioExporter());
		if (Settings.AudioExportFormat == AudioExportFormat.Native)
		{
			OverrideExporter<IAudioClip>(new NativeAudioExporter());
		}
		if (AudioClipExporter.IsSupportedExportFormat(Settings.AudioExportFormat))
		{
			OverrideExporter<IAudioClip>(new AudioClipExporter(Settings));
		}

		//AudioMixer exporters
		AudioMixerExporter audioMixerExporter = new();
		OverrideExporter<IAudioMixer>(audioMixerExporter);
		OverrideExporter<IAudioMixerEffectController>(audioMixerExporter);
		OverrideExporter<IAudioMixerGroup>(audioMixerExporter);
		OverrideExporter<IAudioMixerSnapshot>(audioMixerExporter);

		//Mesh and Model exporters
		if (Settings.MeshExportFormat == MeshExportFormat.Glb)
		{
			OverrideExporter<IMesh>(new GlbMeshExporter());
			GlbModelExporter glbModelExporter = new();
			OverrideExporter<IComponent>(glbModelExporter);
			OverrideExporter<IGameObject>(glbModelExporter);
			OverrideExporter<ILevelGameManager>(glbModelExporter);
		}

		//Terrain and NavMesh exporters
		switch (Settings.TerrainExportMode)
		{
			case TerrainExportMode.Heatmap:
				OverrideExporter<ITerrainData>(new TerrainHeatmapExporter(Settings));
				break;
			case TerrainExportMode.Mesh:
				OverrideExporter<ITerrainData>(new TerrainMeshExporter());
				OverrideExporter<INavMeshData>(new GlbNavMeshExporter());
				break;
			default:
				TerrainYamlExporter terrainYamlExporter = new();
				OverrideExporter<ITerrainData>(terrainYamlExporter);
				OverrideExporter<ITexture2D>(terrainYamlExporter);
				break;
		}

		//Script exporter
		OverrideExporter<IMonoScript>(new ScriptExporter(assemblyManager, Settings));

		//Animator Controller
		OverrideExporter<IUnityObjectBase>(new AnimatorControllerExporter());

		if (!Settings.IgnoreEngineAssets)
		{
			OverrideExporter<IUnityObjectBase>(EngineAssetsExporter.CreateFromEmbeddedData(Settings.Version));
		}
	}
}
