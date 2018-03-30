using System;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.AssetExporters.Classes
{
	public class Meta : IYAMLDocExportable
	{
		public Meta(IExportCollection collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}
			m_collection = collection;
		}

		private static int GetFileFormatVersion(Version version)
		{
#warning TODO: file version acording to read version (current 2017.3.0f3)
			return 2;
		}

		public YAMLDocument ExportYAMLDocument(IAssetsExporter exporter)
		{
			YAMLDocument document = new YAMLDocument();
			YAMLMappingNode root = document.CreateMappingRoot();
			root.Add("fileFormatVersion", GetFileFormatVersion(exporter.Version));
			root.Add("guid", m_collection.GUID.ExportYAML(exporter));
			long cplusTick = (DateTime.Now.Ticks - 0x089f7ff5f7b58000) / 10000000;
			root.Add("timeCreated", cplusTick);
			root.Add("licenseType", "Free");			
			root.Add("NativeFormatImporter", m_collection.MetaImporter.ExportYAML(exporter));

			return document;
		}

		private readonly IExportCollection m_collection;
	}
}
