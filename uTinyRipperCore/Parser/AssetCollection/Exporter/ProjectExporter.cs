using System;
using System.Collections.Generic;
using uTinyRipper.SerializedFiles;

using Object = uTinyRipper.Classes.Object;

namespace uTinyRipper.AssetExporters
{
	public class ProjectExporter
	{
		public ProjectExporter(IFileCollection fileCollection)
		{
			m_fileCollection = fileCollection;

			OverrideExporter(ClassIDType.MonoManager, DummyExporter);
			OverrideExporter(ClassIDType.BuildSettings, DummyExporter);
			OverrideExporter(ClassIDType.AssetBundle, DummyExporter);
			OverrideExporter(ClassIDType.ResourceManager, DummyExporter);
			OverrideExporter(ClassIDType.PreloadData, DummyExporter);
			OverrideExporter(ClassIDType.Sprite, DummyExporter);
			OverrideExporter(ClassIDType.SpriteAtlas, DummyExporter);

			OverrideExporter(ClassIDType.GameObject, YamlExporter);
			OverrideExporter(ClassIDType.Transform, YamlExporter);
			OverrideExporter(ClassIDType.TimeManager, YamlExporter);
			OverrideExporter(ClassIDType.AudioManager, YamlExporter);
			OverrideExporter(ClassIDType.InputManager, YamlExporter);
			OverrideExporter(ClassIDType.Physics2DSettings, YamlExporter);
			OverrideExporter(ClassIDType.Camera, YamlExporter);
			OverrideExporter(ClassIDType.Material, YamlExporter);
			OverrideExporter(ClassIDType.MeshRenderer, YamlExporter);
			OverrideExporter(ClassIDType.OcclusionCullingSettings, YamlExporter);
			OverrideExporter(ClassIDType.GraphicsSettings, YamlExporter);
			OverrideExporter(ClassIDType.MeshFilter, YamlExporter);
			OverrideExporter(ClassIDType.OcclusionPortal, YamlExporter);
			OverrideExporter(ClassIDType.Mesh, YamlExporter);
			OverrideExporter(ClassIDType.QualitySettings, YamlExporter);
			OverrideExporter(ClassIDType.Rigidbody2D, YamlExporter);
			OverrideExporter(ClassIDType.Collider2D, YamlExporter);
			OverrideExporter(ClassIDType.Rigidbody, YamlExporter);
			OverrideExporter(ClassIDType.PhysicsManager, YamlExporter);
			OverrideExporter(ClassIDType.CircleCollider2D, YamlExporter);
			OverrideExporter(ClassIDType.PolygonCollider2D, YamlExporter);
			OverrideExporter(ClassIDType.BoxCollider2D, YamlExporter);
			OverrideExporter(ClassIDType.PhysicsMaterial2D, YamlExporter);
			OverrideExporter(ClassIDType.MeshCollider, YamlExporter);
			OverrideExporter(ClassIDType.BoxCollider, YamlExporter);
			OverrideExporter(ClassIDType.CompositeCollider2D, YamlExporter);
			OverrideExporter(ClassIDType.EdgeCollider2D, YamlExporter);
			OverrideExporter(ClassIDType.CapsuleCollider2D, YamlExporter);
			OverrideExporter(ClassIDType.AnimationClip, YamlExporter);
			OverrideExporter(ClassIDType.TagManager, YamlExporter);
			OverrideExporter(ClassIDType.AudioListener, YamlExporter);
			OverrideExporter(ClassIDType.AudioSource, YamlExporter);
			OverrideExporter(ClassIDType.RenderTexture, YamlExporter);
			OverrideExporter(ClassIDType.Avatar, YamlExporter);
			OverrideExporter(ClassIDType.AnimatorController, YamlExporter);
			OverrideExporter(ClassIDType.GUILayer, YamlExporter);
			OverrideExporter(ClassIDType.Animator, YamlExporter);
			OverrideExporter(ClassIDType.RenderSettings, YamlExporter);
			OverrideExporter(ClassIDType.Light, YamlExporter);
			OverrideExporter(ClassIDType.Animation, YamlExporter);
			OverrideExporter(ClassIDType.MonoBehaviour, YamlExporter);
			OverrideExporter(ClassIDType.Texture3D, YamlExporter);
			OverrideExporter(ClassIDType.FlareLayer, YamlExporter);
			OverrideExporter(ClassIDType.NavMeshAreas, YamlExporter);
			OverrideExporter(ClassIDType.PhysicMaterial, YamlExporter);
			OverrideExporter(ClassIDType.SphereCollider, YamlExporter);
			OverrideExporter(ClassIDType.CapsuleCollider, YamlExporter);
			OverrideExporter(ClassIDType.SkinnedMeshRenderer, YamlExporter);
			OverrideExporter(ClassIDType.BuildSettings, YamlExporter);
			OverrideExporter(ClassIDType.WheelCollider, YamlExporter);
			OverrideExporter(ClassIDType.NetworkManager, YamlExporter);
			OverrideExporter(ClassIDType.TerrainCollider, YamlExporter);
			OverrideExporter(ClassIDType.TerrainData, YamlExporter);
			OverrideExporter(ClassIDType.OcclusionArea, YamlExporter);
			OverrideExporter(ClassIDType.LightmapSettings, YamlExporter);
			OverrideExporter(ClassIDType.AudioReverbZone, YamlExporter);
			OverrideExporter(ClassIDType.NavMeshSettings, YamlExporter);
			OverrideExporter(ClassIDType.ParticleSystem, YamlExporter);
			OverrideExporter(ClassIDType.ParticleSystemRenderer, YamlExporter);
			OverrideExporter(ClassIDType.ShaderVariantCollection, YamlExporter);
			OverrideExporter(ClassIDType.SpriteRenderer, YamlExporter);
			OverrideExporter(ClassIDType.Terrain, YamlExporter);
			OverrideExporter(ClassIDType.AnimatorOverrideController, YamlExporter);
			OverrideExporter(ClassIDType.CanvasRenderer, YamlExporter);
			OverrideExporter(ClassIDType.Canvas, YamlExporter);
			OverrideExporter(ClassIDType.RectTransform, YamlExporter);
			OverrideExporter(ClassIDType.ClusterInputManager, YamlExporter);
			OverrideExporter(ClassIDType.NavMeshData, YamlExporter);
			OverrideExporter(ClassIDType.UnityConnectSettings, YamlExporter);
			OverrideExporter(ClassIDType.ParticleSystemForceField, YamlExporter);
			OverrideExporter(ClassIDType.OcclusionCullingData, YamlExporter);
			OverrideExporter(ClassIDType.Prefab, YamlExporter);
			OverrideExporter(ClassIDType.AvatarMask, YamlExporter);
			OverrideExporter(ClassIDType.SceneAsset, YamlExporter);
			OverrideExporter(ClassIDType.LightmapParameters, YamlExporter);
			OverrideExporter(ClassIDType.SpriteAtlas, YamlExporter);

			OverrideExporter(ClassIDType.Texture2D, BinExporter);
			OverrideExporter(ClassIDType.Shader, BinExporter);
			OverrideExporter(ClassIDType.TextAsset, BinExporter);
			OverrideExporter(ClassIDType.AudioClip, BinExporter);
			OverrideExporter(ClassIDType.Cubemap, BinExporter);
			OverrideExporter(ClassIDType.Font, BinExporter);
			OverrideExporter(ClassIDType.MovieTexture, BinExporter);

			OverrideExporter(ClassIDType.MonoScript, ScriptExporter);
		}

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

