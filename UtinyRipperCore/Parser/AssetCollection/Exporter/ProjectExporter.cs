using System;
using System.Collections.Generic;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	public class ProjectExporter
	{
		public ProjectExporter(IFileCollection fileCollection)
		{
			m_fileCollection = fileCollection;

			DummyAssetExporter dummyExporter = new DummyAssetExporter();
			OverrideExporter(ClassIDType.AnimatorController, dummyExporter);
			OverrideExporter(ClassIDType.MonoBehaviour, dummyExporter);
			OverrideExporter(ClassIDType.MonoScript, dummyExporter);
			OverrideExporter(ClassIDType.MonoManager, dummyExporter);
			OverrideExporter(ClassIDType.BuildSettings, dummyExporter);
			OverrideExporter(ClassIDType.AssetBundle, dummyExporter);
			OverrideExporter(ClassIDType.PreloadData, dummyExporter);
			OverrideExporter(ClassIDType.Sprite, dummyExporter);
			OverrideExporter(ClassIDType.SpriteAtlas, dummyExporter);

			YAMLAssetExporter yamlExporter = new YAMLAssetExporter();
			OverrideExporter(ClassIDType.GameObject, yamlExporter);
			OverrideExporter(ClassIDType.Transform, yamlExporter);
			OverrideExporter(ClassIDType.Camera, yamlExporter);
			OverrideExporter(ClassIDType.Material, yamlExporter);
			OverrideExporter(ClassIDType.MeshRenderer, yamlExporter);
			OverrideExporter(ClassIDType.OcclusionCullingSettings, yamlExporter);
			OverrideExporter(ClassIDType.MeshFilter, yamlExporter);
			OverrideExporter(ClassIDType.OcclusionPortal, yamlExporter);
			OverrideExporter(ClassIDType.Mesh, yamlExporter);
			OverrideExporter(ClassIDType.Rigidbody2D, yamlExporter);
			OverrideExporter(ClassIDType.Rigidbody, yamlExporter);
			OverrideExporter(ClassIDType.CircleCollider2D, yamlExporter);
			OverrideExporter(ClassIDType.PolygonCollider2D, yamlExporter);
			OverrideExporter(ClassIDType.BoxCollider2D, yamlExporter);
			OverrideExporter(ClassIDType.PhysicsMaterial2D, yamlExporter);
			OverrideExporter(ClassIDType.MeshCollider, yamlExporter);
			OverrideExporter(ClassIDType.BoxCollider, yamlExporter);
			OverrideExporter(ClassIDType.CompositeCollider2D, yamlExporter);
			OverrideExporter(ClassIDType.EdgeCollider2D, yamlExporter);
			OverrideExporter(ClassIDType.CapsuleCollider2D, yamlExporter);
			OverrideExporter(ClassIDType.AnimationClip, yamlExporter);
			OverrideExporter(ClassIDType.AudioListener, yamlExporter);
			OverrideExporter(ClassIDType.AudioSource, yamlExporter);
			OverrideExporter(ClassIDType.RenderTexture, yamlExporter);
			OverrideExporter(ClassIDType.Avatar, yamlExporter);
			OverrideExporter(ClassIDType.AnimatorController, yamlExporter);
			OverrideExporter(ClassIDType.GUILayer, yamlExporter);
			OverrideExporter(ClassIDType.Animator, yamlExporter);
			OverrideExporter(ClassIDType.RenderSettings, yamlExporter);
			OverrideExporter(ClassIDType.Light, yamlExporter);
			OverrideExporter(ClassIDType.Animation, yamlExporter);
			OverrideExporter(ClassIDType.FlareLayer, yamlExporter);
			OverrideExporter(ClassIDType.PhysicMaterial, yamlExporter);
			OverrideExporter(ClassIDType.SphereCollider, yamlExporter);
			OverrideExporter(ClassIDType.CapsuleCollider, yamlExporter);
			OverrideExporter(ClassIDType.SkinnedMeshRenderer, yamlExporter);
			OverrideExporter(ClassIDType.WheelCollider, yamlExporter);
			OverrideExporter(ClassIDType.TerrainCollider, yamlExporter);
			OverrideExporter(ClassIDType.TerrainData, yamlExporter);
			OverrideExporter(ClassIDType.OcclusionArea, yamlExporter);
			OverrideExporter(ClassIDType.LightmapSettings, yamlExporter);
			OverrideExporter(ClassIDType.NavMeshSettings, yamlExporter);
			OverrideExporter(ClassIDType.ParticleSystem, yamlExporter);
			OverrideExporter(ClassIDType.ParticleSystemRenderer, yamlExporter);
			OverrideExporter(ClassIDType.SpriteRenderer, yamlExporter);
			OverrideExporter(ClassIDType.Terrain, yamlExporter);
			OverrideExporter(ClassIDType.AnimatorOverrideController, yamlExporter);
			OverrideExporter(ClassIDType.CanvasRenderer, yamlExporter);
			OverrideExporter(ClassIDType.Canvas, yamlExporter);
			OverrideExporter(ClassIDType.RectTransform, yamlExporter);
			OverrideExporter(ClassIDType.NavMeshData, yamlExporter);
			OverrideExporter(ClassIDType.OcclusionCullingData, yamlExporter);
			OverrideExporter(ClassIDType.Prefab, yamlExporter);
			OverrideExporter(ClassIDType.AvatarMask, yamlExporter);
			OverrideExporter(ClassIDType.SceneAsset, yamlExporter);
			OverrideExporter(ClassIDType.LightmapParameters, yamlExporter);

			BinaryAssetExporter binExporter = new BinaryAssetExporter();
			OverrideExporter(ClassIDType.Texture2D, binExporter);
			OverrideExporter(ClassIDType.Shader, binExporter);
			OverrideExporter(ClassIDType.TextAsset, binExporter);
			OverrideExporter(ClassIDType.AudioClip, binExporter);
			OverrideExporter(ClassIDType.Cubemap, binExporter);
			OverrideExporter(ClassIDType.Font, binExporter);
			OverrideExporter(ClassIDType.MovieTexture, binExporter);
		}

		public void OverrideExporter(ClassIDType classType, IAssetExporter exporter)
		{
			if (exporter == null)
			{
				throw new ArgumentNullException(nameof(exporter));
			}
			m_exporters[classType] = exporter;
		}

		public void Export(string path, Object @object)
		{
			Export(path, ToIEnumerable(@object));
		}

		public void Export(string path, IEnumerable<Object> objects)
		{
			List<IExportCollection> collections = new List<IExportCollection>();
			// speed up fetching a little bit
			List<Object> depList = new List<Object>();
			HashSet<Object> depSet = new HashSet<Object>();
			HashSet<Object> queued = new HashSet<Object>();
			depList.AddRange(objects);
			depSet.UnionWith(depList);
			for (int i = 0; i < depList.Count; i++)
			{
				Object current = depList[i];
				if (!queued.Contains(current))
				{
					IAssetExporter exporter = m_exporters[current.ClassID];
					IExportCollection collection = exporter.CreateCollection(current);

					foreach (Object element in collection.Objects)
					{
						queued.Add(element);
					}
					collections.Add(collection);
				}

#warning TODO: if IsGenerateGUIDByContent set it should build collections and write actual references with persistent GUIS, but skip dependencies
				if (Config.IsExportDependencies)
				{
					foreach (Object dependency in current.FetchDependencies(true))
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
			
			ProjectAssetContainer container = new ProjectAssetContainer(this, collections);
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
					return m_exporters[ClassIDType.Texture2D].ToExportType(ClassIDType.Texture2D);
				case ClassIDType.RuntimeAnimatorController:
					return m_exporters[ClassIDType.AnimatorController].ToExportType(ClassIDType.AnimatorController);
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
			return m_exporters[classID].ToExportType(classID);
		}

		private IEnumerable<Object> ToIEnumerable(Object @object)
		{
			yield return @object;
		}

		private readonly Dictionary<ClassIDType, IAssetExporter> m_exporters = new Dictionary<ClassIDType, IAssetExporter>();

		private readonly IFileCollection m_fileCollection;
	}
}