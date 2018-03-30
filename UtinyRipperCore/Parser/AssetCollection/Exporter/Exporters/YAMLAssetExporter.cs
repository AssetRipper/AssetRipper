using System;
using System.Collections.Generic;
using System.IO;
using UtinyRipper.Classes;
using UtinyRipper.Exporter.YAML;
using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	public class YAMLAssetExporter : AssetExporter
	{
		public override IExportCollection CreateCollection(UtinyRipper.Classes.Object @object)
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
				PrefabExportCollection collection = new PrefabExportCollection(this, prefab, prefabContent);
				return collection;
			}
			else
			{
				if (!@object.IsAsset)
				{
					throw new ArgumentException($"Unsupported export object type {@object.ClassID}", nameof(@object));
				}
				
				AssetExportCollection collection = new AssetExportCollection(this, @object);
				return collection;
			}
		}

		public override bool Export(IAssetsExporter exporter, IExportCollection collection, string dirPath)
		{
			AssetExportCollection asset = (AssetExportCollection)collection;
			string subFolder = asset.Asset.ClassID.ToString();
			string subPath = Path.Combine(dirPath, subFolder);
			string fileName = GetUniqueFileName(asset.Asset, subPath);
			string filePath = Path.Combine(subPath, fileName);

			if (!Directory.Exists(subPath))
			{
				Directory.CreateDirectory(subPath);
			}

			if(asset is PrefabExportCollection prefab)
			{
				ExportYAML(exporter, prefab.Objects, filePath);
			}
			else
			{
				ExportYAML(exporter, asset.Asset, filePath);
			}

			exporter.File = asset.Asset.File;
			ExportMeta(exporter, asset, filePath);

			return true;
		}

		public override AssetType ToExportType(ClassIDType classID)
		{
			switch (classID)
			{
				case ClassIDType.Material:
				case ClassIDType.Mesh:
				case ClassIDType.AnimationClip:
				case ClassIDType.Avatar:
				case ClassIDType.AnimatorOverrideController:
					return AssetType.Serialized;

				default:
					throw new NotSupportedException();
			}
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

		private void ExportYAML(IAssetsExporter exporter, UtinyRipper.Classes.Object asset, string path)
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

		private void ExportYAML(IAssetsExporter exporter, IEnumerable<UtinyRipper.Classes.Object> objects, string path)
		{
			using (FileStream fileStream = File.Open(path, FileMode.Create, FileAccess.Write))
			{
				using (StreamWriter streamWriter = new StreamWriter(fileStream))
				{
					YAMLWriter writer = new YAMLWriter();
					foreach (UtinyRipper.Classes.Object @object in objects)
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
