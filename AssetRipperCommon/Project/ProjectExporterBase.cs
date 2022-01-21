using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.GameObject;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Configuration;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Structure;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Project
{
	public abstract class ProjectExporterBase : IProjectExporter
	{
		public event Action EventExportPreparationStarted;
		public event Action EventExportPreparationFinished;
		public event Action EventExportStarted;
		public event Action<int, int> EventExportProgressUpdated;
		public event Action EventExportFinished;

		//Exporters
		protected DefaultYamlExporter DefaultExporter { get; } = new DefaultYamlExporter();
		protected SceneYamlExporter SceneExporter { get; } = new SceneYamlExporter();
		protected ManagerAssetExporter ManagerExporter { get; } = new ManagerAssetExporter();
		protected BuildSettingsExporter BuildSettingsExporter { get; } = new BuildSettingsExporter();
		protected ScriptableObjectExporter ScriptableExporter { get; } = new ScriptableObjectExporter();
		protected DummyAssetExporter DummyExporter { get; } = new DummyAssetExporter();

		/// <summary>Adds an exporter to the stack of exporters for this asset type.</summary>
		/// <param name="classType">The class id for this asset type</param>
		/// <param name="exporter">The new exporter. If it doesn't work, the next one in the stack is used.</param>
		public abstract void OverrideExporter(ClassIDType classType, IAssetExporter exporter);

		/// <summary>Adds an exporter to the stack of exporters for this asset type.</summary>
		/// <typeparam name="T">The c sharp type of this asset type. Any inherited types also get this exporter.</typeparam>
		/// <param name="exporter">The new exporter. If it doesn't work, the next one in the stack is used.</param>
		/// <param name="allowInheritance">Should types that inherit from this type also use the exporter?</param>
		public void OverrideExporter<T>(IAssetExporter exporter, bool allowInheritance) => OverrideExporter(typeof(T), exporter, allowInheritance);
		/// <summary>Adds an exporter to the stack of exporters for this asset type.</summary>
		/// <param name="type">The c sharp type of this asset type. Any inherited types also get this exporter.</param>
		/// <param name="exporter">The new exporter. If it doesn't work, the next one in the stack is used.</param>
		/// <param name="allowInheritance">Should types that inherit from this type also use the exporter?</param>
		public abstract void OverrideExporter(Type type, IAssetExporter exporter, bool allowInheritance);

		public abstract AssetType ToExportType(ClassIDType classID);
		protected abstract IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset);

		protected void OverrideYamlExporter(ClassIDType classType)
		{
			OverrideExporter(classType, DefaultExporter);
			OverrideExporter(classType, ScriptableExporter);
			OverrideExporter(classType, ManagerExporter);
			OverrideExporter(classType, BuildSettingsExporter);
			OverrideExporter(classType, SceneExporter);
		}

		protected void OverrideDummyExporter(ClassIDType classType, bool isEmptyCollection, bool isMetaType)
		{
			DummyExporter.SetUpClassType(classType, isEmptyCollection, isMetaType);
			OverrideExporter(classType, DummyExporter);
		}

		public void Export(GameCollection fileCollection, CoreConfiguration options) => Export(fileCollection, fileCollection.FetchSerializedFiles(), options);
		public void Export(GameCollection fileCollection, SerializedFile file, CoreConfiguration options) => Export(fileCollection, new SerializedFile[] { file }, options);
		public void Export(GameCollection fileCollection, IEnumerable<SerializedFile> files, CoreConfiguration options)
		{
			EventExportPreparationStarted?.Invoke();

			LayoutInfo exportLayout = new LayoutInfo(options.Version, options.Platform, options.Flags);
			VirtualSerializedFile virtualFile = new VirtualSerializedFile(exportLayout);
			List<IExportCollection> collections = new List<IExportCollection>();

			foreach (SerializedFile file in files)
			{
				foreach (IUnityObjectBase asset in file.FetchAssets())
				{
					asset.ConvertToEditor();
					//StaticMeshConverter.MaybeReplaceStaticMesh(asset, file, virtualFile);
				}
			}

			// speed up fetching
			List<IUnityObjectBase> depList = new List<IUnityObjectBase>();
			HashSet<IUnityObjectBase> depSet = new HashSet<IUnityObjectBase>();
			HashSet<IUnityObjectBase> queued = new HashSet<IUnityObjectBase>();

			foreach (SerializedFile file in files)
			{
				foreach (IUnityObjectBase asset in file.FetchAssets())
				{
					if (!options.Filter(asset))
					{
						continue;
					}

					depList.Add(asset);
					depSet.Add(asset);
				}
			}


			for (int i = 0; i < depList.Count; i++)
			{
				IUnityObjectBase asset = depList[i];
				if (!queued.Contains(asset))
				{
					IExportCollection collection = CreateCollection(virtualFile, asset);
					foreach (IUnityObjectBase element in collection.Assets)
					{
						queued.Add(element);
					}
					collections.Add(collection);
				}

				if (options.ExportDependencies && asset is IDependent dependent)
				{
					DependencyContext context = new DependencyContext(exportLayout, true);
					foreach (PPtr<IUnityObjectBase> pointer in dependent.FetchDependencies(context))
					{
						if (pointer.IsNull)
						{
							continue;
						}

						IUnityObjectBase dependency = pointer.FindAsset(asset.SerializedFile);
						if (dependency == null)
						{
							string hierarchy = $"[{asset.SerializedFile.Name}]" + asset.SerializedFile.GetAssetLogString(asset.PathID) + "." + context.GetPointerPath();
							Logger.Log(LogType.Warning, LogCategory.Export, $"{hierarchy}'s dependency {context.PointerName} = {pointer.ToLogString(asset.SerializedFile)} wasn't found");
							continue;
						}

						if (!depSet.Contains(dependency))
						{
							depList.Add(dependency);
							depSet.Add(dependency);
						}
					}
				}
			}
			depList.Clear();
			depSet.Clear();
			queued.Clear();
			EventExportPreparationFinished?.Invoke();

			EventExportStarted?.Invoke();
			ProjectAssetContainer container = new ProjectAssetContainer(this, options, virtualFile, fileCollection.FetchAssets(), collections);
			for (int i = 0; i < collections.Count; i++)
			{
				IExportCollection collection = collections[i];
				container.CurrentCollection = collection;
				bool isExported = collection.Export(container, options.ExportPath);
				if (isExported)
				{
					Logger.Info(LogCategory.ExportedFile, $"'{collection.Name}' exported");
				}
				EventExportProgressUpdated?.Invoke(i, collections.Count);
			}
			EventExportFinished?.Invoke();
		}

		public ProjectExporterBase()
		{
			OverrideExporter<IUnityObjectBase>(new RawAssetExporter(), true);
			OverrideExporter<IUnityObjectBase>(DefaultExporter, true);

			OverrideExporter<IGlobalGameManager>(ManagerExporter, true);

			OverrideExporter<IBuildSettings>(BuildSettingsExporter, true);

			OverrideExporter<IMonoBehaviour>(ScriptableExporter, true);

			OverrideExporter<IGameObject>(SceneExporter, true);
			OverrideExporter<IComponent>(SceneExporter, true);
			OverrideExporter<ILevelGameManager>(SceneExporter, true);


			OverrideDummyExporter(ClassIDType.MonoManager, true, false);
			OverrideDummyExporter(ClassIDType.BuildSettings, false, false);
			OverrideDummyExporter(ClassIDType.AssetBundle, true, false);
			OverrideDummyExporter(ClassIDType.ResourceManager, true, false);
			OverrideDummyExporter(ClassIDType.PreloadData, true, false);
			OverrideDummyExporter(ClassIDType.EditorSettings, false, false);
			OverrideDummyExporter(ClassIDType.Sprite, false, true);
			OverrideDummyExporter(ClassIDType.TextureImporter, false, false);
			OverrideDummyExporter(ClassIDType.DefaultAsset, false, false);
			OverrideDummyExporter(ClassIDType.DefaultImporter, false, false);
			OverrideDummyExporter(ClassIDType.NativeFormatImporter, false, false);
			OverrideDummyExporter(ClassIDType.MonoImporter, false, false);
			OverrideDummyExporter(ClassIDType.DDSImporter, false, false);
			OverrideDummyExporter(ClassIDType.PVRImporter, false, false);
			OverrideDummyExporter(ClassIDType.ASTCImporter, false, false);
			OverrideDummyExporter(ClassIDType.KTXImporter, false, false);
			OverrideDummyExporter(ClassIDType.IHVImageFormatImporter, false, false);
			OverrideDummyExporter(ClassIDType.SpriteAtlas, false, false);

			OverrideYamlExporter(ClassIDType.GameObject);
			OverrideYamlExporter(ClassIDType.Transform);
			OverrideYamlExporter(ClassIDType.TimeManager);
			OverrideYamlExporter(ClassIDType.AudioManager);
			OverrideYamlExporter(ClassIDType.InputManager);
			OverrideYamlExporter(ClassIDType.Physics2DSettings);
			OverrideYamlExporter(ClassIDType.Camera);
			OverrideYamlExporter(ClassIDType.Material);
			OverrideYamlExporter(ClassIDType.MeshRenderer);
			OverrideYamlExporter(ClassIDType.Texture2D);
			OverrideYamlExporter(ClassIDType.OcclusionCullingSettings);
			OverrideYamlExporter(ClassIDType.GraphicsSettings);
			OverrideYamlExporter(ClassIDType.MeshFilter);
			OverrideYamlExporter(ClassIDType.OcclusionPortal);
			OverrideYamlExporter(ClassIDType.Mesh);
			OverrideYamlExporter(ClassIDType.Skybox);
			OverrideYamlExporter(ClassIDType.QualitySettings);
			OverrideYamlExporter(ClassIDType.TextAsset);
			OverrideYamlExporter(ClassIDType.Rigidbody2D);
			OverrideYamlExporter(ClassIDType.Collider2D);
			OverrideYamlExporter(ClassIDType.Rigidbody);
			OverrideYamlExporter(ClassIDType.PhysicsManager);
			OverrideYamlExporter(ClassIDType.CircleCollider2D);
			OverrideYamlExporter(ClassIDType.PolygonCollider2D);
			OverrideYamlExporter(ClassIDType.BoxCollider2D);
			OverrideYamlExporter(ClassIDType.PhysicsMaterial2D);
			OverrideYamlExporter(ClassIDType.MeshCollider);
			OverrideYamlExporter(ClassIDType.BoxCollider);
			OverrideYamlExporter(ClassIDType.CompositeCollider2D);
			OverrideYamlExporter(ClassIDType.EdgeCollider2D);
			OverrideYamlExporter(ClassIDType.CapsuleCollider2D);
			OverrideYamlExporter(ClassIDType.ComputeShader);
			OverrideYamlExporter(ClassIDType.AnimationClip);
			OverrideYamlExporter(ClassIDType.TagManager);
			OverrideYamlExporter(ClassIDType.AudioListener);
			OverrideYamlExporter(ClassIDType.AudioSource);
			OverrideYamlExporter(ClassIDType.RenderTexture);
			OverrideYamlExporter(ClassIDType.Cubemap);
			OverrideYamlExporter(ClassIDType.Avatar);
			OverrideYamlExporter(ClassIDType.GUILayer);
			OverrideYamlExporter(ClassIDType.Animator);
			OverrideYamlExporter(ClassIDType.TextMesh);
			OverrideYamlExporter(ClassIDType.RenderSettings);
			OverrideYamlExporter(ClassIDType.Light);
			OverrideYamlExporter(ClassIDType.Animation);
			OverrideYamlExporter(ClassIDType.TrailRenderer);
			OverrideYamlExporter(ClassIDType.MonoBehaviour);
			OverrideYamlExporter(ClassIDType.Texture3D);
			OverrideYamlExporter(ClassIDType.NewAnimationTrack);
			OverrideYamlExporter(ClassIDType.FlareLayer);
			OverrideYamlExporter(ClassIDType.NavMeshProjectSettings);
			OverrideYamlExporter(ClassIDType.Font);
			OverrideYamlExporter(ClassIDType.GUITexture);
			OverrideYamlExporter(ClassIDType.GUIText);
			OverrideYamlExporter(ClassIDType.PhysicMaterial);
			OverrideYamlExporter(ClassIDType.SphereCollider);
			OverrideYamlExporter(ClassIDType.CapsuleCollider);
			OverrideYamlExporter(ClassIDType.SkinnedMeshRenderer);
			OverrideYamlExporter(ClassIDType.BuildSettings);
			OverrideYamlExporter(ClassIDType.CharacterController);
			OverrideYamlExporter(ClassIDType.WheelCollider);
			OverrideYamlExporter(ClassIDType.NetworkManager);
			OverrideYamlExporter(ClassIDType.MovieTexture);
			OverrideYamlExporter(ClassIDType.TerrainCollider);
			OverrideYamlExporter(ClassIDType.TerrainData);
			OverrideYamlExporter(ClassIDType.LightmapSettings);
			OverrideYamlExporter(ClassIDType.AudioReverbZone);
			OverrideYamlExporter(ClassIDType.WindZone);
			OverrideYamlExporter(ClassIDType.OffMeshLink);
			OverrideYamlExporter(ClassIDType.OcclusionArea);
			OverrideYamlExporter(ClassIDType.NavMeshObsolete);
			OverrideYamlExporter(ClassIDType.NavMeshAgent);
			OverrideYamlExporter(ClassIDType.NavMeshSettings);
			OverrideYamlExporter(ClassIDType.ParticleSystem);
			OverrideYamlExporter(ClassIDType.ParticleSystemRenderer);
			OverrideYamlExporter(ClassIDType.ShaderVariantCollection);
			OverrideYamlExporter(ClassIDType.LODGroup);
			OverrideYamlExporter(ClassIDType.NavMeshObstacle);
			OverrideYamlExporter(ClassIDType.SortingGroup);
			OverrideYamlExporter(ClassIDType.SpriteRenderer);
			OverrideYamlExporter(ClassIDType.ReflectionProbe);
			OverrideYamlExporter(ClassIDType.Terrain);
			OverrideYamlExporter(ClassIDType.AnimatorOverrideController);
			OverrideYamlExporter(ClassIDType.CanvasRenderer);
			OverrideYamlExporter(ClassIDType.Canvas);
			OverrideYamlExporter(ClassIDType.RectTransform);
			OverrideYamlExporter(ClassIDType.CanvasGroup);
			OverrideYamlExporter(ClassIDType.ClusterInputManager);
			OverrideYamlExporter(ClassIDType.NavMeshData);
			OverrideYamlExporter(ClassIDType.LightProbes);
			OverrideYamlExporter(ClassIDType.UnityConnectSettings);
			OverrideYamlExporter(ClassIDType.AvatarMask);
			OverrideYamlExporter(ClassIDType.ParticleSystemForceField);
			OverrideYamlExporter(ClassIDType.OcclusionCullingData);
			OverrideYamlExporter(ClassIDType.PrefabInstance);
			OverrideYamlExporter(ClassIDType.AvatarMaskOld);
			OverrideYamlExporter(ClassIDType.SceneAsset);
			OverrideYamlExporter(ClassIDType.LightmapParameters);
			OverrideYamlExporter(ClassIDType.SpriteAtlas);
			OverrideYamlExporter(ClassIDType.TerrainLayer);
			OverrideYamlExporter(ClassIDType.LightingSettings);
		}
	}
}
