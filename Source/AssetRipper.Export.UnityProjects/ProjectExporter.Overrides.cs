using AssetRipper.Assets;
using AssetRipper.Export.Configuration;
using AssetRipper.Export.UnityProjects.AnimatorControllers;
using AssetRipper.Export.UnityProjects.Audio;
using AssetRipper.Export.UnityProjects.AudioMixers;
using AssetRipper.Export.UnityProjects.DeletedAssets;
using AssetRipper.Export.UnityProjects.EngineAssets;
using AssetRipper.Export.UnityProjects.Miscellaneous;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.Export.UnityProjects.RawAssets;
using AssetRipper.Export.UnityProjects.Scripts;
using AssetRipper.Export.UnityProjects.Shaders;
using AssetRipper.Export.UnityProjects.Terrains;
using AssetRipper.Export.UnityProjects.Textures;
using AssetRipper.Import.AssetCreation;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Mining.PredefinedAssets;
using AssetRipper.Processing.ScriptableObject;
using AssetRipper.Processing.Textures;
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
using AssetRipper.SourceGenerated.Classes.ClassID_189;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
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
	public ProjectExporter(FullConfiguration settings, IAssemblyManager assemblyManager)
	{
		OverrideExporter<IUnityObjectBase>(new DefaultYamlExporter(), true);

		ManagerAssetExporter managerExporter = new();
		OverrideExporter<IGlobalGameManager>(managerExporter, true);
		OverrideExporter<TypeTreeObject>(managerExporter, true);

		OverrideExporter<IMonoBehaviour>(new ScriptableObjectExporter(), true);

		SceneYamlExporter sceneExporter = new();
		OverrideExporter<IPrefabInstance>(sceneExporter, true);
		OverrideExporter<IGameObject>(sceneExporter, true);
		OverrideExporter<IComponent>(sceneExporter, true);
		OverrideExporter<ILevelGameManager>(sceneExporter, true);

		OverrideDummyExporter<IBuildSettings>(true, false);
		OverrideDummyExporter<IPreloadData>(true, false);
		OverrideDummyExporter<IAssetBundle>(true, false);
		OverrideDummyExporter<IAssetBundleManifest>(true, false);
		OverrideDummyExporter<IMonoManager>(true, false);
		OverrideDummyExporter<IResourceManager>(true, false);
		OverrideDummyExporter<IShaderNameRegistry>(true, false);

		OverrideExporter<ISceneAsset>(new SceneAssetExporter(), true);

		if (settings.ExportSettings.ExportUnreadableAssets)
		{
			OverrideExporter<UnknownObject>(new UnknownObjectExporter(), false);
			OverrideExporter<UnreadableObject>(new UnreadableObjectExporter(), false);
		}
		else
		{
			OverrideDummyExporter<UnknownObject>(false, false);
			OverrideDummyExporter<UnreadableObject>(false, false);
		}

		//Yaml Exporters
		YamlStreamedAssetExporter streamedAssetExporter = new();
		OverrideExporter<IMesh>(streamedAssetExporter);
		OverrideExporter<IImageTexture>(streamedAssetExporter);

		//Miscellaneous exporters
		OverrideExporter<ITextAsset>(new TextAssetExporter(settings));
		OverrideExporter<IMovieTexture>(new MovieTextureAssetExporter());
		OverrideExporter<IVideoClip>(new VideoClipExporter());

		//Texture exporters
		TextureAssetExporter textureExporter = new(settings);
		OverrideExporter<ITexture2D>(textureExporter); //Texture2D and Cubemap
		OverrideExporter<ISprite>(textureExporter);
		OverrideExporter<SpriteInformationObject>(textureExporter);
		if (settings.ExportSettings.SpriteExportMode == SpriteExportMode.Yaml)
		{
			YamlSpriteExporter spriteExporter = new();
			OverrideExporter<ISprite>(spriteExporter);
			OverrideExporter<ISpriteAtlas>(spriteExporter);
		}
		if (settings.ExportSettings.LightmapTextureExportFormat is not LightmapTextureExportFormat.Yaml)
		{
			OverrideExporter<ITexture2D>(new LightmapTextureAssetExporter(settings.ExportSettings.LightmapTextureExportFormat is LightmapTextureExportFormat.Exr
				? ImageExportFormat.Exr
				: settings.ExportSettings.ImageExportFormat));
		}

		//Texture Array exporters
		if (settings.Version.GreaterThanOrEquals(2020, 2))
		{
			TextureArrayAssetExporter textureArrayExporter = new(settings);
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
		OverrideExporter<IShader>(settings.ExportSettings.ShaderExportMode switch
		{
			ShaderExportMode.Yaml => new YamlShaderExporter(),
			ShaderExportMode.Disassembly => new ShaderDisassemblyExporter(),
			ShaderExportMode.Decompile => new USCShaderExporter(),
			_ => new DummyShaderTextExporter(),
		});
		OverrideExporter<IShader>(new SimpleShaderExporter());

		//Audio exporters
		OverrideExporter<IAudioClip>(new YamlAudioExporter());
		if (settings.ExportSettings.AudioExportFormat == AudioExportFormat.Native)
		{
			OverrideExporter<IAudioClip>(new NativeAudioExporter());
		}
		if (AudioClipExporter.IsSupportedExportFormat(settings.ExportSettings.AudioExportFormat))
		{
			OverrideExporter<IAudioClip>(new AudioClipExporter(settings));
		}

		//AudioMixer exporters
		AudioMixerExporter audioMixerExporter = new();
		OverrideExporter<IAudioMixer>(audioMixerExporter);
		OverrideExporter<IAudioMixerEffectController>(audioMixerExporter);
		OverrideExporter<IAudioMixerGroup>(audioMixerExporter);
		OverrideExporter<IAudioMixerSnapshot>(audioMixerExporter);

		//Terrain exporter
		TerrainYamlExporter terrainYamlExporter = new();
		OverrideExporter<ITerrainData>(terrainYamlExporter);
		OverrideExporter<ITexture2D>(terrainYamlExporter);

		//Script exporter
		OverrideExporter<IMonoScript>(new ScriptExporter(assemblyManager, settings));

		//Animator Controller
		OverrideExporter<IUnityObjectBase>(new AnimatorControllerExporter());

		//Playable assets
		ScriptableObjectGroupExporter scriptableObjectGroupExporter = new();
		OverrideExporter<IMonoBehaviour>(scriptableObjectGroupExporter);
		OverrideExporter<ScriptableObjectGroup>(scriptableObjectGroupExporter);
	}

	//These need to be absolutely last
	public void DoFinalOverrides(FullConfiguration settings)
	{
		//Engine assets
		OverrideExporter<IUnityObjectBase>(settings.SingletonData.TryGetStoredValue(nameof(EngineResourceData), out EngineResourceData? engineResourceData)
			&& engineResourceData is not null
			? EngineAssetsExporter.CreateFromResourceData(engineResourceData.Value)
			: EngineAssetsExporter.CreateFromEmbeddedData(settings.Version));

		//Deleted assets
		OverrideExporter<IUnityObjectBase>(new DeletedAssetsExporter());
	}
}
