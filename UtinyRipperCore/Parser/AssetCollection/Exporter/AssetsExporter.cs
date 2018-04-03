using System;
using System.Collections.Generic;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	public class AssetsExporter
	{
		public AssetsExporter()
		{
			DummyAssetExporter dummyExporter = new DummyAssetExporter();
			OverrideExporter(ClassIDType.AnimatorController, dummyExporter);
			OverrideExporter(ClassIDType.MonoScript, dummyExporter);
			OverrideExporter(ClassIDType.BuildSettings, dummyExporter);
			OverrideExporter(ClassIDType.AssetBundle, dummyExporter);
			OverrideExporter(ClassIDType.Sprite, dummyExporter);
			OverrideExporter(ClassIDType.SpriteAtlas, dummyExporter);

			YAMLAssetExporter yamlExporter = new YAMLAssetExporter();
			OverrideExporter(ClassIDType.Prefab, yamlExporter);
			OverrideExporter(ClassIDType.Component, yamlExporter);
			OverrideExporter(ClassIDType.GameObject, yamlExporter);
			OverrideExporter(ClassIDType.Material, yamlExporter);
			OverrideExporter(ClassIDType.Mesh, yamlExporter);
			OverrideExporter(ClassIDType.Rigidbody2D, yamlExporter);
			OverrideExporter(ClassIDType.Rigidbody, yamlExporter);
			OverrideExporter(ClassIDType.CircleCollider2D, yamlExporter);
			OverrideExporter(ClassIDType.PolygonCollider2D, yamlExporter);
			OverrideExporter(ClassIDType.BoxCollider2D, yamlExporter);
			OverrideExporter(ClassIDType.PhysicsMaterial2D, yamlExporter);
			OverrideExporter(ClassIDType.MeshCollider, yamlExporter);
			OverrideExporter(ClassIDType.BoxCollider, yamlExporter);
			OverrideExporter(ClassIDType.SpriteCollider2D, yamlExporter);
			OverrideExporter(ClassIDType.EdgeCollider2D, yamlExporter);
			OverrideExporter(ClassIDType.CapsuleCollider2D, yamlExporter);
			OverrideExporter(ClassIDType.AnimationClip, yamlExporter);
			OverrideExporter(ClassIDType.Avatar, yamlExporter);
			OverrideExporter(ClassIDType.Light, yamlExporter);
			OverrideExporter(ClassIDType.PhysicMaterial, yamlExporter);
			OverrideExporter(ClassIDType.SphereCollider, yamlExporter);
			OverrideExporter(ClassIDType.CapsuleCollider, yamlExporter);
			OverrideExporter(ClassIDType.WheelCollider, yamlExporter);
			OverrideExporter(ClassIDType.TerrainCollider, yamlExporter);
			OverrideExporter(ClassIDType.TerrainData, yamlExporter);
			OverrideExporter(ClassIDType.ParticleSystem, yamlExporter);
			OverrideExporter(ClassIDType.ParticleSystemRenderer, yamlExporter);
			OverrideExporter(ClassIDType.SpriteRenderer, yamlExporter);
			OverrideExporter(ClassIDType.Terrain, yamlExporter);
			OverrideExporter(ClassIDType.AnimatorOverrideController, yamlExporter);
			OverrideExporter(ClassIDType.CanvasRenderer, yamlExporter);
			OverrideExporter(ClassIDType.Canvas, yamlExporter);
			OverrideExporter(ClassIDType.AvatarMask, yamlExporter);

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

		public void Export(string path, UtinyRipper.Classes.Object @object)
		{
			Export(path, ToIEnumerable(@object));
		}

		public void Export(string path, IEnumerable<UtinyRipper.Classes.Object> objects)
		{
			List<IExportCollection> collections = new List<IExportCollection>();
			// speed up fetching a little bit
			List<UtinyRipper.Classes.Object> depList = new List<UtinyRipper.Classes.Object>();
			HashSet<UtinyRipper.Classes.Object> depSet = new HashSet<UtinyRipper.Classes.Object>();
			HashSet<UtinyRipper.Classes.Object> queued = new HashSet<UtinyRipper.Classes.Object>();
			depList.AddRange(objects);
			depSet.UnionWith(depList);
			for (int i = 0; i < depList.Count; i++)
			{
				UtinyRipper.Classes.Object current = depList[i];
				if (!queued.Contains(current))
				{
					ClassIDType exportID = current.IsAsset ? current.ClassID : ClassIDType.Component;
					IAssetExporter exporter = m_exporters[exportID];
					IExportCollection collection = exporter.CreateCollection(current);

					foreach (UtinyRipper.Classes.Object element in collection.Objects)
					{
						queued.Add(element);
					}
					collections.Add(collection);
				}

#warning TODO: if IsGenerateGUIDByContent set it should build collections and write actual references with persistent GUIS, but skip dependencies
				if (Config.IsExportDependencies)
				{
					foreach (UtinyRipper.Classes.Object dep in current.FetchDependencies(true))
					{
						if (!depSet.Contains(dep))
						{
							depList.Add(dep);
							depSet.Add(dep);
						}
					}
				}
			}
			depList.Clear();
			depSet.Clear();
			queued.Clear();
			
			AssetsExportContainer container = new AssetsExportContainer(this, collections);
			foreach (IExportCollection collection in collections)
			{
				container.CurrentCollection = collection;
				bool isExported = collection.AssetExporter.Export(container, collection, path);
				if (isExported)
				{
					Logger.Log(LogType.Info, LogCategory.Export, $"'{collection.Name}' exported");
				}
			}
		}

		public AssetType ToExportType(ClassIDType classID)
		{
			// abstract objects
			switch (classID)
			{
				case ClassIDType.Object:
					return AssetType.Meta;

				case ClassIDType.Texture:
					return AssetType.Meta;

				case ClassIDType.RuntimeAnimatorController:
					return AssetType.Serialized;
					
					// not implemented yet
				case ClassIDType.Flare:
					return AssetType.Serialized;
				case ClassIDType.Camera:
					return AssetType.Serialized;
			}

			if (!m_exporters.ContainsKey(classID))
			{
				throw new NotImplementedException($"Export type for class {classID} is undefined");
			}

			return m_exporters[classID].ToExportType(classID);
		}

		private IEnumerable<UtinyRipper.Classes.Object> ToIEnumerable(UtinyRipper.Classes.Object @object)
		{
			yield return @object;
		}

		private readonly Dictionary<ClassIDType, IAssetExporter> m_exporters = new Dictionary<ClassIDType, IAssetExporter>();
	}
}