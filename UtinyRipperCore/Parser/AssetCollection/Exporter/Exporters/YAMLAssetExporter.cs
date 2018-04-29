using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UtinyRipper.Classes;
using UtinyRipper.Exporter.YAML;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	public class YAMLAssetExporter : AssetExporter
	{
		public override IExportCollection CreateCollection(Object @object)
		{
			if(@object.File.IsScene)
			{
				return new SceneExportCollection(this, @object.File.Name, @object.File.FetchAssets().Where(t => !t.ClassID.IsAsset()));
			}
			else
			{
				if (@object is Component comp)
				{
					@object = comp.GameObject.GetObject(comp.File);
				}
				if (@object.ClassID == ClassIDType.GameObject)
				{
					GameObject go = (GameObject)@object;
					@object = go.GetRoot();
				}

				if (@object.ClassID == ClassIDType.GameObject)
				{
					GameObject go = (GameObject)@object;
					Prefab prefab = new Prefab(go);
					IEnumerable<EditorExtension> prefabContent = EnumeratePrefabContent(prefab);
					return new PrefabExportCollection(this, prefab, prefabContent);
				}
				else
				{
					if (!@object.IsAsset)
					{
						throw new ArgumentException($"Unsupported export object type {@object.ClassID}", nameof(@object));
					}

					return new AssetExportCollection(this, @object);
				}
			}
		}

		public override bool Export(IAssetsExporter exporter, IExportCollection collection, string dirPath)
		{
			switch(collection)
			{
#warning TODO: make universal!
				case SceneExportCollection scene:
					{
						string subFolder = "Scenes";
						string subPath = Path.Combine(dirPath, subFolder);
						string fileName = $"{scene.Name}.unity";
						string filePath = Path.Combine(subPath, fileName);

						if (!Directory.Exists(subPath))
						{
							Directory.CreateDirectory(subPath);
						}

						ExportYAML(exporter, scene.Objects, filePath);
						ExportMeta(exporter, scene, filePath);
					}
					break;

				case AssetExportCollection asset:
					{
						string subFolder = asset.Asset.ClassID.ToString();
						string subPath = Path.Combine(dirPath, subFolder);
						string fileName = GetUniqueFileName(asset.Asset, subPath);
						string filePath = Path.Combine(subPath, fileName);

						if (!Directory.Exists(subPath))
						{
							Directory.CreateDirectory(subPath);
						}

						if (asset is PrefabExportCollection prefab)
						{
							ExportYAML(exporter, prefab.Objects, filePath);
						}
						else
						{
							ExportYAML(exporter, asset.Asset, filePath);
						}

						exporter.File = asset.Asset.File;
						ExportMeta(exporter, asset, filePath);
					}
					break;

				default:
					throw new NotSupportedException(collection.GetType().Name);
			}

			return true;
		}

		public override AssetType ToExportType(ClassIDType classID)
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

		private void ExportYAML(IAssetsExporter exporter, Object asset, string path)
		{
			using (FileStream fileStream = File.Open(path, FileMode.Create, FileAccess.Write))
			{
				using (StreamWriter streamWriter = new StreamWriter(fileStream))
				{
					YAMLWriter writer = new YAMLWriter();
					exporter.File = asset.File;
					YAMLDocument doc = asset.ExportYAMLDocument(exporter);
					writer.AddDocument(doc);
					writer.Write(streamWriter);
				}
			}
		}

		private void ExportYAML(IAssetsExporter exporter, IEnumerable<Object> objects, string path)
		{
			using (FileStream fileStream = File.Open(path, FileMode.Create, FileAccess.Write))
			{
				using (StreamWriter streamWriter = new StreamWriter(fileStream))
				{
					YAMLWriter writer = new YAMLWriter();
					foreach (Object @object in objects)
					{
						exporter.File = @object.File;
						YAMLDocument doc = @object.ExportYAMLDocument(exporter);
						writer.AddDocument(doc);
					}
					writer.Write(streamWriter);
				}
			}
		}
	}
}
