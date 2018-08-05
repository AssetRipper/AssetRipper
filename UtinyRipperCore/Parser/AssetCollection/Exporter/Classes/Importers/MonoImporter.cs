using System;
using UtinyRipper.AssetExporters;
using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Importers
{
	public class MonoImporter : DefaultImporter
	{
		public MonoImporter(MonoScript sctipt)
		{
			if(sctipt == null)
			{
				throw new ArgumentNullException(nameof(sctipt));
			}
			m_script = sctipt;
		}

		private static int GetSerializedVersion(Version version)
		{
			if(Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

#warning TODO: value acording to read version (current 2017.3.0f3)
			return 2;
		}

		protected override void ExportYAMLInner(IExportContainer container, YAMLMappingNode node)
		{
#warning TODO: value acording to read version (current 2017.3.0f3)
			node.AddSerializedVersion(2);
			node.Add("defaultReferences", YAMLSequenceNode.Empty);
			node.Add("executionOrder", m_script.ExecutionOrder);

			YAMLMappingNode instanceId = new YAMLMappingNode(MappingStyle.Flow);
			instanceId.Add("instanceID", 0);
			node.Add("icon", instanceId);
		}

		public override string Name => "MonoImporter";

		private readonly MonoScript m_script;
	}
}
