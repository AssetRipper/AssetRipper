using System;
using System.Collections.Generic;
using System.IO;
using UtinyRipper.Classes;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;
using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	public class YAMLAssetExporter : IAssetExporter
	{
		public bool IsHandle(Object asset)
		{
			return true;
		}

		public void Export(IExportContainer container, Object asset, string path)
		{
			Export(container, asset, path, null);
		}
		
		public void Export(IExportContainer container, Object asset, string path, Action<IExportContainer, Object, string> callback)
		{
			using (FileStream fileStream = FileUtils.Open(path, FileMode.Create, FileAccess.Write))
			{
				using (StreamWriter streamWriter = new StreamWriter(fileStream))
				{
					YAMLWriter writer = new YAMLWriter();
					YAMLDocument doc = asset.ExportYAMLDocument(container);
					writer.AddDocument(doc);
					writer.Write(streamWriter);
				}
			}
			callback?.Invoke(container, asset, path);
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string path)
		{
			using (FileStream fileStream = FileUtils.Open(path, FileMode.Create, FileAccess.Write))
			{
				using (StreamWriter streamWriter = new StreamWriter(fileStream))
				{
					YAMLWriter writer = new YAMLWriter();
					writer.WriteHead(streamWriter);
					foreach (Object asset in assets)
					{
						YAMLDocument doc = asset.ExportYAMLDocument(container);
						writer.WriteDocument(doc);
					}
					writer.WriteTail(streamWriter);
				}
			}
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string path, Action<IExportContainer, Object, string> callback)
		{
			throw new NotSupportedException();
		}

		public IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			if (OcclusionCullingSettings.IsCompatible(asset))
			{
				if (asset.File.Collection.IsScene(asset.File))
				{
					return new SceneExportCollection(this, virtualFile, asset.File);
				}
				else
				{
					return new PrefabExportCollection(this, virtualFile, asset);
				}
			}
			else
			{
				switch (asset.ClassID)
				{
					case ClassIDType.NavMeshData:
						return new EmptyExportCollection();
					case ClassIDType.AnimatorController:
						return new AnimatorControllerExportCollection(this, virtualFile, asset);

					case ClassIDType.AudioManager:
					case ClassIDType.GraphicsSettings:
					case ClassIDType.PhysicsManager:
					case ClassIDType.TagManager:
					case ClassIDType.ClusterInputManager:
						return new ManagerExportCollection(this, asset);
					case ClassIDType.BuildSettings:
						return new BuildSettingsExportCollection(this, virtualFile, asset);

					default:
						return new AssetExportCollection(this, asset);
				}
			}
		}

		public AssetType ToExportType(Object asset)
		{
			ToUnknownExportType(asset.ClassID, out AssetType assetType);
			return assetType;
		}

		public bool ToUnknownExportType(ClassIDType classID, out AssetType assetType)
		{
			assetType = AssetType.Serialized;
			return true;
		}
	}
}
