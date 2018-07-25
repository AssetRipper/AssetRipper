using System.Collections.Generic;
using System.IO;
using UtinyRipper.Classes;
using UtinyRipper.Exporter.YAML;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	public class YAMLAssetExporter : IAssetExporter
	{
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
					foreach (Object asset in assets)
					{
						YAMLDocument doc = asset.ExportYAMLDocument(container);
						writer.AddDocument(doc);
					}
					writer.Write(streamWriter);
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

		public AssetType ToExportType(ClassIDType classID)
		{
			return AssetType.Serialized;
		}
	}
}
