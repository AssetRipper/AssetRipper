using System;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

using DateTime = System.DateTime;

namespace uTinyRipper.Project.Classes
{
	public struct Meta
	{
		public Meta(IAssetImporter importer, GUID guid)
		{
			if (importer == null)
			{
				throw new ArgumentNullException(nameof(importer));
			}
			if (guid.IsZero)
			{
				throw new ArgumentNullException(nameof(guid));
			}

			m_importer = importer;
			m_guid = guid;
		}

		private static int GetFileFormatVersion(Version version)
		{
			// TODO:
			return 2;
		}

		public YAMLDocument ExportYAMLDocument(IExportContainer container)
		{
			YAMLDocument document = new YAMLDocument();
			YAMLMappingNode root = document.CreateMappingRoot();
			root.Add("fileFormatVersion", GetFileFormatVersion(container.ExportVersion));
			root.Add("guid", m_guid.ExportYAML(container));
			long cplusTick = (DateTime.Now.Ticks - 0x089f7ff5f7b58000) / 10000000;
			root.Add("timeCreated", cplusTick);
			root.Add("licenseType", "Free");			
			root.Add(m_importer.Name, m_importer.ExportYAML(container));

			return document;
		}

		private readonly IAssetImporter m_importer;
		private readonly GUID m_guid;
	}
}
