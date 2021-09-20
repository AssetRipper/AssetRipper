using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Configuration;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Project.Exporters.Script;
using AssetRipper.Core.Structure;
using System;
using System.Collections.Generic;
using Object = AssetRipper.Core.Classes.Object.Object;

namespace AssetRipper.Core.Project
{
	public class ProjectExporter
	{
		public event Action EventExportPreparationStarted;
		public event Action EventExportPreparationFinished;
		public event Action EventExportStarted;
		public event Action<int, int> EventExportProgressUpdated;
		public event Action EventExportFinished;

		public ProjectExporter(IFileCollection fileCollection, CoreConfiguration configuration)
		{
			m_fileCollection = fileCollection;

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
			OverrideYamlExporter(ClassIDType.AnimationClip);
			OverrideYamlExporter(ClassIDType.TagManager);
			OverrideYamlExporter(ClassIDType.AudioListener);
			OverrideYamlExporter(ClassIDType.AudioSource);
			OverrideYamlExporter(ClassIDType.RenderTexture);
			OverrideYamlExporter(ClassIDType.Cubemap);
			OverrideYamlExporter(ClassIDType.Avatar);
			OverrideYamlExporter(ClassIDType.AnimatorController);
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

			OverrideBinaryExporter(ClassIDType.Shader);

			OverrideExporter(ClassIDType.MonoScript, new ScriptExporter(m_fileCollection.AssemblyManager));
		}

		/// <summary>Adds an exporter to the stack of exporters for this asset type.</summary>
		/// <param name="classType">The class id for this asset type</param>
		/// <param name="exporter">The new exporter. If it doesn't work, the next one in the stack is used.</param>
		public void OverrideExporter(ClassIDType classType, IAssetExporter exporter)
		{
			if (exporter == null)
			{
				throw new ArgumentNullException(nameof(exporter));
			}
			if (!m_exporters.ContainsKey(classType))
			{
				m_exporters[classType] = new Stack<IAssetExporter>(2);
			}
			m_exporters[classType].Push(exporter);
		}

		public void OverrideDummyExporter(ClassIDType classType, bool isEmptyCollection, bool isMetaType)
		{
			DummyExporter.SetUpClassType(classType, isEmptyCollection, isMetaType);
			OverrideExporter(classType, DummyExporter);
		}

		public void OverrideYamlExporter(ClassIDType classType) => OverrideExporter(classType, YamlExporter);

		public void OverrideBinaryExporter(ClassIDType classType) => OverrideExporter(classType, BinaryExporter);

		public void Export(GameCollection fileCollection, CoreConfiguration options) => Export(fileCollection, fileCollection.FetchSerializedFiles(), options);
		public void Export(GameCollection fileCollection, SerializedFile file, CoreConfiguration options) => Export(fileCollection, new SerializedFile[] { file }, options);
		public void Export(GameCollection fileCollection, IEnumerable<SerializedFile> files, CoreConfiguration options)
		{
			EventExportPreparationStarted?.Invoke();

			LayoutInfo info = new LayoutInfo(options.Version, options.Platform, options.Flags);
			AssetLayout exportLayout = new AssetLayout(info);
			VirtualSerializedFile virtualFile = new VirtualSerializedFile(exportLayout);
			List<IExportCollection> collections = new List<IExportCollection>();

			// speed up fetching
			List<Object> depList = new List<Object>();
			HashSet<Object> depSet = new HashSet<Object>();
			HashSet<Object> queued = new HashSet<Object>();

			foreach (SerializedFile file in files)
			{
				foreach (Object asset in file.FetchAssets())
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
				Object asset = depList[i];
				if (!queued.Contains(asset))
				{
					IExportCollection collection = CreateCollection(virtualFile, asset);
					foreach (Object element in collection.Assets)
					{
						queued.Add(element);
					}
					collections.Add(collection);
				}

				if (options.ExportDependencies)
				{
					DependencyContext context = new DependencyContext(exportLayout, true);
					foreach (PPtr<Object> pointer in asset.FetchDependencies(context))
					{
						if (pointer.IsNull)
						{
							continue;
						}

						Object dependency = pointer.FindAsset(asset.File);
						if (dependency == null)
						{
							string hierarchy = $"[{asset.File.Name}]" + asset.File.GetAssetLogString(asset.PathID) + "." + context.GetPointerPath();
							Logger.Log(LogType.Warning, LogCategory.Export, $"{hierarchy}'s dependency {context.PointerName} = {pointer.ToLogString(asset.File)} wasn't found");
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

		public AssetType ToExportType(ClassIDType classID)
		{
			switch (classID)
			{
				// abstract objects
				case ClassIDType.Object:
					return AssetType.Meta;
				case ClassIDType.Renderer:
					return AssetType.Serialized;
				case ClassIDType.Texture:
					classID = ClassIDType.Texture2D;
					break;
				case ClassIDType.RuntimeAnimatorController:
					classID = ClassIDType.AnimatorController;
					break;
				case ClassIDType.Motion:
					return AssetType.Serialized;

				// not implemented yet
				case ClassIDType.Flare:
					return AssetType.Serialized;
				case ClassIDType.AudioMixerGroup:
					return AssetType.Serialized;
				case ClassIDType.EditorExtension:
					return AssetType.Serialized;
			}

			if (!m_exporters.ContainsKey(classID))
			{
				throw new NotImplementedException($"Export type for class {classID} is undefined");
			}
			Stack<IAssetExporter> exporters = m_exporters[classID];
			foreach (IAssetExporter exporter in exporters)
			{
				if (exporter.ToUnknownExportType(classID, out AssetType assetType))
				{
					return assetType;
				}
			}
			throw new NotSupportedException($"There is no exporter that know {nameof(AssetType)} for unknown asset '{classID}'");
		}

		private IExportCollection CreateCollection(VirtualSerializedFile file, Object asset)
		{
			Stack<IAssetExporter> exporters = m_exporters[asset.ClassID];
			foreach (IAssetExporter exporter in exporters)
			{
				if (exporter.IsHandle(asset))
				{
					return exporter.CreateCollection(file, asset);
				}
			}
			throw new Exception($"There is no exporter that can handle '{asset}'");
		}

		private YAMLAssetExporter YamlExporter { get; } = new YAMLAssetExporter();
		private BinaryAssetExporter BinaryExporter { get; } = new BinaryAssetExporter();
		private DummyAssetExporter DummyExporter { get; } = new DummyAssetExporter();

		private readonly Dictionary<ClassIDType, Stack<IAssetExporter>> m_exporters = new Dictionary<ClassIDType, Stack<IAssetExporter>>();

		private readonly IFileCollection m_fileCollection;
	}
}