		public void Export(string path, FileCollection fileCollection, Object asset)
		{
			Export(path, fileCollection, new Object[] { asset });
		}

		public void Export(string path, FileCollection fileCollection, IEnumerable<Object> assets)
		{
			VirtualSerializedFile virtualFile = new VirtualSerializedFile();
			List<IExportCollection> collections = new List<IExportCollection>();
			// speed up fetching a little bit
			List<Object> depList = new List<Object>();
			HashSet<Object> depSet = new HashSet<Object>();
			HashSet<Object> queued = new HashSet<Object>();
			depList.AddRange(assets);
			depSet.UnionWith(depList);
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

#warning TODO: if IsGenerateGUIDByContent set it should build collections and write actual references with persistent GUIS, but skip dependencies
				if (Config.IsExportDependencies)
				{
					foreach (Object dependency in asset.FetchDependencies(true))
					{
						if (dependency == null)
						{
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

			ProjectAssetContainer container = new ProjectAssetContainer(this, fileCollection.FetchAssets(), virtualFile, collections);
			foreach (IExportCollection collection in collections)
			{
				container.CurrentCollection = collection;
				bool isExported = collection.Export(container, path);
				if (isExported)
				{
					Logger.Log(LogType.Info, LogCategory.Export, $"'{collection.Name}' exported");
				}
			}
		}

		public AssetType ToExportType(ClassIDType classID)
		{
			switch (classID)
			{
				// abstract objects
				case ClassIDType.Object:
					return AssetType.Meta;
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
					if (asset.IsValid)
					{
						return exporter.CreateCollection(file, asset);
					}
					else
					{
						Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{asset}' because it isn't valid");
						return new SkipExportCollection(exporter, asset);
					}
				}
			}
			throw new Exception($"There is no exporter that can handle '{asset}'");
		}

		public YAMLAssetExporter YamlExporter { get; } = new YAMLAssetExporter();
		public BinaryAssetExporter BinExporter { get; } = new BinaryAssetExporter();

		internal DummyAssetExporter DummyExporter { get; } = new DummyAssetExporter();
		internal ScriptAssetExporter ScriptExporter { get; } = new ScriptAssetExporter();

		private readonly Dictionary<ClassIDType, Stack<IAssetExporter>> m_exporters = new Dictionary<ClassIDType, Stack<IAssetExporter>>();

		private readonly IFileCollection m_fileCollection;
	}
}