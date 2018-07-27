using System.Collections.Generic;
using System.IO;
using UtinyRipper.Classes;
using UtinyRipper.Exporter.YAML;

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
			using (FileStream fileStream = File.Open(path, FileMode.Create, FileAccess.Write))
			{
				using (StreamWriter streamWriter = new StreamWriter(fileStream))
				{
					YAMLWriter writer = new YAMLWriter();
					YAMLDocument doc = asset.ExportYAMLDocument(container);
					writer.AddDocument(doc);
					writer.Write(streamWriter);
				}
			}
		}

		public void Export(IExportContainer container, IEnumerable<Object> assets, string path)
		{
			using (FileStream fileStream = File.Open(path, FileMode.Create, FileAccess.Write))
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

		public IExportCollection CreateCollection(Object asset)
		{
			if (OcclusionCullingSettings.IsCompatible(asset))
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
						return new EmptyExportCollection();
					case ClassIDType.AnimatorController:
						return new AnimatorControllerExportCollection(this, asset);

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
