using System.IO;
using System.Text.RegularExpressions;
using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;
using UtinyRipper.Exporter.YAML;
using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper.AssetExporters
{
	public abstract class AssetExporter : IAssetExporter
	{
		static AssetExporter()
		{
			string invalidChars = new string(Path.GetInvalidFileNameChars());
			string escapedChars = Regex.Escape(invalidChars);
			FileNameRegex = new Regex($"[{escapedChars}]");
		}

		public abstract IExportCollection CreateCollection(UtinyRipper.Classes.Object @object);
		public abstract bool Export(IAssetsExporter exporter, IExportCollection collection, string dirPath);
		public abstract AssetType ToExportType(ClassIDType classID);

		protected string GetUniqueFileName(UtinyRipper.Classes.Object @object, string dirPath)
		{
			string fileName;
			switch (@object)
			{
				case NamedObject named:
					fileName = named.Name;
					break;
				case Prefab prefab:
					fileName = prefab.GetName();
					break;
				default:
					fileName = @object.GetType().Name;
					break;
			}
			fileName = FixName(fileName);

			fileName = DirectoryUtils.GetMaxIndexName(dirPath, fileName);
			fileName = $"{fileName}.{@object.ExportExtension}";
			return fileName;
		}

		protected void ExportMeta(IAssetsExporter exporter, IExportCollection collection, string filePath)
		{
			Meta meta = new Meta(collection);
			string metaPath = $"{filePath}.meta";
			
			using (FileStream fileStream = File.Open(metaPath, FileMode.Create, FileAccess.Write))
			{
				using (StreamWriter streamWriter = new StreamWriter(fileStream))
				{
					YAMLWriter writer = new YAMLWriter();
					YAMLDocument doc = meta.ExportYAMLDocument(exporter);
					writer.IsWriteDefaultTag = false;
					writer.IsWriteVersion = false;
					writer.AddDocument(doc);
					writer.Write(streamWriter);
				}
			}
		}

		protected string FixName(string path)
		{
			return FileNameRegex.Replace(path, string.Empty);
		}

		private static readonly Regex FileNameRegex;
	}
}
