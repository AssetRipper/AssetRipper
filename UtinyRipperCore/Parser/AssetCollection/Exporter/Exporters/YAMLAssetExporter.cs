using System.Collections.Generic;
using System.IO;
using UtinyRipper.Classes;
using UtinyRipper.Exporter.YAML;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	public class YAMLAssetExporter : IAssetExporter
	{
		public void Export(ProjectAssetContainer container, Object asset, string path)
		{
			using (FileStream fileStream = File.Open(path, FileMode.Create, FileAccess.Write))
			{
				using (StreamWriter streamWriter = new StreamWriter(fileStream))
				{
					YAMLWriter writer = new YAMLWriter();
					container.File = asset.File;
					YAMLDocument doc = asset.ExportYAMLDocument(container);
					writer.AddDocument(doc);
					writer.Write(streamWriter);
				}
			}
		}

		public void Export(ProjectAssetContainer container, IEnumerable<Object> assets, string path)
		{
			using (FileStream fileStream = File.Open(path, FileMode.Create, FileAccess.Write))
			{
				using (StreamWriter streamWriter = new StreamWriter(fileStream))
				{
					YAMLWriter writer = new YAMLWriter();
					foreach (Object asset in assets)
					{
						container.File = asset.File;
						YAMLDocument doc = asset.ExportYAMLDocument(container);
						writer.AddDocument(doc);
					}
					writer.Write(streamWriter);
				}
			}
		}

		public IExportCollection CreateCollection(Object asset)
		{
			if(Prefab.IsCompatible(asset))
			{
				if (asset.File.IsScene)
				{
					return new SceneExportCollection(this, asset.File);
				}
				else
				{
					return new PrefabExportCollection(this, asset);
				}
			}
			else
			{
				switch (asset.ClassID)
				{
					case ClassIDType.NavMeshData:
						return new EmptyExportCollection(this);

					default:
						return new AssetExportCollection(this, asset);
				}
			}

		}

		public AssetType ToExportType(ClassIDType classID)
		{
			return AssetType.Serialized;
		}

		private IEnumerable<EditorExtension> EnumeratePrefabContent(Prefab prefab)
		{
			foreach(EditorExtension @object in prefab.FetchObjects())
			{
				if(@object.ClassID == ClassIDType.GameObject)
				{
					GameObject go = (GameObject)@object;
					int depth = go.GetRootDepth();
					@object.ObjectHideFlags = depth > 1 ? 1u : 0u;
				}
				else
				{
					@object.ObjectHideFlags = 1;
				}
				@object.PrefabInternal = prefab.ThisPrefab;
				yield return @object;
			}
		}
	}
}
